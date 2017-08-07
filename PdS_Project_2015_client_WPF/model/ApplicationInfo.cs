using PdS_Project_2015_client_WPF.model.json;
using System;
using System.Windows;

namespace PdS_Project_2015_client_WPF.services
{
    public class ApplicationInfo : ICloneable
    {
        private Int64 id;
        private Int64 processId;
        private string processName;
        private bool hasFocus;
        private string icon64;

        public Int64 Id { get => id; set => id = value; }
        public Int64 ProcessId { get => processId; set => processId = value; }
        public string ProcessName { get => processName; set => processName = value; }
        public bool HasFocus { get => hasFocus; set => hasFocus = value; }
        public string Icon64 {get => icon64; set => icon64 = value; }        

        public ApplicationInfo()
        {
            this.id = -1;            
        }

        private ApplicationInfo(ApplicationInfo applicationInfo)
        {
            this.id = applicationInfo.Id;
            this.processId = applicationInfo.ProcessId;
            this.processName = applicationInfo.ProcessName;
            this.hasFocus = applicationInfo.HasFocus;            
            this.icon64 = applicationInfo.Icon64;
        }

        public ApplicationInfo(JsonApplicationInfo jsonApplicationInfo)
        {
            this.id = jsonApplicationInfo.app_id;
            this.processId = jsonApplicationInfo.process_id;
            this.processName = jsonApplicationInfo.app_name;
            this.hasFocus = jsonApplicationInfo.focus;
            this.icon64 = jsonApplicationInfo.icon_64;
        }        

        public object Clone()
        {
            return new ApplicationInfo(this);
        }
    }
}