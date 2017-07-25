using System.Collections.Generic;

namespace PdS_Project_2015_client_WPF.services
{
    public interface IApplicationDataSource
    {
        List<ApplicationInfo> GetApplicationInfo();
    }
}