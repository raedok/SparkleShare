using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SparkleShare
{
    public class UpdateWindow : Window
    {
        private ProgressBar progressBar;
        private Label status;
        private string appPath;

        public UpdateWindow(string appPath)
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            this.appPath = appPath;
            Title = "New update released. Updating...";
            ResizeMode = ResizeMode.NoResize;
            Height = 288;
            Width = 720;
            Icon = UserInterfaceBootloaderHelpers.GetImageSource("sparkleshare-app", "ico");

            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            CreateInstall();
        }


        private void CreateInstall()
        {
            Image image = new Image()
            {
                Width = 720,
                Height = 260
            };

            image.Source = UserInterfaceBootloaderHelpers.GetImageSource("about");


            this.status = new Label()
            {
                Content = "Downloading update...",
                FontSize = 11,
                Foreground = new SolidColorBrush(Colors.White)
            };

            this.progressBar = new ProgressBar()
            {
                FontSize = 11,
                Foreground = new SolidColorBrush(Colors.Green),
                Width = 300,
                Height = 20,
            };

            Canvas canvas = new Canvas();

            canvas.Children.Add(image);
            Canvas.SetLeft(image, 0);
            Canvas.SetTop(image, 0);

            canvas.Children.Add(status);
            Canvas.SetLeft(status, 350);
            Canvas.SetTop(status, 132);

            canvas.Children.Add(progressBar);
            Canvas.SetLeft(progressBar, 350);
            Canvas.SetTop(progressBar, 152);

            Content = canvas;

            new Thread(() =>
            {
                var client = new WebClient();
                client.DownloadProgressChanged += Client_DownloadProgressChanged;

                string app_data_path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string tmpPath = Path.Combine(app_data_path, "org.sparkleshare.SparkleShare", "tmp");
                if (!Directory.Exists(tmpPath)) Directory.CreateDirectory(tmpPath);

                string destinationFile = Path.Combine(app_data_path, "org.sparkleshare.SparkleShare", "tmp", "release.7z");

                client.DownloadFileAsync(new Uri("http://share.harvestiasi.ro/sparkleshare/windows/raw/master/release.7z"), destinationFile);
                client.DownloadFileCompleted += Client_DownloadFileCompleted;
            }).Start();
        }

        private void Client_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                this.status.Content = "Installing downloaded update.";
                this.progressBar.Value = 0;
            });

            string app_data_path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string toolsExe = Path.Combine(app_data_path, "org.sparkleshare.SparkleShare", "tmp", "release.7z");

            string sevenZip_path = Path.Combine(appPath, "7z.exe");
            var proc = Process.Start(new ProcessStartInfo()
            {
                FileName = sevenZip_path,
                Arguments = $"x \"{toolsExe}\" -o\"{ this.appPath }\" -y -bsp1 -bse1 -bso1",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true
            });
            while (!proc.HasExited)
                readLine(proc.StandardOutput);
            proc.WaitForExit();
            File.Delete(toolsExe);
            Dispatcher.Invoke(() =>
            {
                this.Close();
            });
        }

        private void readLine(StreamReader reader)
        {
            var line = reader.ReadLine();
            Console.WriteLine(line);
            if (!string.IsNullOrWhiteSpace(line) && line.Contains("%") && line.Contains("-"))
            {
                try
                {
                    var fileName = line.Substring(line.IndexOf('-') + 1);
                    var progressValue = double.Parse(line.Substring(0, line.IndexOf('%')).Trim());
                    Dispatcher.Invoke(() =>
                    {
                        this.status.Content = fileName;
                        this.progressBar.Value = progressValue;
                    });
                }
                catch { }
            }
        }

        private void Client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                this.progressBar.Value = e.ProgressPercentage;
            });
        }
    }
}
