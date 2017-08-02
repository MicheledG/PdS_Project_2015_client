using System;
using System.ComponentModel;
using System.Drawing;

namespace PdS_Project_2015_client_WPF.services
{
    public class ApplicationDetails : INotifyPropertyChanged, ICloneable
    {
        private int id;
        private string processName;
        private bool hasFocus;
        private System.TimeSpan timeOnFocus;
        private double timeOnFocusPercentual;
        private System.Windows.Media.Imaging.BitmapImage icon;

        private ApplicationDetails(ApplicationDetails applicationDetails) {
            this.id = applicationDetails.Id;
            this.processName = applicationDetails.ProcessName;
            this.hasFocus = applicationDetails.HasFocus;
            this.timeOnFocus = applicationDetails.TimeOnFocus;
            this.timeOnFocusPercentual = applicationDetails.TimeOnFocusPercentual;
            this.Icon = applicationDetails.Icon;
        }

        public ApplicationDetails(ApplicationInfo applicationInfo)
        {
            this.id = applicationInfo.Id;
            this.processName = applicationInfo.ProcessName;
            this.hasFocus = applicationInfo.HasFocus;
            this.timeOnFocus = System.TimeSpan.Zero;
            this.timeOnFocusPercentual = 0;
            this.Icon = applicationInfo.fromBase64ToImage();
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

        public System.Windows.Media.Imaging.BitmapImage Icon { get => icon; set => icon = value; }

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