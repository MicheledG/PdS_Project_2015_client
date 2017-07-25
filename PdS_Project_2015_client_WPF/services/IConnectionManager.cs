using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdS_Project_2015_client_WPF.services
{
    interface IConnectionManager
    {
        void OpenConnection(string address, int port);
        void CloseConnection();
        ConnectionInfo GetConnectionInfo();
        IApplicationDataSource GetApplicationDataSource();
    }
}
