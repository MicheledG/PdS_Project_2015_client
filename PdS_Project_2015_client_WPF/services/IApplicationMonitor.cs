using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdS_Project_2015_client_WPF.services
{    
    
    interface IApplicationMonitor : INotifyPropertyChanged
    {
        bool IsActive { get; }
        System.TimeSpan ActiveTime { get; }
        void Start();
        void Stop();
        Dictionary<int, ApplicationDetails> GetAllApplicationDetails();
        event FailureEventHandler ApplicationMonitorFailure;        
    }    
}
