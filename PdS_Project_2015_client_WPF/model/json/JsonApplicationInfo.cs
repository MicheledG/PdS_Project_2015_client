using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdS_Project_2015_client_WPF.model.json
{
    public class JsonApplicationInfo
    {
        public Int64 app_id { get; set; }
        public Int64 process_id { get; set; }
        public String app_name { get; set; }
        public bool focus { get; set; }
        public String icon_64 { get; set; }        
    }
}
