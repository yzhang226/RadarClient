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

namespace Radar.Bidding.Net
{

    [Component]
    public class SocketClient : IDisposable
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(Radar.Bidding.Net.SocketClient));

        private string ip;
 
        private int port;

        //信息接收进程
        private Thread bytesReceiveThread = null;

        // ManualResetEvent instances signal completion.  
        private static ManualResetEvent connectDone = new ManualResetEvent(false);
        private static ManualResetEvent sendDone = new ManualResetEvent(false);
        private static ManualResetEvent receiveDone = new ManualResetEvent(false);

        // The response from the remote device.  
        private static String response = String.Empty;

        private Socket _client;
        private bool isReceiveWork = true;

        private Func<RawMessage, String> anotherRecvCallback;

        private ProjectConfig conf;

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

            isReceiveWork = true;

            try
            {
                // Establish the remote endpoint for the socket.  
                // The name of the remote device is "host.contoso.com".  
                // IPHostEntry ipHostInfo = Dns.GetHostEntry(host);
                // IPAddress ipAddress = ipHostInfo.AddressList[0];

                IPAddress ipAddr = IPAddress.Parse(ip);
                IPEndPoint remoteEP = new IPEndPoint(ipAddr, port);

                // Create a TCP/IP socket.  
                _client = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                // Connect to the remote endpoint.  
                _client.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), _client);
                connectDone.WaitOne();

                logger.InfoFormat("end socket-client connect with address#{0}:{1}, _client#{2}", ip, port, _client);

                // 初始化线程
                bytesReceiveThread = Radar.Common.Threads.Threads.StartNewBackgroudThread(StartReceiveForEver);
            }
            catch (Exception e)
            {
                logger.Error("StartClient error", e);
            }
        }

        public void setAnotherRecvCallback(Func<RawMessage, String> anotherRecvCallback)
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

                // Signal that the connection has been made.  
                connectDone.Set();
            }
            catch (Exception e)
            {
                logger.Error("ConnectCallback error.", e);
            }
        }

        private void StartReceiveForEver()
        {
            while (isReceiveWork)
            {
                try
                {
                    // TODO: 这里只能读取一个buffer大小, 后续优化
                    byte[] bytes = new byte[1024 * 4];
                    int bytesRec = _client.Receive(bytes);
                    byte[] data = new byte[bytesRec];
                    ByteUtils.arraycopy(bytes, 0, data, 0, bytesRec);

                    var raw = RawMessageEncoder.me.decode(data);
                    logger.InfoFormat("receive data is {0}, {1}, {2}", raw.totalLength, raw.messageType, raw.bodyText);

                    Radar.Bidding.Messages.MessageDispatcher.me.Dispatch(raw);

                    this.anotherRecvCallback?.Invoke(raw);

                    // logger.InfoFormat("Echoed test = {0}", Encoding.ASCII.GetString(bytes, 0, bytesRec));
                }
                catch (Exception e)
                {
                    logger.Error("Receive error", e);
                }
            }
        }

        public void Send(RawMessage raw)
        { 
            byte[] byteData = RawMessageEncoder.me.encode(raw);//  Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.  
            _client.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), _client);
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
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
            isReceiveWork = false;
            Radar.Common.Threads.Threads.TryStopThreadByWait(bytesReceiveThread, 100, 100, "SocketClient");

            _client.Shutdown(SocketShutdown.Both);
            _client.Close();
        }

        public void Dispose()
        {
            Shutdown();
        }
    }

}
