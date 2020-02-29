using log4net;
using Radar.Model;
using Radar.Common;
using Radar.IoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Radar.Common.Threads;
using Radar.Common.Raw;
using Radar.Bidding.Messages;
using Radar.Common.Enums;

namespace Radar.Bidding.Net
{

    [Component]
    public class SocketClient : IDisposable
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(SocketClient));

        private static readonly object SEND_LOCK = new object();

        private static readonly object START_LOCK = new object();

        private static readonly object WATCH_LOCK = new object();

        private string ip;
 
        private int port;

        //信息接收进程
        private Thread bytesReceiveThread = null;

        private Thread socketWatchThrad = null;

        // ManualResetEvent instances signal completion.  
        private ManualResetEvent sendDone;// = new ManualResetEvent(false);
        /// <summary>
        /// 1  - 连接中
        /// 10 - 正常
        /// 100 - 异常
        /// </summary>
        private ConnectStatusEnum connectStatus = ConnectStatusEnum.NONE;
        // private static ManualResetEvent receiveDone = new ManualResetEvent(false);

        // The response from the remote device.  
        private static String response = String.Empty;

        private Socket _client;
        private bool working = true;
        private bool watching = true;

        private bool socketStarting = false;

        private Func<RawMessage, String> anotherRecvCallback;

        private ProjectConfig conf;

        public Func<string, string> AfterSuccessConnected { get; set; }

        public bool EnableSocketGuard { get; set; }

        public SocketClient(string ip, int port)
        {
            this.ip = GetIP(ip);
            this.port = port;
        }

        public SocketClient(ProjectConfig conf)
        {
            var addr = conf.SaberServerAddress;
            this.ip = GetIP(addr.Split(':')[0]);
            this.port = int.Parse(addr.Split(':')[1]);

            this.conf = conf;
        }

        public string GetIP(string domain)
        {
            bool isIp = KK.IsIP(domain);
            logger.InfoFormat("domain#{0} IsIP = {1}", domain, isIp);

            if (isIp)
            {
                return domain;
            }

            IPHostEntry hostEntry = Dns.GetHostEntry(domain);
            IPEndPoint ipEndPoint = new IPEndPoint(hostEntry.AddressList[0], 0);
            return ipEndPoint.Address.ToString();
        }

        public void StartClient()
        {
            logger.InfoFormat("begin start socket-client with address#{0}:{1}", ip, port);

            socketStarting = true;
            working = true;
            connectStatus = ConnectStatusEnum.CONNECTING;
            try
            {
                IPAddress ipAddr = IPAddress.Parse(ip);
                IPEndPoint remoteEP = new IPEndPoint(ipAddr, port);

                // Create a TCP/IP socket.  
                _client = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                // Connect to the remote endpoint.  
                _client.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), _client);
                while (connectStatus == ConnectStatusEnum.CONNECTING)
                {
                    KK.Sleep(100);

                    if (connectStatus == ConnectStatusEnum.CONNECT_FAILED)
                    {
                        break;
                    }
                }
                socketStarting = false;
                logger.InfoFormat("connectStatus is {0}", connectStatus);

                logger.InfoFormat("end socket-client connect with address#{0}:{1}, _client#{2}", ip, port, _client);

                this.InvokeAfterSuccessConnected();
            }
            catch (Exception e)
            {
                logger.Error("StartClient error", e);
            }
            finally
            {
                socketStarting = false;
            }


            // 无论何种情况，都应开启守护
            if (EnableSocketGuard)
            {
                KeepAliveManager.ME.RestartGuard(this);
            }

        }

        private void InvokeAfterSuccessConnected()
        {
            if (connectStatus != ConnectStatusEnum.CONNECTED)
            {
                logger.ErrorFormat("connectStatus is not success, so DONOT Invoke AfterSuccessConnected");
                return;
            }

            // 初始化线程
            bytesReceiveThread = ThreadUtils.StartNewBackgroudThread(ReceiveForEver);

            try
            {
                AfterSuccessConnected?.Invoke("");
            }
            catch (Exception e1)
            {
                logger.Error("afterSuccessConnected Invoke error", e1);
            }

        }

        public void RestartClient()
        {
            logger.InfoFormat("begin RestartClient");
            lock (START_LOCK)
            {
                this.Shutdown();

                this.StartClient();
            }
            logger.InfoFormat("done RestartClient");
        }

        public void setAnotherRecvCallback(Func<RawMessage, string> anotherRecvCallback)
        {
            this.anotherRecvCallback = anotherRecvCallback;
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket client = (Socket) ar.AsyncState;

                // Complete the connection.  
                client.EndConnect(ar);

                logger.InfoFormat("Socket connected to {0}", client.RemoteEndPoint.ToString());

                connectStatus = ConnectStatusEnum.CONNECTED;

            }
            catch (Exception e)
            {
                logger.Error("ConnectCallback error.", e);
                connectStatus = ConnectStatusEnum.CONNECT_FAILED;
            }
        }

        private void ReceiveForEver()
        {
            while (working)
            {
                try
                {
                    // TODO: 这里只能读取一个buffer大小, 后续优化
                    byte[] bytes = new byte[1024 * 4];
                    int bytesRec = _client.Receive(bytes);
                    byte[] data = new byte[bytesRec];
                    ByteUtils.arraycopy(bytes, 0, data, 0, bytesRec);

                    RawMessage raw = RawMessageEncoder.me.decode(data);
                    if (raw.getMessageType() != (int) RawMessageType.PING_COMMAND)
                    {
                        logger.InfoFormat("receive data is {0}, {1}, {2}", raw.totalLength, raw.messageType, raw.bodyText);
                    }
                    
                    MessageDispatcher.ME.Dispatch(raw);

                    this.anotherRecvCallback?.Invoke(raw);

                    // logger.InfoFormat("Echoed test = {0}", Encoding.ASCII.GetString(bytes, 0, bytesRec));
                }
                catch (Exception e)
                {
                    // logger.ErrorFormat("Receive error", e.Message);
                    logger.Error("Receive error", e);

                    // TODO: 出现错误, 先临时处理 不再接收了 
                    working = false;

                    KK.Sleep(50);
                }
            }
        }

        private string DispatchCallback(RawMessage message)
        {
            Send(message);
            return "ok";
        }

        public void Send(RawMessage raw)
        { 
            if (raw == null)
            {
                return;
            }

            // TODO: 看起来不应该并发发送
            lock (SEND_LOCK)
            {
                sendDone = new ManualResetEvent(false);

                byte[] byteData = RawMessageEncoder.me.encode(raw);

                // Begin sending the data to the remote device.  
                _client.BeginSend(byteData, 0, byteData.Length, SocketFlags.None, new AsyncCallback(SendCallback), _client);

                // TODO: 这里可以等待发送完成在返回

            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                logger.DebugFormat("SendCallback in");

                // Retrieve the socket from the state object.  
                Socket client = (Socket) ar.AsyncState;

                // Complete sending the data to the remote device.  
                int bytesSent = client.EndSend(ar);

                logger.DebugFormat("Sent {0} bytes to server.", bytesSent);

                // Signal that all bytes have been sent.  
                sendDone.Set();
            }
            catch (Exception e)
            {
                logger.Error("SendCallback error", e);
            }
        }

        public void Shutdown()
        {
            if (_client == null)
            {
                logger.InfoFormat("_client already closed.");
                return;
            }

            working = false;
            // watching = false;
            ThreadUtils.TryStopThreadByWait(bytesReceiveThread, 200, 300, "BytesReceiveThread");

            try
            {
                _client.Shutdown(SocketShutdown.Both);
                logger.InfoFormat("_client.Shutdown done");
            } 
            catch (Exception e)
            {
                logger.Error("_client.Shutdown error", e);
            }
            
            try
            {
                _client.Close();
                logger.InfoFormat("_client.Close done");
            } 
            catch (Exception e)
            {
                logger.Error("_client.Close error", e);
            }

            _client = null;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public bool IsConnected()
        {
            if (_client == null || !_client.Connected)
            {
                return false;
            }

            return true;
        }


        public void Dispose()
        {
            Shutdown();
        }
    }

}
