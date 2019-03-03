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
    public class GitInstall : Window
    {

        private ProgressBar progressBar;
        private Label status;
        private string appPath;

        public GitInstall(string appPath)
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            
            this.appPath = appPath;
            Title = "Installing required tools";
            ResizeMode = ResizeMode.NoResize;
            Height = 288;
            Width = 720;
            Icon = UserInterfaceHelpers.GetImageSource("sparkleshare-app", "ico");

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

            image.Source = UserInterfaceHelpers.GetImageSource("about");


            this.status = new Label()
            {
                Content = "Downloading required tools...",
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
                string toolsExe = Path.Combine(app_data_path, "org.sparkleshare.SparkleShare", "tmp", "tools.exe");

                client.DownloadFileAsync(new Uri("https://github.com/git-for-windows/git/releases/download/v2.21.0.windows.1/PortableGit-2.21.0-32-bit.7z.exe"), toolsExe);
                client.DownloadFileCompleted += Client_DownloadFileCompleted;
            }).Start();
        }

        private void Client_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                this.status.Content = "Installing downloaded tools.";
                this.progressBar.Value = 0;
            });

            string app_data_path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string toolsExe = Path.Combine(app_data_path, "org.sparkleshare.SparkleShare", "tmp", "tools.exe");
            string gitPath = Path.Combine(app_data_path, "org.sparkleshare.SparkleShare", "git");

            string sevenZip_path = Path.Combine(appPath, "7z.exe");
            var proc = Process.Start(new ProcessStartInfo()
            {
                FileName = sevenZip_path,
                Arguments = $"x \"{toolsExe}\" -o\"{gitPath}\" -y -bsp1 -bse1 -bso1",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true
            });
            while(!proc.HasExited)
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
