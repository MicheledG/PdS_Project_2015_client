using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdS_Project_2015_client_WPF.services
{
    interface IApplicationMonitor
    {
        void Start();
        void Stop();
        List<ApplicationDetails> GetApplicationDetails();
    }
}
