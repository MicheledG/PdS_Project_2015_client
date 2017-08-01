using System;
using System.Collections.Generic;
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

        public event AppOpenedEventHandler AppOpened;
        public event AppClosedEventHandler AppClosed;
        public event FocusChangeEventHandler FocusChange;

        public LocalApplicationInfoDataSource()
        {
            this.appInfoDB = new Dictionary<int, ApplicationInfo>();
            this.dbLock = new Object();
            this.opened = false;
            
            //initialize db in memory
            focusIndex = 1;
            appInfoDB.Add(1, new ApplicationInfo() { Id = 1, ProcessName = "chrome", HasFocus = false });
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
            this.opened = true;
            this.dataSourceThread = new System.Threading.Thread(this.UpdatingDataSource);
            this.dataSourceThread.IsBackground = true;
            this.dataSourceThread.Start();
        }

        public void Close()
        {
            this.opened = false;
            this.dataSourceThread.Join();
        }
    }
}
