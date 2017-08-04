using System.Collections.Generic;
using System.ComponentModel;

namespace PdS_Project_2015_client_WPF.services
{    
    public delegate void InitialAppInfoListReadyEventHandler();
    public delegate void AppOpenedEventHandler(int appId);
    public delegate void AppClosedEventHandler(int appId);
    public delegate void FocusChangeEventHandler(int previousFocusAppId, int currentFocusAppId);

    public interface IApplicationInfoDataSource
    {
        void Open();
        void Close();
        bool Opened { get; }
        event FailureEventHandler DataSourceFailure;        
        List<ApplicationInfo> GetAllApplicationInfo();
        ApplicationInfo GetApplicationInfo(int appId);
        event InitialAppInfoListReadyEventHandler InitialAppInfoListReady;
        event AppOpenedEventHandler AppOpened;
        event AppClosedEventHandler AppClosed;
        event FocusChangeEventHandler FocusChange;        
    }
}