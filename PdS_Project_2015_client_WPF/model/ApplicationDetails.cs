using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows;

namespace PdS_Project_2015_client_WPF.services
{
    public class ApplicationDetails : INotifyPropertyChanged, ICloneable
    {
        private Int64 id;
        private Int64 processId;
        private string processName;
        private bool hasFocus;
        private System.TimeSpan timeOnFocus;
        private double timeOnFocusPercentual;
        private System.Windows.Media.Imaging.BitmapImage icon;

        private ApplicationDetails(ApplicationDetails applicationDetails) {
            this.id = applicationDetails.Id;
            this.processId = applicationDetails.ProcessId;
            this.processName = applicationDetails.ProcessName;
            this.hasFocus = applicationDetails.HasFocus;
            this.timeOnFocus = applicationDetails.TimeOnFocus;
            this.timeOnFocusPercentual = applicationDetails.TimeOnFocusPercentual;
            this.Icon = applicationDetails.Icon;
        }

        public ApplicationDetails(ApplicationInfo applicationInfo)
        {
            this.id = applicationInfo.Id;
            this.processId = applicationInfo.ProcessId;
            this.processName = applicationInfo.ProcessName;
            this.hasFocus = applicationInfo.HasFocus;
            this.timeOnFocus = System.TimeSpan.Zero;
            this.timeOnFocusPercentual = 0;
            //need to do this action in GUI thread because BitmapImages are bad boys
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                this.Icon = this.fromBase64ToImage(applicationInfo.Icon64);                
            }));            
        }

        public Int64 Id {
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

        public Int64 ProcessId
        {
            get => processId;
            set
            {
                if (this.processId != value)
                {
                    processId = value;
                    this.NotifyPropertyChanged("ProcessId");
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

        private System.Windows.Media.Imaging.BitmapImage fromBase64ToImage(string imageBase64)
        {
            System.Windows.Media.Imaging.BitmapImage image;
            try
            {
                image = new System.Windows.Media.Imaging.BitmapImage();
                byte[] data = System.Convert.FromBase64String(imageBase64);
                var stream = new System.IO.MemoryStream(data, 0, data.Length);
                image = new System.Windows.Media.Imaging.BitmapImage();
                image.BeginInit();
                image.StreamSource = stream;
                image.EndInit();
            }
            catch (System.Exception)
            {
                image = (System.Windows.Media.Imaging.BitmapImage)System.Windows.Application.Current.FindResource("MissingIconImage");
            }
            return image;
        }
    }
}