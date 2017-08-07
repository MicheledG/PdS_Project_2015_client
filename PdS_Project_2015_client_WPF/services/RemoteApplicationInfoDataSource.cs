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
        private Dictionary<Int64, ApplicationInfo> appInfoDB;
        private IConnection remoteEndPoint;
        private bool opened;

        public bool Opened { get => this.opened; }
        public event FailureEventHandler DataSourceFailure;
        public event AppOpenedEventHandler AppOpened;
        public event AppClosedEventHandler AppClosed;
        public event FocusChangeEventHandler FocusChange;        
        public event InitialAppInfoListReadyEventHandler InitialAppInfoListReady;
        

        public RemoteApplicationInfoDataSource(IConnection remoteEndPoint)
        {
            this.appInfoDB = new Dictionary<Int64, ApplicationInfo>();
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
                List<ApplicationInfo> clone = new List<ApplicationInfo>();
                foreach(ApplicationInfo applicationInfo in this.appInfoDB.Values.ToList())
                {
                    clone.Add((ApplicationInfo)applicationInfo.Clone());
                }
                return clone;
            }            
        }

        public ApplicationInfo GetApplicationInfo(Int64 appId)
        {
            lock (this.dbLock)
            {
                if (this.appInfoDB.ContainsKey(appId))
                {
                    return (ApplicationInfo) this.appInfoDB[appId].Clone();
                }
                else
                {
                    throw new Exception("application with id " + appId + " not found in data source db!");
                }
            }
        }        
        
        public void Open()
        {
            if (!this.opened)
            {
                this.opened = true;
                this.remoteEndPoint.Connect();
            }
            else
            {
                throw new Exception("cannot open data source, the data source is already opened!");
            }
                                                    
        }

        public void Close()
        {
            if (this.opened)
            {                
                this.remoteEndPoint.Disconnect();
                this.opened = false;
            }            
        }
     
        private void HandleMessage(string message)
        {
            try
            {
                //extract json from message
                JsonApplicationInfoMessage jsonMsg = Newtonsoft.Json.JsonConvert.DeserializeObject<JsonApplicationInfoMessage>(message);

                //DEBUG START
                this.PrintReceivedMessage(jsonMsg);
                //DEBUG END

                String notificationString = jsonMsg.notification_event;
                if (String.IsNullOrEmpty(notificationString))
                {
                    //it is the initial complete list of apps                                
                    this.PopulateAppInfoDB(jsonMsg.app_list);
                }
                else
                {
                    //it is a notification message                
                    //act depending on the notification type
                    JsonApplicationInfoMessage.NotificationEvent notificationEvent = (JsonApplicationInfoMessage.NotificationEvent)Enum.Parse(typeof(JsonApplicationInfoMessage.NotificationEvent), notificationString);
                    switch (notificationEvent)
                    {
                        case JsonApplicationInfoMessage.NotificationEvent.APP_CREATE:
                            //extract the opened app from the message
                            JsonApplicationInfo jsonApplicationInfo = jsonMsg.app_list[0];
                            //insert the new opened app into the db
                            this.InsertOpenedAppInInfoDB(jsonApplicationInfo);
                            break;
                        case JsonApplicationInfoMessage.NotificationEvent.APP_DESTROY:
                            //extract the closed app id from the message
                            Int64 appId = jsonMsg.app_list[0].app_id;
                            //remove the closed application from the db
                            this.RemoveClosedAppFromInfoDB(appId);
                            break;
                        case JsonApplicationInfoMessage.NotificationEvent.APP_FOCUS:
                            //extract the id of the previous app with focus
                            Int64 previousFocusAppId = jsonMsg.app_list[0].app_id;
                            //extract the id of the current app with focus
                            Int64 currentFocusAppId = jsonMsg.app_list[1].app_id;
                            //update the two application info
                            this.ChangeFocusInInfoDB(previousFocusAppId, currentFocusAppId);
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                this.NotifyDataSourceFailureEvent(e.Message);
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

            this.NotifyInitialAppListReadyEvent();
        }

        private void InsertOpenedAppInInfoDB(JsonApplicationInfo jsonApplicationInfo)
        {
            ApplicationInfo applicationInfo = new ApplicationInfo(jsonApplicationInfo);
            lock (this.dbLock)
            {
                if (!this.appInfoDB.ContainsKey(applicationInfo.Id))
                {
                    this.appInfoDB.Add(applicationInfo.Id, applicationInfo);                    
                }
                else
                {
                    throw new Exception("impossible to add application with id " + applicationInfo.Id + ", it is already in the data source db");
                }
            }

            this.NotifyAppOpenedEvent(applicationInfo.Id);
        }

        private void RemoveClosedAppFromInfoDB(Int64 appId)
        {            
            lock (this.dbLock)
            {
                if (this.appInfoDB.ContainsKey(appId))
                {
                    this.appInfoDB.Remove(appId);                    
                }
                else
                {
                    throw new Exception("impossible to remove application with id " + appId + ", it is not present in the data source db");
                }
            }

            this.NotifyAppClosedEvent(appId);

        }

        private void ChangeFocusInInfoDB(Int64 previousFocusAppId, Int64 currentFocusAppId)
        {
            lock (this.dbLock)
            {
                //if the previous app with focus is in the db yet, change its status
                if (this.appInfoDB.ContainsKey(previousFocusAppId))
                {
                    this.appInfoDB[previousFocusAppId].HasFocus = false;
                }

                //update the new app with focus
                if (this.appInfoDB.ContainsKey(currentFocusAppId))
                {
                    this.appInfoDB[currentFocusAppId].HasFocus = true;
                }
                else
                {
                    throw new Exception("impossible to update focus: missing the current focus application in the data source db");
                }                                
            }

            this.NotifyFocusChangeEvent(previousFocusAppId, currentFocusAppId);
        }

        private void HandleConnectionFailure(string failureDescription)
        {
            Console.WriteLine("Failure in connection layer: " + failureDescription);
            this.NotifyDataSourceFailureEvent(failureDescription);
        }

        private void NotifyDataSourceFailureEvent(string failureDescription)
        {
            if (this.DataSourceFailure != null)
            {
                this.DataSourceFailure(failureDescription);
            }
        }

        private void NotifyInitialAppListReadyEvent()
        {
            if (this.InitialAppInfoListReady != null)
            {
                this.InitialAppInfoListReady();
            }
        }

        private void NotifyAppOpenedEvent(Int64 appId)
        {
            if (this.AppOpened != null)
            {
                this.AppOpened(appId);
            }
        }

        private void NotifyAppClosedEvent(Int64 appId)
        {
            if (this.AppClosed != null)
            {
                this.AppClosed(appId);
            }
        }

        private void NotifyFocusChangeEvent(Int64 previousFocusAppId, Int64 currentFocusAppId)
        {
            if (this.FocusChange != null)
            {
                this.FocusChange(previousFocusAppId, currentFocusAppId);
            }
        }

        //DEBUG FUNCTION!!!
        private void PrintReceivedMessage(JsonApplicationInfoMessage jsonMsg)
        {
            Console.WriteLine("==============================");
            Console.WriteLine("Notification Event: "+jsonMsg.notification_event);
            Console.WriteLine("App number: " + jsonMsg.app_number);
            Console.WriteLine("Applicaiton list:");
            foreach(JsonApplicationInfo app in jsonMsg.app_list)
            {
                Console.WriteLine("*********");
                Console.WriteLine("App id: " + app.app_id);
                Console.WriteLine("App name: " + app.app_name);
                Console.WriteLine("App has focus: " + app.focus);
                Console.WriteLine("App icon: " + app.icon_64);
                Console.WriteLine("*********");
            }
            Console.WriteLine("==============================");
        }

    }
}

