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
        private IApplicationInfoDataSource dataSource;
        private Dictionary<int, ApplicationDetails> applicationDetailsDB;
        private Object dbLock;
        private bool hasStarted;
        public bool HasStarted { get { return this.hasStarted; } }

        public ApplicationMonitor(IApplicationInfoDataSource dataSource)
        {
            this.dataSource = dataSource;
            this.applicationDetailsDB = new Dictionary<int, ApplicationDetails>();
            this.dbLock = new Object();
            this.hasStarted = false;
        }

        public List<ApplicationDetails> GetApplicationDetails()
        {
            this.updateAllApplicationDetailsFocusTime();
            lock (this.dbLock)
            {
                return this.applicationDetailsDB.Values.ToList();
            }
        }

        public void Start()
        {
            this.InitializeApplicationMonitor();
        }

        public void Stop()
        {
            this.DeactiveApplicationMonitor();
        }

        private void InitializeApplicationMonitor()
        {

            this.hasStarted = true;
            this.startingTime = System.DateTime.Now;
            this.lastUpdateTime = this.startingTime;

            this.dataSource.Open();

            //populate the db with the initial information
            List<ApplicationInfo> allApplicationInfo = dataSource.GetAllApplicationInfo();
            lock (this.dbLock)
            {
                foreach (ApplicationInfo applicationInfo in allApplicationInfo)
                {
                    ApplicationDetails applicationDetails = new ApplicationDetails(applicationInfo);
                    this.applicationDetailsDB.Add(applicationDetails.Id, applicationDetails);
                }
            }
            
            //subscribe the event handler
            this.dataSource.AppOpened += this.AppOpenedEventHandler;
            this.dataSource.AppClosed += this.AppClosedEventHandler;
            this.dataSource.FocusChange += this.FocusChangeEventHandler;

        }

        private void DeactiveApplicationMonitor()
        {

            this.dataSource.Close();

            //unsubscribe the event handler
            this.dataSource.AppOpened -= this.AppOpenedEventHandler;
            this.dataSource.AppClosed -= this.AppClosedEventHandler;
            this.dataSource.FocusChange -= this.FocusChangeEventHandler;

            //free the db
            lock (this.dbLock)
            {
                this.applicationDetailsDB.Clear();
            }

            this.startingTime = System.DateTime.MinValue;
            this.lastUpdateTime = System.DateTime.MinValue;

            this.hasStarted = false;
        }

        private void updateAllApplicationDetailsFocusTime()
        {
            System.DateTime updateTime = System.DateTime.Now;

            lock (this.dbLock)
            {
                foreach (KeyValuePair<int, ApplicationDetails> dbEntry in this.applicationDetailsDB)
                {
                    if (dbEntry.Value.HasFocus)
                    {
                        //update focus time
                        dbEntry.Value.TimeOnFocus += (updateTime - this.lastUpdateTime);
                    }
                }
            }
            
            this.lastUpdateTime = updateTime;

            this.PrintAllApplicationDetails();
        }

        private void AppOpenedEventHandler(int appId)
        {
            if (!this.applicationDetailsDB.ContainsKey(appId))
            {
                ApplicationInfo applicationInfo = this.dataSource.GetApplicationInfo(appId);
                ApplicationDetails applicationDetails = new ApplicationDetails(applicationInfo);

                lock (this.dbLock)
                {
                    this.applicationDetailsDB.Add(applicationDetails.Id, applicationDetails);
                }

                this.updateAllApplicationDetailsFocusTime();
            }
            else
            {
                throw new Exception("impossible to add application with id " + appId + " already in the db");
            }
        }

        private void AppClosedEventHandler(int appId)
        {
            if (this.applicationDetailsDB.ContainsKey(appId))
            {
                lock (this.dbLock)
                {
                    this.applicationDetailsDB.Remove(appId);
                }

                this.updateAllApplicationDetailsFocusTime();
            }
            else
            {
                throw new Exception("impossible to remove application with id " + appId + " not present in the db");
            }
        }

        private void FocusChangeEventHandler(int previousFocusAppId, int currentFocusAppId)
        {
            if (this.applicationDetailsDB.ContainsKey(previousFocusAppId) && this.applicationDetailsDB.ContainsKey(currentFocusAppId))
            {
                lock (this.dbLock)
                {
                    this.applicationDetailsDB[previousFocusAppId].HasFocus = false;
                    this.applicationDetailsDB[currentFocusAppId].HasFocus = true;
                }

                this.updateAllApplicationDetailsFocusTime();
            }
            else
            {
                throw new Exception("impossible to update focus missing applications in the db");
            }
        }

        private void PrintAllApplicationDetails()
        {
            lock (this.dbLock)
            {
                foreach (KeyValuePair<int, ApplicationDetails> dbEntry in this.applicationDetailsDB)
                {
                    Console.WriteLine("==============================");
                    Console.WriteLine("Application id: " + dbEntry.Value.Id);
                    Console.WriteLine("Application process name: " + dbEntry.Value.ProcessName);
                    Console.WriteLine("Application has focus: " + dbEntry.Value.HasFocus);
                    Console.WriteLine("Application focus time: " +dbEntry.Value.TimeOnFocus);
                    Console.WriteLine("==============================");
                }
            }
        }
    }
}
