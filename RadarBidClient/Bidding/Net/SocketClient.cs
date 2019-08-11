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
        private int connectStatus = 0;
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
            this.ip = ip;
            this.port = port;
        }

        public SocketClient(ProjectConfig conf)
        {
            var addr = conf.SaberServerAddress;
            this.ip = addr.Split(':')[0];
            this.port = int.Parse(addr.Split(':')[1]);

            this.conf = conf;
        }

        public void StartClient()
        {
            logger.InfoFormat("begin start socket-client with address#{0}:{1}", ip, port);

            socketStarting = true;
            working = true;
            connectStatus = 1;
            try
            {
                IPAddress ipAddr = IPAddress.Parse(ip);
                IPEndPoint remoteEP = new IPEndPoint(ipAddr, port);

                // Create a TCP/IP socket.  
                _client = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                // Connect to the remote endpoint.  
                _client.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), _client);
                while (connectStatus == 1)
                {
                    KK.Sleep(100);

                    if (connectStatus == 100)
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


            if (EnableSocketGuard)
            {
                StartSocketGuardIfNotExist();
            }

        }

        private void InvokeAfterSuccessConnected()
        {
            if (connectStatus != 10)
            {
                logger.WarnFormat("connectStatus is not success, so DONOT Invoke AfterSuccessConnected");
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

        private void RestartClient()
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

                connectStatus = 10;
            }
            catch (Exception e)
            {
                logger.Error("ConnectCallback error.", e);
                connectStatus = 100;
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
                    logger.InfoFormat("receive data is {0}, {1}, {2}", raw.totalLength, raw.messageType, raw.bodyText);

                    MessageDispatcher.me.Dispatch(raw);

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
                logger.InfoFormat("Sent {0} bytes to server.", bytesSent);

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

        private void StartSocketGuardIfNotExist()
        {
            if (socketWatchThrad != null)
            {
                return;
            }

            logger.InfoFormat("begin StartSocketClientGuard");
            watching = true;
            socketWatchThrad = ThreadUtils.StartNewBackgroudThread(WatchSocketClient);
        }

        /// <summary>
        /// 连续 未连接 次数
        /// </summary>
        private int continuousDisconnectedCount = 0;

        /// <summary>
        /// 连续 连接 次数
        /// </summary>
        private int continuousConnectedCount = 0;

        private static readonly int RESTART_THRESHOLD = 1;

        private static readonly int DETECT_INTERVAL_MILLS = 2000;

        private static readonly int AGAIN_CHECK_INTERVAL_MILLS = DETECT_INTERVAL_MILLS * 30;

        private static readonly int AGAIN_CHECK_THRESHOLD = AGAIN_CHECK_INTERVAL_MILLS / DETECT_INTERVAL_MILLS;

        private void WatchSocketClient()
        {
            KK.Sleep(5 * 1000);


            while (watching)
            {
                KK.Sleep(DETECT_INTERVAL_MILLS);

                if (socketStarting)
                {
                    logger.InfoFormat("socket is starting");
                    continue;
                }

                try
                {

                    bool connected = _client != null && IsConnected(_client);
                    DateTime dt = DateTime.Now;

                    // logger.InfoFormat("socket connected#{0}", connected);

                    if (dt.Minute > 57 && dt.Second % 8 == 0)
                    {
                        logger.InfoFormat("dice socket connected#{0}", connected);
                    }

                    if (!connected)
                    {
                        continuousConnectedCount = 0;
                        continuousDisconnectedCount++;
                        logger.InfoFormat("socket connected is false, count#{0}", continuousDisconnectedCount);
                    }
                    else
                    {
                        continuousConnectedCount++;
                        continuousDisconnectedCount = 0;
                    }

                    if (continuousDisconnectedCount >= RESTART_THRESHOLD)
                    {
                        continuousConnectedCount = 0;
                        continuousDisconnectedCount = 0;
                        this.RestartClient();
                    } 

                } catch (Exception e)
                {
                    logger.Error("WatchSocketClient error", e);
                } 
                finally
                {
                    
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        private bool IsConnected(Socket client)
        {
            if (client == null || !client.Connected)
            {
                return false;
            }

            if ( (continuousConnectedCount % AGAIN_CHECK_THRESHOLD) == 0 )
            {
                return CheckConnectStateBySend(client);
            }
            else
            {
                return CheckConnectStateByPoll(client);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private bool CheckConnectStateByPoll(Socket s)
        {
            bool part1 = s.Poll(1000, SelectMode.SelectRead);
            bool part2 = (s.Available == 0);
            if ((part1 && part2) || !s.Connected)
                return false;
            else
                return true;
        }

        /// <summary>
        /// Connected 当它返回false时Socket, 要么从未连接, 要么不再处于连接状态。
        /// 
        /// 参考 https://docs.microsoft.com/zh-cn/dotnet/api/system.net.sockets.socket.connected?redirectedfrom=MSDN&view=netframework-4.8#System_Net_Sockets_Socket_Connected
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        private bool CheckConnectStateBySend(Socket client)
        {
            // This is how you can determine whether a socket is still connected.
            bool blockingState = client.Blocking;
            bool connected = false;
            try
            {
                byte[] tmp = new byte[1];

                client.Blocking = false;
                client.Send(tmp, 0, 0);
                connected = true;
            }
            catch (SocketException e)
            {
                // 10035 == WSAEWOULDBLOCK
                if (e.NativeErrorCode.Equals(10035))
                {
                    logger.InfoFormat("Still Connected, but the Send would block");
                }
                else
                {
                    logger.ErrorFormat("Disconnected: error code {0}!", e.NativeErrorCode);
                }
            }
            catch(Exception e)
            {
                connected = false;
                logger.Error("CheckConnectStateBySend error", e);
            }
            finally
            {
                client.Blocking = blockingState;
            }

            logger.DebugFormat("Connected: {0}", connected);

            return connected;
        }

        public void Dispose()
        {
            Shutdown();
        }
    }

}
