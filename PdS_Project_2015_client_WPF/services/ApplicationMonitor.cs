using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdS_Project_2015_client_WPF.services
{
    class ApplicationMonitor : IApplicationMonitor
    {
        private const int UPDATE_RATE = 100; //ms

        private bool active;
        private System.DateTime startingTime;        
        private System.DateTime lastUpdateTime;
        private TimeSpan activeTime;
        private System.Threading.Thread monitorThread;
        //protects all the data accessible from different thread (dued to Event handlers, GUI thread and background thread)
        private Object monitorLock;
        private IApplicationInfoDataSource dataSource;
        private Dictionary<Int64, ApplicationDetails> applicationDetailsDB;
               
        public event FailureEventHandler ApplicationMonitorFailure;
        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsActive { get => this.active;
            private set
            {
                if(this.active != value)
                {
                    this.active = value;
                    this.NotifyPropertyChanged("IsActive");
                }
            }
        }        

        public TimeSpan ActiveTime { get => activeTime; set => activeTime = value; }

        public ApplicationMonitor(IApplicationInfoDataSource dataSource)
        {
            this.IsActive = false;
            this.applicationDetailsDB = new Dictionary<Int64, ApplicationDetails>();
            this.monitorLock = new Object();
            this.dataSource = dataSource;
            this.dataSource.DataSourceFailure += this.DataSourceFailureEventHandler;
            this.dataSource.InitialAppInfoListReady += this.InitialAppInfoListReadyEventHandler;
            this.dataSource.AppOpened += this.AppOpenedEventHandler;
            this.dataSource.AppClosed += this.AppClosedEventHandler;
            this.dataSource.FocusChange += this.FocusChangeEventHandler;
            
        }                         

        public void Start()
        {
            if (!this.active)
            {
                this.IsActive = true;
                this.startingTime = System.DateTime.Now;
                this.lastUpdateTime = this.startingTime;
                this.activeTime = System.TimeSpan.Zero;
                this.applicationDetailsDB.Clear();

                this.monitorThread = new System.Threading.Thread(this.MonitorApplications);
                this.monitorThread.IsBackground = true; //usefull during application exit
                this.monitorThread.Start();

                this.dataSource.Open();
            }
            else
            {                
                throw new Exception("cannot start application monitor, the application monitor is already active!");
            }            
        }        

        public void Stop()
        {
            if (this.active)
            {
                //stop the application monitor thread
                this.IsActive = false;
                if (this.monitorThread != null && this.monitorThread.IsAlive)
                {
                    this.monitorThread.Join();
                    this.monitorThread = null;
                }

                //close the data source (stop)
                this.dataSource.Close();                                               
            }
        }
        
        //Background method executed in a different thread that updates periodically the information contained into the db
        private void MonitorApplications()
        {
            while (this.IsActive)
            {
                UpdateMonitorData();
                System.Threading.Thread.Sleep(UPDATE_RATE);                
            }

            Console.WriteLine("Application monitor thread says:'Bye Bye :-)'");
        }

        //This method is called periodically by the monitor thread
        private void UpdateMonitorData()
        {
            //this method shouldn't throw exceptions!!!
            lock (this.monitorLock)
            {
                System.DateTime updateTime = System.DateTime.Now;
                this.activeTime = updateTime - this.startingTime;
            
                foreach (KeyValuePair<Int64, ApplicationDetails> dbEntry in this.applicationDetailsDB)
                {
                    if (dbEntry.Value.HasFocus)
                    {
                        //update absolute time on focus
                        dbEntry.Value.TimeOnFocus += (updateTime - this.lastUpdateTime);
                    }
                    //update percentual time on focus
                    dbEntry.Value.TimeOnFocusPercentual = (dbEntry.Value.TimeOnFocus.TotalMilliseconds / this.activeTime.TotalMilliseconds);
                }
                        
                this.lastUpdateTime = updateTime;
            }
    
        }
                
        public Dictionary<Int64, ApplicationDetails> GetAllApplicationDetails()
        {
            lock (this.monitorLock)
            {
                //clone the db wit a deep copy
                Dictionary<Int64, ApplicationDetails> clone = new Dictionary<Int64, ApplicationDetails>();
                foreach (KeyValuePair<Int64, ApplicationDetails> originalEntry in this.applicationDetailsDB)
                {
                    clone.Add(originalEntry.Key, (ApplicationDetails) originalEntry.Value.Clone());
                }
                return clone;                
            }
        }

        //Handler called by the data source to notify a failure in the data source layer
        private void DataSourceFailureEventHandler(string failureDescription)
        {
            Console.WriteLine("Failure in data source layer: " + failureDescription);
            this.NotifyApplicationMonitorFailure(failureDescription);
        }

        //Handler called by the data source to insert the initial complete list of application info in the application monitor DB
        private void InitialAppInfoListReadyEventHandler()
        {
            lock (this.monitorLock)
            {
                this.applicationDetailsDB.Clear();
                List<ApplicationInfo> allApplicationInfo = this.dataSource.GetAllApplicationInfo();
                foreach (ApplicationInfo applicationInfo in allApplicationInfo)
                {
                    ApplicationDetails applicationDetails = new ApplicationDetails(applicationInfo);
                    this.applicationDetailsDB.Add(applicationDetails.Id, applicationDetails);
                }
            }
        }

        //Handler called by the data source to insert the new application details in the application monitor DB
        private void AppOpenedEventHandler(Int64 appId)
        {
            try
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
                        throw new Exception("impossible to add application with id " + appId + ", it is already in the monitor db");
                    }
                }
            }            
            catch (Exception e)
            {
                this.NotifyApplicationMonitorFailure(e.Message);
            }
        }

        //Handler called by anyone to remove the details of a closed application from the Monitor DB
        private void AppClosedEventHandler(Int64 appId)
        {
            try
            {
                lock (this.monitorLock)
                {
                    if (this.applicationDetailsDB.ContainsKey(appId))
                    {
                        this.applicationDetailsDB.Remove(appId);
                    }
                    else
                    {
                        throw new Exception("impossible to remove application with id " + appId + ", it is not present in the monitor db");
                    }
                }
            }
            catch (Exception e)
            {
                this.NotifyApplicationMonitorFailure(e.Message);
            }

        }

        //Handler called by anyone to update the application which holds the focus
        private void FocusChangeEventHandler(Int64 previousFocusAppId, Int64 currentFocusAppId)
        {
            try
            {                
                lock (this.monitorLock)
                {
                    //if the previous app with focus is in the db yet, change its status
                    if (this.applicationDetailsDB.ContainsKey(previousFocusAppId))
                    {
                        this.applicationDetailsDB[previousFocusAppId].HasFocus = false;
                    }

                    //update the new app with focus
                    if (this.applicationDetailsDB.ContainsKey(currentFocusAppId))
                    {
                        this.applicationDetailsDB[currentFocusAppId].HasFocus = true;
                    }
                    else
                    {
                        throw new Exception("impossible to update focus: missing the current focus application in the application monitor db");
                    }                                       
                }
            }
            catch (Exception e)
            {
                this.NotifyApplicationMonitorFailure(e.Message);
            }

        }

        private void NotifyApplicationMonitorFailure(string failureDescription)
        {
            if (this.ApplicationMonitorFailure != null)
            {
                this.ApplicationMonitorFailure(failureDescription);
            }
        }

        private void NotifyPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        //DEBUG FUNCTION!!!
        private void PrintAllApplicationDetails()
        {
            lock (this.monitorLock)
            {
                foreach (KeyValuePair<Int64, ApplicationDetails> dbEntry in this.applicationDetailsDB)
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
