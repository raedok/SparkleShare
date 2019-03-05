using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace SparkleShare.Windows.Bootloader
{
    static class Program
    {
        static Mutex program_mutex = new Mutex(false, "SparkleShare");

        [STAThread]
        static void Main(string[] args)
        {
            // Only allow one instance of SparkleShare (on Windows)
            if (!program_mutex.WaitOne(2000, exitContext: false))
            {
                Console.WriteLine("SparkleShare is already running.");
                Environment.Exit(-1);
            }
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            VersionChecker.CheckAndUpdate();

            program_mutex.ReleaseMutex();
            AppDomain.CurrentDomain.UnhandledException -= OnUnhandledException;

            LoadSparkleShare(args);
        }

        private static void LoadSparkleShare(string[] args)
        {
            string executable_path = Path.GetDirectoryName(Application.ExecutablePath);
            var exe = Path.Combine(executable_path, "SparkleShare.Windows.exe");
            Assembly assembly = Assembly.LoadFile(exe);
            assembly.EntryPoint.Invoke(null, new object[] { args });
        }

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine(e.ExceptionObject);
        }
    }
}
