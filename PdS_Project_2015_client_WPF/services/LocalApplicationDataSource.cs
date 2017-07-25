using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdS_Project_2015_client_WPF.services
{
    class LocalApplicationDataSource : IApplicationDataSource
    {

        private List<ApplicationInfo> appInfos;
        private int focusIndex;

        public LocalApplicationDataSource()
        {
            this.appInfos = new List<ApplicationInfo>();

            focusIndex = 2;
            appInfos.Add(new ApplicationInfo() { Id = 1, ProcessName = "chrome", HasFocus = false });
            appInfos.Add(new ApplicationInfo() { Id = 2, ProcessName = "notepad", HasFocus = false });
            appInfos.Add(new ApplicationInfo() { Id = 3, ProcessName = "spotify", HasFocus = false });
        }

        public List<ApplicationInfo> GetApplicationInfo()
        {
            appInfos[focusIndex].HasFocus = false;
            focusIndex++;
            if(focusIndex >= 3)
            {
                focusIndex = 0;
            }
            appInfos[focusIndex].HasFocus = true;

            return this.appInfos;
        }
    }
}
