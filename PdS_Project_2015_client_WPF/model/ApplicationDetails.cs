﻿using System;

namespace PdS_Project_2015_client_WPF.services
{
    public class ApplicationDetails : ApplicationInfo
    {
        private System.TimeSpan timeOnFocus;
        
        public ApplicationDetails(ApplicationInfo applicationInfo)
        {
            this.Id = applicationInfo.Id;
            this.ProcessName = applicationInfo.ProcessName;
            this.HasFocus = applicationInfo.HasFocus;
            this.timeOnFocus = System.TimeSpan.Zero;
        }

        public TimeSpan TimeOnFocus { get => timeOnFocus; set => timeOnFocus = value; }
    }
}