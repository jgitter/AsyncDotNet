using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
using Nito.AsyncEx;

namespace AsyncDeadlocks
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Bttn1_Click(object sender, RoutedEventArgs e)
        {
            DemonstrationMethods.FullySynchronousMethod();
            StatusBlock.Text = "Finished - Synchronous - DEADLOCK!";
        }

        private async void Bttn3_Click(object sender, RoutedEventArgs e)
        {
            var cancelSource = new CancellationTokenSource(TimeSpan.FromSeconds(1));
            Task t = DemonstrationMethods.WrapSynchronousInAsynchronousMethod(cancelSource.Token);

            StatusBlock.Text = "Finished - Synchronous on separate thread";
        }

        private async void Bttn2_Click(object sender, RoutedEventArgs e)
        {
            await DemonstrationMethods.AsyncAllTheWayDownMethod();
            StatusBlock.Text = "Finished - Asynchronous";
        }

        private async void Bttn4_Click(object sender, RoutedEventArgs e)
        {
            DemonstrationMethods.SynchronousWithNoCapturedContinuationMethod();
            StatusBlock.Text = "Finished - Synchronous without captured context";
        }

        private async void Bttn5_Click(object sender, RoutedEventArgs e)
        {
            DemonstrationMethods.SynchronousUsingStephenClearyMethod();
            StatusBlock.Text = "Finished - Synchronous using Stephen Cleary method";
        }
        private async void Bttn6_Click(object sender, RoutedEventArgs e)
        {
            DemonstrationMethods.SynchronousTemporaryContextRemovalMethod();
            StatusBlock.Text = "Finished - Synchronous using safe getter";
        }
    }
}
