using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdS_Project_2015_client_WPF.services
{
    class ApplicationMonitor : IApplicationMonitor
    {
        private System.DateTime startingTime;
        private System.DateTime lastUpdateTime;
        private IApplicationDataSource dataSource;
        private Dictionary<int, ApplicationDetails> applicationDetailsDB;

        public ApplicationMonitor(IApplicationDataSource dataSource)
        {
            this.dataSource = dataSource;
            this.applicationDetailsDB = new Dictionary<int, ApplicationDetails>();
        }

        public List<ApplicationDetails> GetApplicationDetails()
        {
            this.UpdateApplicationDetails();
            return this.applicationDetailsDB.Values.ToList();
        }

        public void Start()
        {
            this.startingTime = System.DateTime.Now;
        }

        public void Stop()
        {
            //nothing to release till now
            this.startingTime = System.DateTime.MinValue;
        }

        /*
         *  It retrieves information from the data source and updates the 
         *  details of the applications
         *
         */
        private void UpdateApplicationDetails()
        {

            System.DateTime updateTime = System.DateTime.Now;
            List<ApplicationInfo> applicationInfos = dataSource.GetApplicationInfo();

            //remove all the applications closed between an update and the following
            Dictionary<int, ApplicationDetails> tmp = new Dictionary<int, ApplicationDetails>();
            foreach (KeyValuePair<int, ApplicationDetails> dbEntry in applicationDetailsDB)
            {
                if (applicationInfos.Contains(dbEntry.Value as ApplicationInfo))
                {
                    tmp.Add(dbEntry.Key, dbEntry.Value);
                }
            }
            this.applicationDetailsDB = tmp;

            //add all the applications opened between an update and the following
            foreach (ApplicationInfo applicationInfo in applicationInfos)
            {
                if (!this.applicationDetailsDB.ContainsKey(applicationInfo.Id))
                {
                    ApplicationDetails applicationDetails = new ApplicationDetails(applicationInfo);
                    this.applicationDetailsDB.Add(applicationDetails.Id, applicationDetails);
                }
            }

            //TODO: HANDLE FOCUS IN A CLEVER WAY => MAYBE DELEGATE AND EVENTS!

            this.lastUpdateTime = updateTime;
        }
    }
}
