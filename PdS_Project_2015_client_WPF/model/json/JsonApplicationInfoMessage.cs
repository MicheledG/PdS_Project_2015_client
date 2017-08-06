using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdS_Project_2015_client_WPF.model.json
{
    
    public class JsonApplicationInfoMessage
    {
        public List<JsonApplicationInfo> app_list { get; set; }
        public int app_number { get; set; }
        public String notification_event { get; set; }
        public enum NotificationEvent
        {
            APP_CREATE,
            APP_DESTROY,
            APP_FOCUS
        }
    }    
}
