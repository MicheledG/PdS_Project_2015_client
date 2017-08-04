using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdS_Project_2015_client_WPF.services
{
    public delegate void ApplicationMonitorDataUpdatedEventHandler();
    public delegate void ApplicationMonitorStatusChangedEventHandler();

    interface IApplicationMonitor
    {
        bool HasStarted { get; }
        System.TimeSpan ActiveTime { get; }
        void Start();
        void Stop();
        Dictionary<int, ApplicationDetails> GetAllApplicationDetails();
        event ApplicationMonitorDataUpdatedEventHandler ApplicationMonitorDataUpdated;
        event ApplicationMonitorStatusChangedEventHandler ApplicationMonitorStatusChanged;
    }    
}
