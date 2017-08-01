using System;
using System.ComponentModel;

namespace PdS_Project_2015_client_WPF.services
{
    public class ApplicationDetails : INotifyPropertyChanged, ICloneable
    {
        private int id;
        private string processName;
        private bool hasFocus;
        private System.TimeSpan timeOnFocus;
        private double timeOnFocusPercentual;

        private ApplicationDetails(ApplicationDetails applicationDetails) {
            this.id = applicationDetails.Id;
            this.processName = applicationDetails.ProcessName;
            this.hasFocus = applicationDetails.HasFocus;
            this.timeOnFocus = applicationDetails.TimeOnFocus;
            this.timeOnFocusPercentual = applicationDetails.TimeOnFocusPercentual;
        }

        public ApplicationDetails(ApplicationInfo applicationInfo)
        {
            this.id = applicationInfo.Id;
            this.processName = applicationInfo.ProcessName;
            this.hasFocus = applicationInfo.HasFocus;
            this.timeOnFocus = System.TimeSpan.Zero;
            this.timeOnFocusPercentual = 0;
        }

        public int Id {
            get => id;
            set
            {
                if (this.id != value)
                {
                    id = value;
                    this.NotifyPropertyChanged("Id");
                }                
            }
        }

        public string ProcessName {
            get => processName;
            set
            {
                if (this.processName != value)
                {
                    processName = value;
                    this.NotifyPropertyChanged("ProcessName");
                }
            }
        }

        public bool HasFocus
        {
            get => hasFocus;
            set
            {
                if(this.hasFocus != value)
                {
                    hasFocus = value;
                    this.NotifyPropertyChanged("HasFocus");
                }                
            }
        }

        public TimeSpan TimeOnFocus {
            get => timeOnFocus;
            set
            {
                if(this.timeOnFocus != value)
                {
                    this.timeOnFocus = value;
                    this.NotifyPropertyChanged("TimeOnFocus");
                }                
            }
        }

        public double TimeOnFocusPercentual { 
            get => timeOnFocusPercentual;
            set
            {
                if (this.timeOnFocusPercentual != value)
                {
                    this.timeOnFocusPercentual = value;
                    this.NotifyPropertyChanged("TimeOnFocusPercentual");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public object Clone()
        {
            return new ApplicationDetails(this);
        }

        private void NotifyPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}