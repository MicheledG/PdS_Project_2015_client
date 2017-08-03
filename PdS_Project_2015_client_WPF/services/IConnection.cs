using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdS_Project_2015_client_WPF.services
{
    public delegate void MessageReceivedEventHandler(string message);
    public delegate void ConnectionFailureEventHandler(string failureDescription);

    interface IConnection
    {
        String EndPointAddress { get; set; }
        int EndPointPort { get; set; }
        void Connect();
        void Disconnect();
        void SendMessage(string message);
        event MessageReceivedEventHandler MessageReceived;
        event ConnectionFailureEventHandler ConnectionFailure;
    }
}
