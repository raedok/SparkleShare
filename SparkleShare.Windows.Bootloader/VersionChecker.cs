using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Security.Principal;
using System.Windows.Forms;

namespace SparkleShare.Windows
{
    public class VersionChecker
    {
        static bool IsElevated
        {
            get
            {
                return WindowsIdentity.GetCurrent().Owner.IsWellKnown(WellKnownSidType.BuiltinAdministratorsSid);
            }
        }

        public static void CheckAndUpdate()
        {
            string executable_path = Path.GetDirectoryName(Application.ExecutablePath);
            var localVersion = "";
            try { localVersion = File.ReadAllText(Path.Combine(executable_path, "version.txt")); } catch { }
            if (string.IsNullOrEmpty(localVersion)) localVersion = "0.0.0";
            WebClient client = new WebClient();
            var webVersion = "";
            try { webVersion = client.DownloadString("http://share.harvestiasi.ro/sparkleshare/windows/raw/master/version.txt"); } catch { }
            if (string.IsNullOrEmpty(localVersion)) return;

            var local = Version.Parse(localVersion);
            var web = Version.Parse(webVersion);

            if (web > local)
            {
                if (!IsElevated)
                {
                    Process.Start(new ProcessStartInfo() {
                        FileName = Application.ExecutablePath,
                        Verb = "runas"
                    });
                    Environment.Exit(0);
                }
                else
                {
                    var window = new UpdateWindow(executable_path);
                    window.ShowDialog();
                    
                    Process.Start(new ProcessStartInfo()
                    {
                        FileName = Application.ExecutablePath
                    });
                    Environment.Exit(0);
                }
            }
        }
    }
}
