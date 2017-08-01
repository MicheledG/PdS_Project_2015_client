using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdS_Project_2015_client_WPF.services
{
    class ApplicationMonitor : IApplicationMonitor
    {
        private const int UPDATE_RATE = 500; //ms

        private bool hasStarted;
        private System.DateTime startingTime;        
        private System.DateTime lastUpdateTime;
        private TimeSpan activeTime;

        private System.Threading.Thread monitorThread;
                
        //protects all the data accessible from different thread (dued to Event handlers, GUI thread and background thread)
        private Object monitorLock;

        private IApplicationInfoDataSource dataSource;
        private Dictionary<int, ApplicationDetails> applicationDetailsDB;

        public event ApplicationMonitorDataUpdatedEventHandler ApplicationMonitorDataUpdated;

        public bool HasStarted { get { return this.hasStarted; } }
        public TimeSpan ActiveTime { get => activeTime; set => activeTime = value; }

        public ApplicationMonitor(IApplicationInfoDataSource dataSource)
        {
            this.hasStarted = false;            
            this.dataSource = dataSource;
            this.applicationDetailsDB = new Dictionary<int, ApplicationDetails>();
            this.monitorLock = new Object();
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
            
                //subscribe the event handler
                this.dataSource.AppOpened += this.AppOpenedEventHandler;
                this.dataSource.AppClosed += this.AppClosedEventHandler;
                this.dataSource.FocusChange += this.FocusChangeEventHandler;
            }

            this.monitorThread = new System.Threading.Thread(this.MonitorApplications);
            this.monitorThread.IsBackground = true;
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

        //This method is called periodically by the monitor thread
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

            this.NotifyApplicationMonitorDataUpdated();
        }

        private void NotifyApplicationMonitorDataUpdated()
        {
            if(this.ApplicationMonitorDataUpdated != null)
            {
                this.ApplicationMonitorDataUpdated();
            }
        }

        public Dictionary<int, ApplicationDetails> GetAllApplicationDetails()
        {
            lock (this.monitorLock)
            {
                //clone the db wit a deep copy
                Dictionary<int, ApplicationDetails> clone = new Dictionary<int, ApplicationDetails>();
                foreach (KeyValuePair<int, ApplicationDetails> originalEntry in this.applicationDetailsDB)
                {
                    clone.Add(originalEntry.Key, (ApplicationDetails) originalEntry.Value.Clone());
                }
                return clone;                
            }
        }        

        //Handler called by anyone to insert the new application details in the Monitor DB
        private void AppOpenedEventHandler(int appId)
        {
            lock (this.monitorLock)
            {
                if (!this.applicationDetailsDB.ContainsKey(appId))
                {
                    ApplicationInfo applicationInfo = this.dataSource.GetApplicationInfo(appId);
                    ApplicationDetails applicationDetails = new ApplicationDetails(applicationInfo);
                    this.applicationDetailsDB.Add(applicationDetails.Id, applicationDetails);
                }
                else
                {
                    throw new Exception("impossible to add application with id " + appId + " already in the db");
                }
            }
        }

        //Handler called by anyone to remove the details of a closed application from the Monitor DB
        private void AppClosedEventHandler(int appId)
        {
            lock (this.monitorLock)
            {
                if (this.applicationDetailsDB.ContainsKey(appId))
                {
                   this.applicationDetailsDB.Remove(appId);
                }
                else
                {
                    throw new Exception("impossible to remove application with id " + appId + " not present in the db");
                }
            }            
        }

        //Handler called by anyone to update the application which holds the focus
        private void FocusChangeEventHandler(int previousFocusAppId, int currentFocusAppId)
        {
            lock (this.monitorLock)
            {
                if (this.applicationDetailsDB.ContainsKey(previousFocusAppId) && this.applicationDetailsDB.ContainsKey(currentFocusAppId))
                {
                    this.applicationDetailsDB[previousFocusAppId].HasFocus = false;
                    this.applicationDetailsDB[currentFocusAppId].HasFocus = true;
                }
                else
                {
                    throw new Exception("impossible to update focus missing applications in the db");
                }
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
