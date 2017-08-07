using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace PdS_Project_2015_client_WPF.services
{    
    public delegate void InitialAppInfoListReadyEventHandler();
    public delegate void AppOpenedEventHandler(Int64 appId);
    public delegate void AppClosedEventHandler(Int64 appId);
    public delegate void FocusChangeEventHandler(Int64 previousFocusAppId, Int64 currentFocusAppId);

    public interface IApplicationInfoDataSource
    {
        void Open();
        void Close();
        bool Opened { get; }
        event FailureEventHandler DataSourceFailure;        
        List<ApplicationInfo> GetAllApplicationInfo();
        ApplicationInfo GetApplicationInfo(Int64 appId);
        event InitialAppInfoListReadyEventHandler InitialAppInfoListReady;
        event AppOpenedEventHandler AppOpened;
        event AppClosedEventHandler AppClosed;
        event FocusChangeEventHandler FocusChange;        
    }
}