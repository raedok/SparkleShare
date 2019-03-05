//   SparkleShare, a collaboration and sharing tool.
//   Copyright (C) 2010  Hylke Bons <hi@planetpeanut.uk>
//
//   This program is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General Public License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
//
//   This program is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//   GNU General Public License for more details.
//
//   You should have received a copy of the GNU General Public License
//   along with this program. If not, see <http://www.gnu.org/licenses/>.


using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

using System.Windows;
using Forms = System.Windows.Forms;

using Sparkles;
using Sparkles.Git;

namespace SparkleShare
{

    public class Controller : BaseController
    {

        public Controller(Configuration config)
            : base(config)
        {
        }


        public override string PresetsPath
        {
            get
            {
                var assembly = Assembly.GetExecutingAssembly();
                var directory = Path.GetDirectoryName(assembly.Location);
                return Path.Combine(directory, "Presets");
            }
        }


        public override void Initialize()
        {
            // Add msysgit to path, as we cannot asume it is added to the path
            // Asume it is installed in @"<exec dir>\msysgit\bin"
            string executable_path = Path.GetDirectoryName(Forms.Application.ExecutablePath);
            string app_data_path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            if (InstallationInfo.OperatingSystem != OS.Windows && InstallationInfo.OperatingSystem != OS.macOS)
                app_data_path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), ".config");
            string msysgit_path = Path.Combine(app_data_path, "org.sparkleshare.SparkleShare", "git");

            if (!Directory.Exists(msysgit_path))
            {
                var wnd = new GitInstall(executable_path);
                wnd.ShowDialog();
            }

            Environment.SetEnvironmentVariable("HOME", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));

            SSHCommand.SSHPath = Path.Combine(msysgit_path, "usr", "bin");
            SSHFetcher.SSHKeyScan = Path.Combine(msysgit_path, "usr", "bin", "ssh-keyscan.exe");
            GitCommand.GitPath = Path.Combine(msysgit_path, "bin", "git.exe");
            var gitLfs = Path.Combine(msysgit_path, "mingw32", "bin", "git-lfs.exe");
            string gitLfsDest = Path.Combine(app_data_path, "org.sparkleshare.SparkleShare", "bin", "git-lfs.exe");

            if (!File.Exists(gitLfsDest))
                File.Copy(gitLfs, gitLfsDest);

            base.Initialize();
        }


        public override string EventLogHTML
        {
            get
            {
                string html = UserInterfaceHelpers.GetHTML("event-log.html");
                return html.Replace("<!-- $jquery -->", UserInterfaceHelpers.GetHTML("jquery.js"));
            }
        }


        public override string DayEntryHTML
        {
            get
            {
                return UserInterfaceHelpers.GetHTML("day-entry.html");
            }
        }


        public override string EventEntryHTML
        {
            get
            {
                return UserInterfaceHelpers.GetHTML("event-entry.html");
            }
        }


        public override void SetFolderIcon()
        {
            string app_path = Path.GetDirectoryName(Forms.Application.ExecutablePath);
            string icon_file_path = Path.Combine(app_path, "Images", "sparkleshare-folder.ico");

            if (!File.Exists(icon_file_path))
            {
                string ini_file_path = Path.Combine(FoldersPath, "desktop.ini");
                string n = Environment.NewLine;

                string ini_file = "[.ShellClassInfo]" + n +
                    "IconFile=" + icon_file_path + n +
                    "IconIndex=0" + n +
                    "InfoTip=SparkleShare";

                try
                {
                    File.Create(ini_file_path).Close();
                    File.WriteAllText(ini_file_path, ini_file);

                    File.SetAttributes(ini_file_path,
                        File.GetAttributes(ini_file_path) | FileAttributes.Hidden | FileAttributes.System);

                }
                catch (IOException e)
                {
                    Logger.LogInfo("Config", "Failed setting icon for '" + FoldersPath + "': " + e.Message);
                }
            }
        }


        public override void CreateStartupItem()
        {
            string startup_folder_path = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            string shortcut_path = Path.Combine(startup_folder_path, "SparkleShare.lnk");

            if (File.Exists(shortcut_path))
                File.Delete(shortcut_path);

            string shortcut_target = Forms.Application.ExecutablePath;

            Shortcut shortcut = new Shortcut();
            shortcut.Create(shortcut_path, shortcut_target);
        }


        public override void InstallProtocolHandler()
        {
            // We ship a separate .exe for this
        }


        public void AddToBookmarks()
        {
            string user_profile_path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string shortcut_path = Path.Combine(user_profile_path, "Links", "SparkleShare.lnk");

            if (File.Exists(shortcut_path))
                File.Delete(shortcut_path);

            Shortcut shortcut = new Shortcut();
            shortcut.Create(FoldersPath, shortcut_path);
        }


        public override void CreateSparkleShareFolder()
        {
            if (!Directory.Exists(FoldersPath))
            {
                Directory.CreateDirectory(FoldersPath);

                File.SetAttributes(FoldersPath, File.GetAttributes(FoldersPath) | FileAttributes.System);
                Logger.LogInfo("Config", "Created '" + FoldersPath + "'");
            }
        }


        public override void OpenFile(string path)
        {
            Process.Start(path);
        }


        public override void OpenFolder(string path)
        {
            Process.Start(path);
        }


        public override void OpenWebsite(string url)
        {
            Process.Start(new ProcessStartInfo(url));
        }


        public override void CopyToClipboard(string text)
        {
            try
            {
                Clipboard.SetData(DataFormats.Text, text);

            }
            catch (COMException e)
            {
                Logger.LogInfo("Controller", "Copy to clipboard failed", e);
            }
        }


        public override void PlatformQuit()
        {
            Environment.Exit(0);
        }
    }
}
