using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdS_Project_2015_client_WPF.services
{
    class SocketConnection : IConnection
    {
        private const int CHECK_MESSAGE_RATE = 100; //ms

        private string endPointAddress;
        private int endPointPort;
        private System.Net.Sockets.TcpClient tcpClient;
        private System.Net.Sockets.NetworkStream stream;
        private System.Threading.Thread connectionThread;
        private bool active;
        private object socketLock;

        public string EndPointAddress { get => this.endPointAddress; set => this.endPointAddress = value; }
        public int EndPointPort { get => this.endPointPort; set => this.endPointPort = value; }

        public event MessageReceivedEventHandler MessageReceived;
        public event FailureEventHandler ConnectionFailure;

        public SocketConnection()
        {
            this.socketLock = new object();
        }

        public void Connect()
        {
            if (!this.active)
            {
                this.active = true;

                if (String.IsNullOrEmpty(this.endPointAddress) || endPointPort == 0)
                {
                    throw new Exception("endpoint address or endpoint port not set");
                }

                //connect to server socket
                this.tcpClient = new System.Net.Sockets.TcpClient(this.endPointAddress, this.endPointPort);
                this.stream = tcpClient.GetStream();

                //let's start the background thread that handle the socket connection
                this.connectionThread = new System.Threading.Thread(this.ManageConnection);
                this.connectionThread.IsBackground = true;
                this.connectionThread.Start();
            }
            else
            {
                throw new Exception("cannot connect, the connection is already active!");
            }
        }

        public void Disconnect()
        {
            if (this.active)
            {
                this.active = false;
                if (this.connectionThread != null && this.connectionThread.IsAlive)
                {
                    this.connectionThread.Join();
                    this.connectionThread = null;
                }

                if (this.tcpClient != null)
                {
                    this.tcpClient.Close();
                    this.tcpClient = null;
                }
            }            
        }

        public void SendMessage(string message)
        {            
            try
            {
                lock (this.socketLock)
                {
                    //translate the message from string to bytes
                    byte[] buffer = Encoding.ASCII.GetBytes(message);
                    int N = buffer.Count();
                    byte[] messageLength = BitConverter.GetBytes(N);

                    //send the message length
                    this.stream.Write(messageLength, 0, messageLength.Length);

                    //send the message
                    this.stream.Write(buffer, 0, N);
                }                                
            }
            catch(Exception e)
            {
                this.NotifyConnectionFailure(e.Message);
            }


        }

        private void NotifyConnectionFailure(string failureDescription)
        {
            if(this.ConnectionFailure != null)
            {
                this.ConnectionFailure(failureDescription);
            }
        }

        private void NotifyMessageReceived(string message)
        {
            if(this.MessageReceived != null)
            {
                this.MessageReceived(message);
            }
        }

        private void ManageConnection()
        {
            while (this.active)
            {
                //detect incoming message                
                try
                 {                    
                    this.checkConnectionStatus();
                    if (this.stream.DataAvailable)
                    {
                        //read msg from server
                        string message = ReadMessage();                    
                        if (String.IsNullOrEmpty(message))
                        {
                            throw new Exception("impossible to read message from server");
                        }
                        //notify that a message has been received
                        this.NotifyMessageReceived(message);
                    }
                    else
                    {                        
                        System.Threading.Thread.Sleep(CHECK_MESSAGE_RATE);
                    }
                }
                catch (Exception e)
                {   
                    //the thread will die soon
                    this.NotifyConnectionFailure(e.Message);
                    break;
                }
            }

            Console.WriteLine("Socket connection thread says:'Bye Bye :-)'");
        }

        private void checkConnectionStatus()
        {
            lock (this.socketLock)
            {
                //send a message length of zero bytes
                byte[] zeroMessageLength = BitConverter.GetBytes((int)0);

                this.stream.Write(zeroMessageLength, 0, zeroMessageLength.Length);
            }            
        }

        private string ReadMessage()
        {
            try
            {
                //read message length
                byte[] msgLen = ReadNBytes(sizeof(Int32));
                if (msgLen == null)
                {
                    return null;
                }                    
                Int32 iMsgLen = BitConverter.ToInt32(msgLen, 0);
                //read message
                byte[] msg = ReadNBytes(iMsgLen);
                if (msg == null)
                {
                    return null;
                }                    
                String stringMsg = System.Text.Encoding.UTF8.GetString(msg);                
                return stringMsg;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private byte[] ReadNBytes(int N)
        {            
            byte[] buffer = new byte[N];
            try
            {
                lock (this.socketLock)
                {
                    int totReadBytes = 0;
                    while (totReadBytes < N)
                    {
                        int readBytes = this.stream.Read(buffer, totReadBytes, N);
                        if (readBytes == 0)
                            return null;
                        totReadBytes += readBytes;
                    }
                }                
                return buffer;
            }
            catch (Exception)
            {
                return null;
            }
        }        
    }
}
