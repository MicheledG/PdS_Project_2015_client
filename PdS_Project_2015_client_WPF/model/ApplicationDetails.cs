using System;
using System.ComponentModel;

namespace PdS_Project_2015_client_WPF.services
{
    public class ApplicationDetails : ApplicationInfo
    {
        private System.TimeSpan timeOnFocus;
        private int timeOnFocusPercentual;
        
        public ApplicationDetails(ApplicationInfo applicationInfo)
        {
            this.Id = applicationInfo.Id;
            this.ProcessName = applicationInfo.ProcessName;
            this.HasFocus = applicationInfo.HasFocus;
            this.timeOnFocus = System.TimeSpan.Zero;
        }

        public TimeSpan TimeOnFocus {
            get => timeOnFocus;
            set
            {
                if(this.timeOnFocus != value)
                {
                    this.timeOnFocus = value;                    
                }                
            }
        }

        public int TimeOnFocusPercentual { 
            get => timeOnFocusPercentual;
            set
            {
                if (this.timeOnFocusPercentual != value)
                {
                    this.timeOnFocusPercentual = value;                    
                }
            }
        }        
    }
}