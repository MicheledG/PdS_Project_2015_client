using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdS_Project_2015_client_WPF.services
{
    class ApplicationMonitor : IApplicationMonitor
    {
        private const int UPDATE_RATE = 100; //ms
        private System.DateTime startingTime;
        private TimeSpan activeTime;
        private System.DateTime lastUpdateTime;
        private bool hasStarted;
        public bool HasStarted { get { return this.hasStarted; } }

        public TimeSpan ActiveTime { get => activeTime; set => activeTime = value; }

        private System.Threading.Thread monitorThread;

        private IApplicationInfoDataSource dataSource;
        private Dictionary<int, ApplicationDetails> applicationDetailsDB;
        private Object monitorLock;
        

        public ApplicationMonitor(IApplicationInfoDataSource dataSource)
        {
            this.hasStarted = false;            

            this.dataSource = dataSource;
            this.applicationDetailsDB = new Dictionary<int, ApplicationDetails>();
            this.monitorLock = new Object();
        }
        
        public List<ApplicationDetails> GetApplicationDetails()
        {
            lock (this.monitorLock)
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
            lock (this.monitorLock)
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

            this.monitorThread = new System.Threading.Thread(this.MonitorApplications);
            this.monitorThread.Start();
        }

        private void DeactiveApplicationMonitor()
        {

            this.hasStarted = false;
            this.monitorThread.Join();

            this.dataSource.Close();

            //unsubscribe the event handler
            this.dataSource.AppOpened -= this.AppOpenedEventHandler;
            this.dataSource.AppClosed -= this.AppClosedEventHandler;
            this.dataSource.FocusChange -= this.FocusChangeEventHandler;

            //free the db
            lock (this.monitorLock)
            {
                this.applicationDetailsDB.Clear();
            }

            this.startingTime = System.DateTime.MinValue;
            this.lastUpdateTime = System.DateTime.MinValue;
            this.activeTime = System.TimeSpan.Zero;                        
        }
        
        //Background method executed in a different thread that updates periodically the information contained into the db
        private void MonitorApplications()
        {
            while (this.hasStarted)
            {
                UpdateMonitorData();
                System.Threading.Thread.Sleep(UPDATE_RATE);
            }
        }

        //This method can be called by anyone to update the monitor status and the application details stored into the monitor DB
        private void UpdateMonitorData()
        {
            lock (this.monitorLock)
            {
                System.DateTime updateTime = System.DateTime.Now;
                this.activeTime = updateTime - this.startingTime;
            
                foreach (KeyValuePair<int, ApplicationDetails> dbEntry in this.applicationDetailsDB)
                {
                    if (dbEntry.Value.HasFocus)
                    {
                        //update absolute time on focus
                        dbEntry.Value.TimeOnFocus += (updateTime - this.lastUpdateTime);
                    }
                    //update percentual time on focus
                    dbEntry.Value.TimeOnFocusPercentual = (int)((dbEntry.Value.TimeOnFocus.TotalMilliseconds / this.activeTime.TotalMilliseconds) * 100);
                }
                        
                this.lastUpdateTime = updateTime;
            }

            //DEBUG!
            this.PrintAllApplicationDetails();
        }

        /*
         * EVENT DRIVEN HANDLERs 
         */
       
        //Handler called by anyone to insert the new application details in the Monitor DB
        private void AppOpenedEventHandler(int appId)
        {
            if (!this.applicationDetailsDB.ContainsKey(appId))
            {
                ApplicationInfo applicationInfo = this.dataSource.GetApplicationInfo(appId);
                ApplicationDetails applicationDetails = new ApplicationDetails(applicationInfo);

                lock (this.monitorLock)
                {
                    this.applicationDetailsDB.Add(applicationDetails.Id, applicationDetails);
                }

                this.UpdateMonitorData();
            }
            else
            {
                throw new Exception("impossible to add application with id " + appId + " already in the db");
            }
        }

        //Handler called by anyone to remove the details of a closed application from the Monitor DB
        private void AppClosedEventHandler(int appId)
        {
            if (this.applicationDetailsDB.ContainsKey(appId))
            {
                lock (this.monitorLock)
                {
                    this.applicationDetailsDB.Remove(appId);
                }

                this.UpdateMonitorData();
            }
            else
            {
                throw new Exception("impossible to remove application with id " + appId + " not present in the db");
            }
        }

        //Handler called by anyone to update the application which holds the focus
        private void FocusChangeEventHandler(int previousFocusAppId, int currentFocusAppId)
        {
            if (this.applicationDetailsDB.ContainsKey(previousFocusAppId) && this.applicationDetailsDB.ContainsKey(currentFocusAppId))
            {
                lock (this.monitorLock)
                {
                    this.applicationDetailsDB[previousFocusAppId].HasFocus = false;
                    this.applicationDetailsDB[currentFocusAppId].HasFocus = true;
                }

                this.UpdateMonitorData();
            }
            else
            {
                throw new Exception("impossible to update focus missing applications in the db");
            }
        }

        //DEBUG FUNCTION!!!
        private void PrintAllApplicationDetails()
        {
            lock (this.monitorLock)
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
