using System.Collections.Generic;

namespace PdS_Project_2015_client_WPF.services
{
    public delegate void AppOpenedEventHandler(int appId);
    public delegate void AppClosedEventHandler(int appId);
    public delegate void FocusChangeEventHandler(int previousFocusAppId, int currentFocusAppId);

    public interface IApplicationInfoDataSource
    {
        void Open();
        void Close();
        List<ApplicationInfo> GetAllApplicationInfo();
        ApplicationInfo GetApplicationInfo(int appId); 
        event AppOpenedEventHandler AppOpened;
        event AppClosedEventHandler AppClosed;
        event FocusChangeEventHandler FocusChange;
    }
}