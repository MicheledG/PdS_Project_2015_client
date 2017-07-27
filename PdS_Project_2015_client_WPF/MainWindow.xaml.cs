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
        private DispatcherTimer timer;

        public MainWindow()
        {

            this.applicationDataSource = new LocalApplicationInfoDataSource();
            this.applicationMonitor = new ApplicationMonitor(this.applicationDataSource);
            this.timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += UpdateGui;
            
            //Initialize GUI components!
            InitializeComponent();

        }

        private void UpdateGui(object sender, EventArgs e)
        {
            this.lvApplicationDetails.ItemsSource = this.applicationMonitor.GetApplicationDetails();
        }

        private void StartApplicationMonitor_CanExecute(Object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !this.applicationMonitor.HasStarted;
        }

        private void StartApplicationMonitor_Executed(Object sender, ExecutedRoutedEventArgs e)
        {
            this.applicationMonitor.Start();
            this.timer.Start();
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
