using PdS_Project_2015_client_WPF.services;
using System;
using System.Collections.Generic;
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
                
        public MainWindow()
        {

            this.applicationDataSource = new LocalApplicationInfoDataSource();
            this.applicationMonitor = new ApplicationMonitor(this.applicationDataSource);
            this.applicationMonitor.ApplicationMonitorDataUpdated += () => { Application.Current.Dispatcher.Invoke(this.UpdateApplicationMonitorDataShown); };

            //Initialize GUI components!
            InitializeComponent();

        }

        private void StartApplicationMonitor_CanExecute(Object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !this.applicationMonitor.HasStarted;
        }

        private void StartApplicationMonitor_Executed(Object sender, ExecutedRoutedEventArgs e)
        {            
            this.applicationMonitor.Start();                                    
        }

        private void StopApplicationMonitor_CanExecute(Object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.applicationMonitor.HasStarted;
        }

        private void StopApplicationMonitor_Executed(Object sender, ExecutedRoutedEventArgs e)
        {
            this.applicationMonitor.Stop();
            this.lvApplicationDetails.ItemsSource = null;
            this.tbApplicationMonitorActiveTime.Text = null;
        }

        private void UpdateApplicationMonitorDataShown()
        {
            //update the list of the application
            this.lvApplicationDetails.ItemsSource = this.applicationMonitor.GetAllApplicationDetails();
            //update the active time of the monitor
            this.tbApplicationMonitorActiveTime.Text = this.applicationMonitor.ActiveTime.ToString();
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
