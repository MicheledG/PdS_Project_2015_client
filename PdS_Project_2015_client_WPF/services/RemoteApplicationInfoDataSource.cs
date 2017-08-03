using PdS_Project_2015_client_WPF.model.json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdS_Project_2015_client_WPF.services
{
    class RemoteApplicationInfoDataSource : IApplicationInfoDataSource
    {        
        private Object dbLock;
        private Dictionary<int, ApplicationInfo> appInfoDB;
        private IConnection remoteEndPoint;
        private bool opened;

        public bool Opened
        {
            get => this.opened;
            set
            {
                if (this.opened != value)
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

        public RemoteApplicationInfoDataSource(IConnection remoteEndPoint)
        {
            this.appInfoDB = new Dictionary<int, ApplicationInfo>();
            this.dbLock = new Object();
            this.opened = false;
            this.remoteEndPoint = remoteEndPoint;
            this.remoteEndPoint.ConnectionFailure += HandleConnectionFailure;
            this.remoteEndPoint.MessageReceived += HandleMessage;
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
        
        public void Open()
        {
            //handle exceptions!
            this.remoteEndPoint.Connect();
            this.Opened = true;
        }

        public void Close()
        {
            this.Opened = false;
            this.remoteEndPoint.Disconnect();
        }
     
        private void HandleMessage(string message)
        {
            //extract json from message
            JsonMessage jsonMsg = Newtonsoft.Json.JsonConvert.DeserializeObject<JsonMessage>(message);

            String notificationString = jsonMsg.notification_event;
            if (String.IsNullOrEmpty(notificationString))
            {
                //it is the initial complete list of apps                                
                this.PopulateAppInfoDB(jsonMsg.app_list);                
            }
            else
            {
                //it is a notification message
                try
                {
                    //act depending on the notification type
                    JsonMessage.NotificationEvent notificationEvent = (JsonMessage.NotificationEvent)Enum.Parse(typeof(JsonMessage.NotificationEvent), notificationString);
                    switch (notificationEvent)
                    {
                        case JsonMessage.NotificationEvent.APP_CREATE:
                            //extract the opened app from the message
                            JsonApplicationInfo jsonApplicationInfo = jsonMsg.app_list[0];
                            //insert the new opened app into the db
                            this.InsertOpenedAppInInfoDB(jsonApplicationInfo);                                                                                    
                            break;
                        case JsonMessage.NotificationEvent.APP_DESTROY:
                            //extract the closed app id from the message
                            int appId = jsonMsg.app_list[0].app_id;
                            //remove the closed application from the db
                            this.RemoveClosedAppFromInfoDB(appId);                            
                            break;
                        case JsonMessage.NotificationEvent.APP_FOCUS:
                            //extract the id of the previous app with focus
                            int previousFocusAppId = jsonMsg.app_list[0].app_id;
                            //extract the id of the current app with focus
                            int currentFocusAppId = jsonMsg.app_list[1].app_id;
                            //update the two application info
                            this.ChangeFocusInInfoDB(previousFocusAppId, currentFocusAppId);                            
                            break;
                        default:
                            break;
                    }

                }
                catch (OverflowException)
                {
                    //ignore the message
                    return;
                }
            }
        }
        
        private void PopulateAppInfoDB(List<JsonApplicationInfo> jsonApplicationInfoList)
        {
            lock (this.dbLock)
            {
                this.appInfoDB.Clear();
                foreach (JsonApplicationInfo appJson in jsonApplicationInfoList)
                {
                    ApplicationInfo applicationInfo = new ApplicationInfo(appJson);
                    this.appInfoDB.Add(applicationInfo.Id, applicationInfo);
                }
            }
        }

        private void InsertOpenedAppInInfoDB(JsonApplicationInfo jsonApplicationInfo)
        {
            ApplicationInfo applicationInfo = new ApplicationInfo(jsonApplicationInfo);
            lock (this.dbLock)
            {
                if (!this.appInfoDB.ContainsKey(applicationInfo.Id))
                {
                    this.appInfoDB.Add(applicationInfo.Id, applicationInfo);
                    this.NotifyAppOpenedEvent(applicationInfo.Id);
                }
                else
                {
                    throw new Exception("impossible to add application with id " + applicationInfo.Id + ", it is already in the db");
                }
            }
        }

        private void RemoveClosedAppFromInfoDB(int appId)
        {            
            lock (this.dbLock)
            {
                if (this.appInfoDB.ContainsKey(appId))
                {
                    this.appInfoDB.Remove(appId);
                    this.NotifyAppClosedEvent(appId);
                }
                else
                {
                    throw new Exception("impossible to remove application with id " + appId + ", it is not present in the db");
                }
            }
        }

        private void ChangeFocusInInfoDB(int previousFocusAppId, int currentFocusAppId)
        {
            lock (this.dbLock)
            {
                if (this.appInfoDB.ContainsKey(previousFocusAppId) && this.appInfoDB.ContainsKey(currentFocusAppId))
                {
                    this.appInfoDB[previousFocusAppId].HasFocus = false;
                    this.appInfoDB[currentFocusAppId].HasFocus = true;
                    this.NotifyFocusChangeEvent(previousFocusAppId, currentFocusAppId);
                }
                else
                {
                    throw new Exception("impossible to update focus missing applications in the db");
                }
            }
        }

        private void HandleConnectionFailure(string failureDescription)
        {
            this.Opened = false;
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
            if (this.AppOpened != null)
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

    }
}

