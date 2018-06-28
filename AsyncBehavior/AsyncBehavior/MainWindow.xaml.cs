using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace AsyncBehavior
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        Stopwatch sw = new Stopwatch();


        private enum RunMode
        {
            NoAwaits, Yield, Delay0, Delay100, Delay0NoCapture, Delay100NoCapture, Delay100ViaTaskRun
        }

        #region EventHandlers
        private async void bttnSync_Click(object sender, RoutedEventArgs e)
        {
            Run(RunMode.NoAwaits);
        }

        private async void bttnYield_Click(object sender, RoutedEventArgs e)
        {
            Run(RunMode.Yield);
        }

        private async void bttnDelay0_Click(object sender, RoutedEventArgs e)
        {
            Run(RunMode.Delay0);
        }

        private async void bttnDelay100_Click(object sender, RoutedEventArgs e)
        {
            Run(RunMode.Delay100);
        }

        private async void bttnDelay0NoCapture_Click(object sender, RoutedEventArgs e)
        {
            Run(RunMode.Delay0NoCapture);
        }

        private async void bttnDelay100NoCapture_Click(object sender, RoutedEventArgs e)
        {
            Run(RunMode.Delay100NoCapture);
        }

        private async void bttnTaskRun_Click(object sender, RoutedEventArgs e)
        {
            Run(RunMode.Delay100ViaTaskRun);
        }
        #endregion


        // EventHandlers call this method with a given test mode (above)
        private async void Run(RunMode mode)
        {
            RootCanvas.Children.Clear();
            txtStatus.Text = "Started";

            sw.Restart();

            Func<Task> redWork = () => DoWorkAsync(Colors.Red, 0, 200, mode);
            Func<Task> blueWork = () => DoWorkAsync(Colors.Blue, 1, 300, mode);
            Func<Task> greenWork = () => DoWorkAsync(Colors.Green, 2, 500, mode);


            // this test mode invokes each "DoWorkAsync" call in a separate thread
            if (mode == RunMode.Delay100ViaTaskRun)
            {
                await Task.WhenAll(
                    Task.Run(() => redWork()),
                    Task.Run(() => blueWork()),
                    Task.Run(() => greenWork()));
            }
            else
            {
                await Task.WhenAll(
                    redWork(),
                    blueWork(),
                    greenWork());
            }

            sw.Stop();

            txtStatus.Text = $"Finished after {sw.ElapsedMilliseconds} ms";
        }

        private async Task DoWorkAsync(Color color, int column, int delay, RunMode mode)
        {
            for (int i = 0; i < 5; i++)
            {
                switch (mode)
                {
                    // the presence of an await (if executed and non-trivial) creates a "continuation point" where the current context,
                    // if one exists, may be captured.  These continuation points are also opportunities for multiple tasks to
                    // be run at the same time.  This may be done on a single thread synchronously or asynchronously (interleaved),
                    // or in parallel on multiple threads depending on several factors, including behavior of downstream async calls.
                    case RunMode.NoAwaits:
                        // do nothing - no chance of a continuation here
                        break;
                    case RunMode.Yield:
                        await Task.Yield();
                        break;
                    case RunMode.Delay0:
                        await Task.Delay(0);
                        break;
                    case RunMode.Delay100:
                    case RunMode.Delay100ViaTaskRun:
                        await Task.Delay(100);
                        break;
                    case RunMode.Delay0NoCapture:
                        await Task.Delay(0).ConfigureAwait(false);
                        break;
                    case RunMode.Delay100NoCapture:
                        await Task.Delay(100).ConfigureAwait(false);
                        break;
                }

                var startTime = sw.ElapsedMilliseconds;

                // spin here for 'delay' milliseconds just to keep busy
                while (sw.ElapsedMilliseconds < startTime + delay) { /* do nothing */ }

                var endTime = sw.ElapsedMilliseconds;

                var threadId = Thread.CurrentThread.ManagedThreadId;

                // intentionally not awaited - UI thread will continue to draw after this method returns
                Dispatcher.BeginInvoke((Action)(() => AddTimeRectangle(startTime, endTime - 5, color, column, threadId)));
            }
        }

        /// <summary>
        /// add a rectangle that whose height & position represents the start and end time of an activity
        /// </summary>
        private void AddTimeRectangle(double startTime, double endTime, Color color, int column, int threadId)
        {
            const int columnWidth = 50;
            Debug.WriteLine("Adding Rect {0} {1} {2} from thread:{3}", startTime, endTime, color, threadId);

            double left = column * (columnWidth * 2 / 3);
            double height = endTime - startTime;

            var r = new Rectangle
            {
                Fill = new SolidColorBrush(color),
                Width = columnWidth,
                Height = height
            };
            Canvas.SetLeft(r, left);
            Canvas.SetTop(r, startTime);

            RootCanvas.Children.Add(r);

            var t = new Label
            {
                Foreground = new SolidColorBrush(Colors.White),
                Content = $"Thread {threadId}; UI Thread {Thread.CurrentThread.ManagedThreadId}",
                FontSize = 3d,
                RenderTransform = new ScaleTransform(1, 30),
                Padding = new Thickness(1)
            };
            Canvas.SetLeft(t, left);
            Canvas.SetTop(t, startTime);
            RootCanvas.Children.Add(t);

            if (RootCanvas.Height < endTime)
            {
                RootCanvas.Height = endTime;
            }
            if (RootCanvas.Width < left + columnWidth)
            {
                RootCanvas.Width = left + columnWidth;
            }
        }
    }
}
