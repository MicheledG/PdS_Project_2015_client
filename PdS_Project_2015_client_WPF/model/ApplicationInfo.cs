namespace PdS_Project_2015_client_WPF.services
{
    public class ApplicationInfo
    {
        private int id;
        private string processName;
        private bool hasFocus;

        public int Id { get => id; set => id = value; }
        public string ProcessName { get => processName; set => processName = value; }
        public bool HasFocus { get => hasFocus; set => hasFocus = value; }
    }
}