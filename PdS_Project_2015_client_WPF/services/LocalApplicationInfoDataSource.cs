using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdS_Project_2015_client_WPF.services
{
    class LocalApplicationInfoDataSource : IApplicationInfoDataSource
    {

        private Object dbLock;
        private Dictionary<int, ApplicationInfo> appInfoDB;
        private int focusIndex;

        private bool opened;
        private System.Threading.Thread dataSourceThread;

        public bool Opened {
            get => this.opened;
            set
            {
                if(this.opened != value)
                {
                    this.opened = value;
                    this.NotifyStatusChangedEvent();                    
                }
            }
        }        

        public event AppOpenedEventHandler AppOpened;
        public event AppClosedEventHandler AppClosed;
        public event FocusChangeEventHandler FocusChange;
        public event StatusChangedEventHandler StatusChanged;
        public event InitialAppInfoListReadyEventHandler InitialAppInfoListReady;

        public LocalApplicationInfoDataSource()
        {
            this.appInfoDB = new Dictionary<int, ApplicationInfo>();
            this.dbLock = new Object();
            this.opened = false;
            
            //initialize db in memory
            focusIndex = 1;
            appInfoDB.Add(1, new ApplicationInfo() { Id = 1, ProcessName = "chrome", HasFocus = false, Icon64="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" });
            appInfoDB.Add(2, new ApplicationInfo() { Id = 2, ProcessName = "notepad", HasFocus = false });
            appInfoDB.Add(3, new ApplicationInfo() { Id = 3, ProcessName = "spotify", HasFocus = false });

        }

        public List<ApplicationInfo> GetAllApplicationInfo()
        {
            lock (this.dbLock)
            {
                return this.appInfoDB.Values.ToList();
            }
        }

        
        public ApplicationInfo GetApplicationInfo(int appId)
        {
            lock (this.dbLock)
            {
                if (this.appInfoDB.ContainsKey(appId))
                {
                    return this.appInfoDB[appId];
                }
                else
                {
                    throw new KeyNotFoundException("application with id " + appId + " not found!");
                }
            }            
        }

        private void NotifyStatusChangedEvent()
        {
            if (this.StatusChanged != null)
            {
                this.StatusChanged();
            }
        }

        private void NotifyAppOpenedEvent(int appId)
        {
            if(this.AppOpened != null)
            {
                this.AppOpened(appId);
            }
        }

        private void NotifyAppClosedEvent(int appId)
        {
            if (this.AppClosed != null)
            {
                this.AppClosed(appId);
            }
        }

        private void NotifyFocusChangeEvent(int previousFocusAppId, int currentFocusAppId)
        {
            if (this.FocusChange != null)
            {
                this.FocusChange(previousFocusAppId, currentFocusAppId);
            }
        }

        private void UpdatingDataSource()
        {
            int previousFocusAppId;
            int currentFocusAppId;

            while (this.opened)
            {
                lock (this.dbLock)
                {
                    this.appInfoDB.ElementAt(this.focusIndex).Value.HasFocus = false;
                    previousFocusAppId = this.appInfoDB.ElementAt(this.focusIndex).Value.Id;
                    focusIndex++;
                    if (focusIndex >= this.appInfoDB.Count)
                    {
                        focusIndex = 1;
                    }
                    this.appInfoDB.ElementAt(this.focusIndex).Value.HasFocus = true;
                    currentFocusAppId = this.appInfoDB.ElementAt(this.focusIndex).Value.Id;
                }

                this.NotifyFocusChangeEvent(previousFocusAppId, currentFocusAppId);
                System.Threading.Thread.Sleep(1000);

                //insert a new  random application
                Random randomKeyGenerator = new Random();                
                ApplicationInfo applicationInfo;
                lock (this.dbLock)
                {
                    applicationInfo = new ApplicationInfo()
                    {
                        Id = randomKeyGenerator.Next(1, 1001),
                        ProcessName = "new test process",
                        HasFocus = false
                    };
                    this.appInfoDB.Add(applicationInfo.Id, applicationInfo);
                }

                this.NotifyAppOpenedEvent(applicationInfo.Id);
                System.Threading.Thread.Sleep(1000);

                //remove a random application                
                int randomIndex = randomKeyGenerator.Next(1, this.appInfoDB.Count);
                int randomKey = this.appInfoDB.ElementAt(randomIndex).Key;
                this.appInfoDB.Remove(randomKey);
                this.NotifyAppClosedEvent(randomKey);
                System.Threading.Thread.Sleep(1000);
            }
        }

        public void Open()
        {
            this.Opened = true;
            this.dataSourceThread = new System.Threading.Thread(this.UpdatingDataSource);
            this.dataSourceThread.IsBackground = true;
            this.dataSourceThread.Start();            
        }

        public void Close()
        {
            this.Opened = false;
            this.dataSourceThread.Join();
        }
    }
}
