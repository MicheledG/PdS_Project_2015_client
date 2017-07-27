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
                    this.appInfoDB[this.focusIndex].HasFocus = false;
                    previousFocusAppId = this.focusIndex;
                    focusIndex++;
                    if (focusIndex > 3)
                    {
                        focusIndex = 1;
                    }
                    this.appInfoDB[this.focusIndex].HasFocus = true;
                    currentFocusAppId = focusIndex;
                }
                
                this.NotifyFocusChangeEvent(previousFocusAppId, currentFocusAppId);
                System.Threading.Thread.Sleep(1000);
            }
        }

        public void Open()
        {
            this.opened = true;
            this.dataSourceThread = new System.Threading.Thread(this.UpdatingDataSource);
            this.dataSourceThread.Start();
        }

        public void Close()
        {
            this.opened = false;
            this.dataSourceThread.Join();
        }
    }
}
