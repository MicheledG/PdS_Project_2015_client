using PdS_Project_2015_client_WPF.services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Globalization;
using System.ComponentModel;
using PdS_Project_2015_client_WPF.model;

namespace PdS_Project_2015_client_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public delegate void FailureEventHandler(string failureDescription);

    public class GlobalConst
    {
        public const int MIN_PORT = 1;
        public const int MAX_PORT = 65000;
        public const int DEFAULT_PORT = 27015;
    }

    public partial class MainWindow : Window
    {
        private const int GUI_REFRESH_RATE = 500; //ms
        private IApplicationInfoDataSource applicationDataSource;
        private IApplicationMonitor applicationMonitor;
        private IConnection connection;
        private IKeysSender keysSender;
        private Dictionary<int, int> applicationDetailsIndexes;
        private ObservableCollection<ApplicationDetails> applicationDetailsList;
        private DispatcherTimer timer;        

        public int HostAddressByte0 { get; set; }
        public int HostAddressByte1 { get; set; }
        public int HostAddressByte2 { get; set; }
        public int HostAddressByte3 { get; set; }
        public int HostPort { get; set; }        

        public MainWindow()
        {
            this.connection = new SocketConnection();
            this.applicationDataSource = new RemoteApplicationInfoDataSource(this.connection);
            this.applicationMonitor = new ApplicationMonitor(this.applicationDataSource);
            this.keysSender = new KeysSender(this.connection);
            this.applicationDetailsIndexes = new Dictionary<int, int>();
            this.applicationDetailsList = new ObservableCollection<ApplicationDetails>();
            
            //set default value for the Host address and Host port
            this.HostAddressByte0 = 127;
            this.HostAddressByte1 = 0;
            this.HostAddressByte2 = 0;
            this.HostAddressByte3 = 1;
            this.HostPort = GlobalConst.DEFAULT_PORT;

            this.timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(GUI_REFRESH_RATE);
            timer.Tick += Timer_Tick;

            //Initialize GUI components!
            InitializeComponent();
            this.DataContext = this;
            this.lvApplicationDetails.ItemsSource = this.applicationDetailsList;
            this.applicationMonitor.ApplicationMonitorFailure += this.ApplicationMonitorFailureEventHandler;
            this.applicationMonitor.PropertyChanged += this.ApplicationMonitorStatusChangeEventHandler;
            this.keysSender.NewKeyInShortcut += this.KeysSenderNewKeyInShortcutEventHandler;
            this.tbSendKeys.IsEnabled = false;            
        }
        
        private void Timer_Tick(object sender, EventArgs e)
        {
            this.UpdateGui();
        }
        
        //update all the GUI components
        private void UpdateGui()
        {
            this.UpdateApplicationMontiorActiveTime();
            this.UpdateApplicationDetailsList();
        }

        //update the application monitor active time
        private void UpdateApplicationMontiorActiveTime()
        {
            this.tbApplicationMonitorActiveTime.Text = this.applicationMonitor.ActiveTime.ToString(@"hh\:mm\:ss");
        }

        //update the observale list of the application details
        private void UpdateApplicationDetailsList()
        {

            //get the latest application details from the application monitor
            Dictionary<int, ApplicationDetails> applicationDetails = this.applicationMonitor.GetAllApplicationDetails();
            int i;

            //remove the closed apps from the application details shown in the GUI 
            for (i = 0; i < this.applicationDetailsIndexes.Count; i++)
            {
                KeyValuePair<int, int> indexEntry = this.applicationDetailsIndexes.ElementAt(i);
                if (!applicationDetails.ContainsKey(indexEntry.Key))
                {
                    this.applicationDetailsList.RemoveAt(indexEntry.Value);
                    this.applicationDetailsIndexes.Remove(indexEntry.Key);
                    i--;
                }
            }

            //update the indexes to the application details
            this.applicationDetailsIndexes.Clear();
            for (i = 0; i < this.applicationDetailsList.Count; i++)
            {
                this.applicationDetailsIndexes.Add(this.applicationDetailsList[i].Id, i);
            }

            //enqueue the new opened apps in the application details shown in the GUI and update the details of all the other application
            foreach (KeyValuePair<int, ApplicationDetails> entry in applicationDetails)
            {
                if (!this.applicationDetailsIndexes.ContainsKey(entry.Key))
                {
                    //insert the new opened app
                    ApplicationDetails openedApplication = entry.Value;                    
                    this.applicationDetailsList.Add(openedApplication);
                    this.applicationDetailsIndexes.Add(openedApplication.Id, this.applicationDetailsList.Count - 1);
                }
                else
                {
                    //update the details of the application
                    int appIndex = this.applicationDetailsIndexes[entry.Key];
                    this.applicationDetailsList[appIndex].HasFocus = entry.Value.HasFocus;
                    this.applicationDetailsList[appIndex].TimeOnFocus = entry.Value.TimeOnFocus;
                    this.applicationDetailsList[appIndex].TimeOnFocusPercentual = entry.Value.TimeOnFocusPercentual;
                }
            }
            
        }

        //handle the failures of the application monitor
        private void ApplicationMonitorFailureEventHandler(string failureDescription)
        {
            Console.WriteLine("Failure in application monitor layer: " + failureDescription);
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                this.applicationMonitor.Stop();
                if (this.timer.IsEnabled)
                {
                    this.timer.Stop();
                }
                MessageBox.Show("Failure in application monitor layer: " + failureDescription, "Failure", MessageBoxButton.OK, MessageBoxImage.Error);                
            }));            
        }

        //handle the change of status of the application monitor
        private void ApplicationMonitorStatusChangeEventHandler(object sender, PropertyChangedEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (this.applicationMonitor.IsActive)
                {
                    this.imgConnectionStatus.Source = (System.Windows.Media.Imaging.BitmapImage)System.Windows.Application.Current.FindResource("ConnectedImage");
                    this.tbConnectionStatus.Text = "Connected";
                    this.tbHostAddressByte0.IsEnabled = false;
                    this.tbHostAddressByte1.IsEnabled = false;
                    this.tbHostAddressByte2.IsEnabled = false;
                    this.tbHostAddressByte3.IsEnabled = false;
                    this.tbHostPort.IsEnabled = false;
                    this.tbSendKeys.IsEnabled = true;
                }
                else
                {
                    this.imgConnectionStatus.Source = (System.Windows.Media.Imaging.BitmapImage)System.Windows.Application.Current.FindResource("DisconnectedImage");
                    this.tbConnectionStatus.Text = "Disconnected";
                    this.tbHostAddressByte0.IsEnabled = true;
                    this.tbHostAddressByte1.IsEnabled = true;
                    this.tbHostAddressByte2.IsEnabled = true;
                    this.tbHostAddressByte3.IsEnabled = true;
                    this.tbHostPort.IsEnabled = true;
                    this.tbSendKeys.IsEnabled = false;
                }

                //to update button status
                CommandManager.InvalidateRequerySuggested();
            }));            
        }
        
        private void tbSendKeys_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            this.keysSender.HandleKeyDown(e);
        }

        private void tbSendKeys_KeyDown(object sender, KeyEventArgs e)
        {
            this.keysSender.HandleKeyDown(e);
        }

        private void tbSendKeys_KeyUp(object sender, KeyEventArgs e)
        {            
            this.keysSender.HandleKeyUp(e);           
        }

        //shows in the GUI the shortcut beeing created
        private void KeysSenderNewKeyInShortcutEventHandler(KeyAndAction keyAndAction)
        {
            if (this.tbSendKeys.Text.Length > 0)
            {
                this.tbSendKeys.AppendText(" +");
            }

            this.tbSendKeys.AppendText(" " + keyAndAction.Key);

            if (keyAndAction.IsDown)
            {
                this.tbSendKeys.AppendText(" (DOWN)");
            }
            else
            {
                this.tbSendKeys.AppendText(" (UP)");
            }            
        }

        private void StartApplicationMonitor_CanExecute(Object sender, CanExecuteRoutedEventArgs e)
        {
            bool canExecute = !this.applicationMonitor.IsActive;
            canExecute = canExecute && !String.IsNullOrEmpty(this.tbHostAddressByte0.Text);
            canExecute = canExecute && !Validation.GetHasError(this.tbHostAddressByte0);
            canExecute = canExecute && !String.IsNullOrEmpty(this.tbHostAddressByte1.Text);
            canExecute = canExecute && !Validation.GetHasError(this.tbHostAddressByte1);
            canExecute = canExecute && !String.IsNullOrEmpty(this.tbHostAddressByte2.Text);
            canExecute = canExecute && !Validation.GetHasError(this.tbHostAddressByte2);
            canExecute = canExecute && !String.IsNullOrEmpty(this.tbHostAddressByte3.Text);
            canExecute = canExecute && !Validation.GetHasError(this.tbHostAddressByte3);
            canExecute = canExecute && !String.IsNullOrEmpty(this.tbHostPort.Text);
            canExecute = canExecute && !Validation.GetHasError(this.tbHostPort);

            e.CanExecute = canExecute;
        }

        private void StartApplicationMonitor_Executed(Object sender, ExecutedRoutedEventArgs e)
        {
            //extract the host address and the host port in the right format from the GUI
            string hostAddress = "" + this.HostAddressByte0 + ".";
            hostAddress += this.HostAddressByte1 + ".";
            hostAddress += this.HostAddressByte2 + ".";
            hostAddress += this.HostAddressByte3;
            int hostPort = this.HostPort;

            this.connection.EndPointAddress = hostAddress;
            this.connection.EndPointPort = hostPort;

            //initialize the list
            this.applicationDetailsList.Clear();
            this.applicationDetailsIndexes.Clear();

            //let's the application monitor start
            try
            {
                this.applicationMonitor.Start();
                timer.Start();
            }
            catch (Exception exception)
            {
                Console.WriteLine("Exception starting application monitor: " + exception.Message);
                MessageBox.Show("Exception starting application monitor: " + exception.Message, "Failure", MessageBoxButton.OK, MessageBoxImage.Error);
                this.applicationMonitor.Stop();
                if (this.timer.IsEnabled)
                {
                    this.timer.Stop();
                }
            }

        }

        private void StopApplicationMonitor_CanExecute(Object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.applicationMonitor.IsActive;
        }

        private void StopApplicationMonitor_Executed(Object sender, ExecutedRoutedEventArgs e)
        {
            this.applicationMonitor.Stop();
            timer.Stop();
        }

        private void SendKeys_CanExecute(Object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.keysSender.IsShortcutSendable && this.applicationMonitor.IsActive;            
        }

        private void SendKeys_Executed(Object sender, ExecutedRoutedEventArgs e)
        {
            this.tbSendKeys.Clear();

            try
            {
                this.keysSender.Send();
            }
            catch (Exception exception)
            {
                MessageBox.Show("Exception sending key shortcut: " + exception.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            this.keysSender.Clear();
        }

        private void CancelKeys_CanExecute(Object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.keysSender.IsShortcutSendable && this.applicationMonitor.IsActive;
        }

        private void CancelKeys_Executed(Object sender, ExecutedRoutedEventArgs e)
        {
            this.tbSendKeys.Clear();
            this.keysSender.Clear();
        }
        
    }

    public static class CustomCommands
    {
        //Define commands here!
        public static readonly RoutedUICommand StartApplicationMonitor = new RoutedUICommand
                (
                        "Start Application Monitor",
                        "Start Application Monitor",
                        typeof(CustomCommands)
                );
        public static readonly RoutedUICommand StopApplicationMonitor = new RoutedUICommand
                (
                        "Stop Application Monitor",
                        "Stop Application Monitor",
                        typeof(CustomCommands)
                );
        public static readonly RoutedUICommand SendKeys = new RoutedUICommand
                (
                        "Send Keys",
                        "Send Keys",
                        typeof(CustomCommands)
                );
        public static readonly RoutedUICommand CancelKeys = new RoutedUICommand
                (
                        "Cancel Keys",
                        "Cancel Keys",
                        typeof(CustomCommands)
                );

    }    

    public class IpAddressByteValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string input = (string)value;
            if (String.IsNullOrEmpty(input))
            {                
                return new ValidationResult(false, "empty byte!");
            }

            int ipAddressByte;
            if (!Int32.TryParse(input, out ipAddressByte))
            {
                return new ValidationResult(false, "the value typed must be an integer!");
            }
            
            if(ipAddressByte < 0 || ipAddressByte > 255)
            {
                return new ValidationResult(false, "the integer typed must range between 1 and 255!");
            }
            
            return new ValidationResult(true, null);
        }
    }

    public class PortValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string input = (string)value;
            if (String.IsNullOrEmpty(input))
            {
                return new ValidationResult(false, "empty port value!");
            }

            int port;
            if (!Int32.TryParse(input, out port))
            {
                return new ValidationResult(false, "the value typed must be an integer!");
            }

            if (port < GlobalConst.MIN_PORT || port > GlobalConst.MAX_PORT)
            {
                return new ValidationResult(false, "the integer typed must range between "+ GlobalConst.MIN_PORT + " and "+ GlobalConst.MAX_PORT + "!");
            }

            return new ValidationResult(true, null);
        }
    }

}
