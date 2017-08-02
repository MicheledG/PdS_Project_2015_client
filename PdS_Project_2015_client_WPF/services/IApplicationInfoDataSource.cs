using System.Collections.Generic;
using System.ComponentModel;

namespace PdS_Project_2015_client_WPF.services
{
    public delegate void StatusChangedEventHandler();
    public delegate void AppOpenedEventHandler(int appId);
    public delegate void AppClosedEventHandler(int appId);
    public delegate void FocusChangeEventHandler(int previousFocusAppId, int currentFocusAppId);

    public interface IApplicationInfoDataSource
    {
        void Open();
        void Close();
        bool Opened { get; }
        event StatusChangedEventHandler StatusChanged;        
        List<ApplicationInfo> GetAllApplicationInfo();
        ApplicationInfo GetApplicationInfo(int appId); 
        event AppOpenedEventHandler AppOpened;
        event AppClosedEventHandler AppClosed;
        event FocusChangeEventHandler FocusChange;        
    }
}