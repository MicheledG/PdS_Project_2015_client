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

namespace PdS_Project_2015_client_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {        
        private IApplicationInfoDataSource applicationDataSource;
        private IApplicationMonitor applicationMonitor;
        private Dictionary<int, int> applicationDetailsIndexes;
        private ObservableCollection<ApplicationDetails> applicationDetailsList;
        private DispatcherTimer timer;    


        public MainWindow()
        {

            this.applicationDataSource = new LocalApplicationInfoDataSource();
            this.applicationMonitor = new ApplicationMonitor(this.applicationDataSource);
            this.applicationDetailsIndexes = new Dictionary<int, int>();
            this.applicationDetailsList = new ObservableCollection<ApplicationDetails>();

            this.timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += Timer_Tick;
            

            //Initialize GUI components!
            InitializeComponent();
            this.lvApplicationDetails.ItemsSource = this.applicationDetailsList;

        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            this.UpdateApplicationDetailsList();
        }

        private void StartApplicationMonitor_CanExecute(Object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !this.applicationMonitor.HasStarted;
        }

        private void StartApplicationMonitor_Executed(Object sender, ExecutedRoutedEventArgs e)
        {            
            this.applicationMonitor.Start();
            timer.Start();
        }

        private void StopApplicationMonitor_CanExecute(Object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.applicationMonitor.HasStarted;
        }

        private void StopApplicationMonitor_Executed(Object sender, ExecutedRoutedEventArgs e)
        {
            this.applicationMonitor.Stop();
            timer.Stop();
        }

        //update the observale list of the application details
        private void UpdateApplicationDetailsList()
        {                        
            
            //get the latest application details from the application monitor
            Dictionary<int, ApplicationDetails> applicationDetails = this.applicationMonitor.GetAllApplicationDetails();

            //remove the closed apps from the application details shown in the GUI 
            for (int i = 0; i < this.applicationDetailsIndexes.Count; i++)
            {
                KeyValuePair<int, int> indexEntry = this.applicationDetailsIndexes.ElementAt(i);
                if (!applicationDetails.ContainsKey(indexEntry.Key))
                {
                    this.applicationDetailsList.RemoveAt(indexEntry.Value);
                    this.applicationDetailsIndexes.Remove(indexEntry.Key);
                    i--;
                }
            }

            //insert the new opened apps in the application details shown in the GUI and update the details of all the other application
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
    }
}
