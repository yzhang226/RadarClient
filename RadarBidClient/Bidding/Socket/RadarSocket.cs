using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Radar
{

    public class MessageDispatcher
    {

        private static readonly ILog logger = LogManager.GetLogger(typeof(MessageDispatcher));

        private static readonly Dictionary<int, MessageProcessor> processors = new Dictionary<int, MessageProcessor>();

        public static readonly MessageDispatcher dispatcher = new MessageDispatcher();

        private MessageDispatcher()
        {

        }

        public void dispatch(RawMessage message)
        {
            MessageProcessor processor = processors[message.messageType];
            if (processor != null)
            {
                processor.process(message);
            }
            else
            {
                logger.ErrorFormat("no processor for type#{0}", message.messageType);
            }
        }

        public void register(MessageProcessor processor)
        {
            processors[processor.messageType()] = processor;
            logger.InfoFormat("register message-type#{0} with processor#{1}", processor.messageType(), processor);
        }

    }

    public interface MessageProcessor
    {
        ProcessResult process(RawMessage message);

        int messageType();

    }

    

    public class ProcessResult
    {

        public int status;
        public string message;
        public int data;
    }


    public class RawMessages
    {


        public static RawMessage from(int messageType, int clientNo, String body)
        {
            RawMessage msg = new RawMessage();

            int bLen = body.Length;
            int tLen = 4 + 4 + 8 + 4 + 4 + 4 + bLen;

            msg.setTotalLength(tLen);
            msg.setMagic(B.MAGIC_NUMBER);
            msg.setOccurMills(KK.CurrentMills());

            msg.setClientNo(clientNo);
            msg.setMessageType(messageType);
            msg.setBodyLength(bLen);

            msg.setBodyText(body);

            return msg;
        }


    }

    public class RawMessage
    {

        /**
         * 总长度
         */
        public int totalLength;// 4 - bytes

        /**
         * 魔数
         */
        public int magic;// 4 - bytes

        /**
         * 发生时间毫秒数
         */
        public long occurMills;// 8 - bytes

        /**
         * 客户端编号
         */
        public int clientNo { get; set; }// 4

        /**
         * 消息类型
         */
        public int messageType;// 4

        /**
         *
         */
        public int bodyLength;// 4

        /**
         * 消息体 - 字符串格式
         */
        public String bodyText;


        public int getTotalLength()
        {
            return totalLength;
        }

        public void setTotalLength(int totalLength)
        {
            this.totalLength = totalLength;
        }

        public int getMagic()
        {
            return magic;
        }

        public void setMagic(int magic)
        {
            this.magic = magic;
        }

        public long getOccurMills()
        {
            return occurMills;
        }

        public void setOccurMills(long occurMills)
        {
            this.occurMills = occurMills;
        }

        public int getClientNo()
        {
            return clientNo;
        }

        public void setClientNo(int clientNo)
        {
            this.clientNo = clientNo;
        }

        public int getMessageType()
        {
            return messageType;
        }

        public void setMessageType(int messageType)
        {
            this.messageType = messageType;
        }

        public int getBodyLength()
        {
            return bodyLength;
        }

        public void setBodyLength(int bodyLength)
        {
            this.bodyLength = bodyLength;
        }

        public String getBodyText()
        {
            return bodyText;
        }

        public void setBodyText(String bodyText)
        {
            this.bodyText = bodyText;
        }

    }

    public class RawMessageEncoder
    {

        public RawMessage decode(byte[] data)
        {
            RawMessage msg = new RawMessage();
            msg.setTotalLength(B.readInt(data, 0));
            msg.setMagic(B.readInt(data, 4));
            msg.setOccurMills(B.readLong(data, 8));

            msg.setClientNo(B.readInt(data, 16));
            msg.setMessageType(B.readInt(data, 20));
            msg.setBodyLength(B.readInt(data, 24));

            string body = Encoding.Default.GetString(B.readFixLength(data, 28, msg.getBodyLength()));

            msg.setBodyText(body);

            return msg;
        }

        public byte[] encode(RawMessage msg)
        {
            byte[] data = new byte[msg.getTotalLength()];

            B.writeInt(data, 0, msg.getTotalLength());
            B.writeInt(data, 4, msg.getMagic());
            B.writeLong(data, 8, msg.getOccurMills());

            B.writeInt(data, 16, msg.getClientNo());
            B.writeInt(data, 20, msg.getMessageType());
            B.writeInt(data, 24, msg.getBodyLength());

            byte[] bodyBytes = Encoding.Default.GetBytes(msg.getBodyText());

            B.writeBytes(data, 28, bodyBytes);

            return data;
        }
    }

    class B
    {

        /**
         *
         */
        public static readonly int MAGIC_NUMBER = 2140483647;

        // write

        public static void writeLong(byte[] data, int offset, long value)
        {
            data[offset] = (byte)((value >> 56) & 0xFF);
            data[offset + 1] = (byte)((value >> 48) & 0xFF);
            data[offset + 2] = (byte)((value >> 40) & 0xFF);
            data[offset + 3] = (byte)((value >> 32) & 0xFF);
            data[offset + 4] = (byte)((value >> 24) & 0xFF);
            data[offset + 5] = (byte)((value >> 16) & 0xFF);
            data[offset + 6] = (byte)((value >> 8) & 0xFF);
            data[offset + 7] = (byte)((value) & 0xFF);
        }

        public static void writeInt(byte[] data, int offset, int value)
        {
            data[offset] = (byte)((value >> 24) & 0xFF);
            data[offset + 1] = (byte)((value >> 16) & 0xFF);
            data[offset + 2] = (byte)((value >> 8) & 0xFF);
            data[offset + 3] = (byte)((value) & 0xFF);
        }

        public static void writeByte(byte[] data, int offset, byte value)
        {
            data[offset] = value;
        }

        public static void writeBytes(byte[] data, int offset, byte[] bs)
        {
            for (int i = 0; i < bs.Length; i++)
            {
                data[offset + i] = bs[i];
            }
        }

        // read

        public static byte read1Byte(byte[] bs, int offset)
        {
            return bs[offset];
        }

        public static byte[] read4Bytes(byte[] bs, int offset)
        {
            return new byte[] { bs[offset], bs[offset + 1], bs[offset + 2], bs[offset + 3] };
        }

        public static byte[] readFixLength(byte[] bs, int offset, int len)
        {
            byte[] tar = new byte[len];
            arraycopy(bs, offset, tar, 0, len);
            return tar;
        }

        public static int readInt(byte[] bs, int offset)
        {
            byte[] seg = read4Bytes(bs, offset);
            int ret = (seg[0] << 24) + (seg[1] << 16) + (seg[2] << 8) + seg[3];
            return ret;
        }

        public static long readLong(byte[] bs, int offset)
        {
            byte[] seg = readFixLength(bs, offset, 8);
            long ret = ((long)seg[0] << 56) + ((long)seg[1] << 48) + ((long)seg[2] << 40) + ((long)seg[3] << 32) +
                    ((long)seg[4] << 24) + (seg[5] << 16) + (seg[6] << 8) + seg[7]
                    ;
            return ret;
        }

        public static void arraycopy(byte[] source, int sourceOffset, byte[] target, int targetOffset, int targetLength)
        {
            int i = 0;
            for (; i<targetLength; i++)
            {
                target[targetOffset + i] = source[sourceOffset + i];
            }

        }

    }

    class SocketClient
    {

        private static readonly ILog logger = LogManager.GetLogger(typeof(SocketClient));

        private string ipAdress = "127.0.0.1";

        // The port number for the remote device.  
        private int port = 9966;

        //信息接收进程
        private Thread bytesReceiveThread = null;

        // ManualResetEvent instances signal completion.  
        private static ManualResetEvent connectDone = new ManualResetEvent(false);
        private static ManualResetEvent sendDone = new ManualResetEvent(false);
        private static ManualResetEvent receiveDone = new ManualResetEvent(false);

        // The response from the remote device.  
        private static String response = String.Empty;

        private Socket _client;

        private Func<RawMessage, String> anotherRecvCallback;

        private RawMessageEncoder messageEncoder = new RawMessageEncoder();

        public SocketClient(string ipAdress, int port)
        {
            this.ipAdress = ipAdress;
            this.port = port;
        }

        public void StartClient()
        {
            // Connect to a remote device.  
            try
            {
                // Establish the remote endpoint for the socket.  
                // The name of the remote device is "host.contoso.com".  
                // IPHostEntry ipHostInfo = Dns.GetHostEntry(host);
                // IPAddress ipAddress = ipHostInfo.AddressList[0];

                IPAddress ipAddress = IPAddress.Parse(ipAdress);
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

                // Create a TCP/IP socket.  
                _client = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                // Connect to the remote endpoint.  
                _client.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), _client);
                connectDone.WaitOne();

                // Send test data to the remote device.  
                //Send(client, "This is a test<EOF>");
                //sendDone.WaitOne();

                // Receive the response from the remote device.  
                // StartReceiveForEver(_client);

                //初始化线程
                bytesReceiveThread = new Thread(new ThreadStart(StartReceiveForEver));
                bytesReceiveThread.IsBackground = true;
                //开启线程[用于接收数据]
                bytesReceiveThread.Start();

                // receiveDone.WaitOne();

                // Write the response to the console.  
                // logger.InfoFormat("Response received : {0}", response);

                // Release the socket.  
                // client.Shutdown(SocketShutdown.Both);
                // client.Close();

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
                Socket client = (Socket)ar.AsyncState;

                // Complete the connection.  
                client.EndConnect(ar);

                logger.InfoFormat("Socket connected to {0}", client.RemoteEndPoint.ToString());

                // Signal that the connection has been made.  
                connectDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void StartReceiveForEver()
        {
            while (true)
            {
                try
                {
                    // TODO: 这里使用 发生了问题, 后续解决
                    // Create the state object.  
                    //StateObject state = new StateObject();
                    //state.workSocket = _client;

                    ////// Begin receiving the data from the remote device.  
                    //_client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);

                    //receiveDone.WaitOne();

                    // TODO: 这里只能读取一个buffer大小, 后续优化
                    byte[] bytes = new byte[1024 * 4];
                    int bytesRec = _client.Receive(bytes);
                    byte[] data = new byte[bytesRec];
                    B.arraycopy(bytes, 0, data, 0, bytesRec);

                    var raw = messageEncoder.decode(data);
                    logger.InfoFormat("receive data is {0}, {1}, {2}", raw.totalLength, raw.messageType, raw.bodyText);

                    MessageDispatcher.dispatcher.dispatch(raw);

                    this.anotherRecvCallback?.Invoke(raw);

                    // logger.InfoFormat("Echoed test = {0}", Encoding.ASCII.GetString(bytes, 0, bytesRec));
                }
                catch (Exception e)
                {
                    logger.Error("Receive error", e);
                }
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the state object and the client socket from the asynchronous state object.  
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;

                // Read data from the remote device.  
                int bytesRead = client.EndReceive(ar);

                if (bytesRead > 0)
                {
                    int totalLen = state.byteTotalLength + bytesRead;
                    byte[] data = new byte[totalLen];

                    // copy orginal 
                    for (int i=0; i<state.byteTotalLength; i++)
                    {
                        data[i] = state.bytesData[i];
                    }
                    
                    // put readed buffer to data
                    for (int i=0; i<bytesRead; i++)
                    {
                        data[state.byteTotalLength + i] = state.buffer[i];
                    }

                    // There might be more data, so store the data received so far.  
                   //  state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

                    state.bytesData = data;

                    // Get the rest of the data.  
                    client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
                }
                else
                {
                    // All the data has arrived; put it in response.  
                    if (state.sb.Length > 1)
                    {
                        // response = state.sb.ToString();
                        var raw = messageEncoder.decode(state.bytesData);

                        logger.InfoFormat("receive data is {0}, {1}, {2}", raw.totalLength, raw.messageType, raw.bodyText);

                    }
                    // Signal that all bytes have been received.  
                    receiveDone.Set();
                }
            }
            catch (Exception e)
            {
                logger.Error("ReceiveCallback error", e);
            }
        }

        public void Send(String data)
        {
            RawMessage raw = RawMessages.from(10002, 443322, data);
            // Convert the string data to byte data using ASCII encoding.  
            byte[] byteData = messageEncoder.encode(raw);//  Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.  
            _client.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), _client);
        }

        public void Send(RawMessage raw)
        { 
            byte[] byteData = messageEncoder.encode(raw);//  Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.  
            _client.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), _client);
        }

        private static void SendCallback(IAsyncResult ar)
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


    }



    // State object for receiving data from remote device.  
    public class StateObject
    {
        // Client socket.  
        public Socket workSocket = null;
        // Size of receive buffer.  
        public const int BufferSize = 256;
        // Receive buffer.  
        public byte[] buffer = new byte[BufferSize];
        // Received data string.  
        public StringBuilder sb = new StringBuilder();

        // length of total bytes
        public int byteTotalLength;

        // total bytes
        public byte[] bytesData;

    }

}
