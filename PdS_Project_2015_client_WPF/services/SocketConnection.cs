using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdS_Project_2015_client_WPF.services
{
    class SocketConnection : IConnection
    {
        private string endPointAddress;
        private int endPointPort;
        private System.Net.Sockets.TcpClient tcpClient;
        private System.Net.Sockets.NetworkStream stream;
        private System.Threading.Thread connectionThread;
        private bool active;

        public string EndPointAddress { get => this.endPointAddress; set => this.endPointAddress = value; }
        public int EndPointPort { get => this.endPointPort; set => this.endPointPort = value; }

        public event MessageReceivedEventHandler MessageReceived;
        public event ConnectionFailureEventHandler ConnectionFailure;

        public void Connect()
        {
            if(String.IsNullOrEmpty(this.endPointAddress) || endPointPort == 0)
            {
                throw new Exception("endpoint address or endpoint port not set");
            }
        
            //connect to server socket
            this.tcpClient = new System.Net.Sockets.TcpClient(this.endPointAddress, this.endPointPort);
            this.stream = tcpClient.GetStream();
            this.active = true;

            //let's start the background thread that handle the socket connection
            this.connectionThread = new System.Threading.Thread(this.ManageConnection);
            this.connectionThread.IsBackground = true;
            this.connectionThread.Start();
        }

        public void Disconnect()
        {
            if(this.connectionThread != null)
            {
                this.active = false;
                this.connectionThread.Join();
                this.connectionThread = null;                
            }
            if(this.tcpClient != null)
            {
                this.tcpClient.Close();
            }
        }

        public void SendMessage(string message)
        {
            throw new NotImplementedException();
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
                    //read msg from server
                    String message = WaitMessage();
                    if (String.IsNullOrEmpty(message))
                    {
                        throw new Exception("impossible to read message from server");
                    }
                    //notify that a message has been received
                    this.NotifyMessageReceived(message);
                }
                catch (Exception e)
                {
                    //close the connection
                    this.tcpClient.Close();
                    this.tcpClient = null;
                    this.NotifyConnectionFailure(e.Message);
                    break;
                }
            }
        }

        private string WaitMessage()
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
                int totReadBytes = 0;
                while (totReadBytes < N)
                {
                    int readBytes = this.stream.Read(buffer, totReadBytes, N);
                    if (readBytes == 0)
                        return null;
                    totReadBytes += readBytes;
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
