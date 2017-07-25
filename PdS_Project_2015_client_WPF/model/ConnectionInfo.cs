using System;

namespace PdS_Project_2015_client_WPF.services
{
    public class ConnectionInfo
    {
        private string address;
        private int port;
        private System.DateTime connectionOpeningTime;

        public string Address { get => address; set => address = value; }
        public int Port { get => port; set => port = value; }
        public DateTime ConnectionOpeningTime { get => connectionOpeningTime; set => connectionOpeningTime = value; }
        public TimeSpan ConnectionTime
        {
            get
            {
                return System.DateTime.Now - this.ConnectionOpeningTime;
            }
        }

    }
}