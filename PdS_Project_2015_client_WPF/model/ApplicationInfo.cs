using PdS_Project_2015_client_WPF.model.json;
using System;

namespace PdS_Project_2015_client_WPF.services
{
    public class ApplicationInfo
    {
        private int id;
        private string processName;
        private bool hasFocus;
        private string icon64;

        public int Id { get => id; set => id = value; }
        public string ProcessName { get => processName; set => processName = value; }
        public bool HasFocus { get => hasFocus; set => hasFocus = value; }
        public string Icon64 {get => icon64; set => icon64 = value; }        

        public ApplicationInfo()
        {
            this.id = -1;            
        }

        public ApplicationInfo(JsonApplicationInfo jsonApplicationInfo)
        {
            this.id = jsonApplicationInfo.app_id;
            this.processName = jsonApplicationInfo.app_name;
            this.hasFocus = jsonApplicationInfo.focus;
            this.icon64 = jsonApplicationInfo.icon_64;
        }

        public System.Windows.Media.Imaging.BitmapImage fromBase64ToImage()
        {
            System.Windows.Media.Imaging.BitmapImage image;
            try
            {
                byte[] data = System.Convert.FromBase64String(this.icon64);
                var stream = new System.IO.MemoryStream(data, 0, data.Length);
                image = new System.Windows.Media.Imaging.BitmapImage();
                image.BeginInit();
                image.StreamSource = stream;
                image.EndInit();
                return image;
            }
            catch (System.Exception)
            {                                
                image = (System.Windows.Media.Imaging.BitmapImage) System.Windows.Application.Current.FindResource("MissingIconImage");
            }
            return image;
        }

    }
}