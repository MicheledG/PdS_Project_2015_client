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

namespace PdS_Project_2015_client_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IApplicationDataSource applicationDataSource;
        private IApplicationMonitor applicationMonitor;

        public MainWindow()
        {
            InitializeComponent();

            this.applicationDataSource = new LocalApplicationDataSource();
            this.applicationMonitor = new ApplicationMonitor(this.applicationDataSource);
        }
    }
}
