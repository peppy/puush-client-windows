//#define USE_DEV_SERVER
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using osu_common.Helpers;
using System.IO;
using System.Diagnostics;
using System.Security.Principal;
using System.Security.AccessControl;
using System.Threading;
using Microsoft.Win32;
using osu_common.Libraries.NetLib;
using Gma.UserActivityMonitor;
using System.Runtime.InteropServices;
using System.Text;

namespace puush
{
    internal static class puush
    {
        internal static pConfigManager config;

        public const int INTERNAL_VERSION = 93;
        internal static bool RecoveringFromError;

        /// <summary>The GetForegroundWindow function returns a handle to the foreground window.</summary>
        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        internal static IntPtr LastForegroundWindow;

        public static string getPuushUrl(string action)
        {
#if USE_DEV_SERVER && DEBUG
            return "http://dev.puush.me/" + action;
#else
            return "http://puush.me/" + action;
#endif
        }

        public static string getApiUrl(string action)
        {
            return getPuushUrl("api/" + action);
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThreadAttribute]
        static void Main()
        {
#if !DEBUG
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
#endif

            Environment.CurrentDirectory = Path.GetDirectoryName(Application.ExecutablePath);

            string[] args = Environment.GetCommandLineArgs();

            //make sure we have an appdata folder
            string folderName = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\puush";

            config = new pConfigManager(folderName + "\\puush.ini");
            config.WriteOnChange = true;

            if (args.Length > 1)
            {
                switch (args[1])
                {
                    case "-setPermissions":
                        IsRunningElevated = true;
                        SetPermissions();
                        return;
                    case "-removeContext":
                        IsRunningElevated = true;
                        SetPermissions();
                        ContextMenuHandler.Remove();
                        return;
                    case "-update":
                        CompleteUpdate();
                        return;
                    case "-ohnoes":
                        RecoveringFromError = true;

                        MainForm.threadMeSome(delegate
                        {
                            Thread.Sleep(30000);
                            RecoveringFromError = false;
                        });
                        break;
                    case "-upload":
                        string filename = string.Empty;

                        for (int i = 2; i < args.Length; i++)
                            filename += " " + args[i];

                        filename = filename.Trim();

                        try
                        {
                            IPC.LoadFile(filename);
                            return;
                        }
                        catch (Exception e)
                        {
                            //maybe puush isn't started?
                            new Thread(() =>
                            {
                                Thread.Sleep(2000);
                                FileUpload.Upload(filename);

                            }).Start();
                        }
                        break;
                }
            }

            EnsureFirstInstance();

            IPC.AcceptConnections();

            if (!Directory.Exists(folderName))
                Directory.CreateDirectory(folderName);

            if (puush.config.GetValue<bool>("contextmenu", true))
                ContextMenuHandler.Install();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            SetStartupBehaviour();

            UpdateManager.SetupTimedChecks();

            TimedStuff();

            Application.Run(new MainForm());

            try
            {
                HookManager.UnsubscribeAllHooks();
                IPC.Unregister();
            }
            catch { }
        }

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, out uint ProcessId);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        static string GetProcessPathFromWindowHandle(IntPtr hWnd)
        {
            uint processId = 0;
            GetWindowThreadProcessId(hWnd, out processId);
            if (processId == 0) return string.Empty;

            Process p = Process.GetProcessById((int)processId);

            StringBuilder className = new StringBuilder(100);
            GetClassName(hWnd, className, className.Capacity);

            return p.ProcessName + ":" + className;
        }

        public static System.Windows.Forms.Timer timer;
        internal static bool IsRunningElevated;
        private static void TimedStuff()
        {
            if (timer == null)
            {
                //setup a timer which will check for updates every so often.
                timer = new System.Windows.Forms.Timer();
                timer.Tick += new EventHandler(delegate
                {
                    try
                    {
                        IntPtr currentForegroundWindow = GetForegroundWindow();

                        BindingManager.Bind();

                        string[] split = GetProcessPathFromWindowHandle(currentForegroundWindow).Split(':');

                        if (split == null || split.Length < 2)
                            return;

                        string filename = split[0].ToLower();

                        switch (filename)
                        {
                            case "puush":
                                return;
                            case "explorer":
                                switch (split[1])
                                {
                                    case "Shell_TrayWnd":
                                    case "DV2ControlHost":
                                        return;
                                    default:
                                        break;
                                }
                                break;
                            default:
                                LastForegroundWindow = currentForegroundWindow;
                                break;
                        }
                    }
                    catch { }
                });
                timer.Interval = 1000;
            }
            timer.Start();
        }

        private static void EnsureFirstInstance()
        {
            string name = Path.GetFileNameWithoutExtension(Application.ExecutablePath);

            Environment.CurrentDirectory = Path.GetDirectoryName(Application.ExecutablePath);

            bool hasFileArguments = false;

            bool firstInstance = Process.GetProcessesByName(name).Length < 2;

            if (!firstInstance)
            {
                Thread.Sleep(2500);
                firstInstance = Process.GetProcessesByName(name).Length < 2;
                if (!firstInstance && !hasFileArguments)
                {
                    {
                        KillStuckProcesses(name);
                        int count = 0;
                        while (count++ < 20 && !firstInstance)
                        {
                            Thread.Sleep(100);
                            firstInstance = Process.GetProcessesByName(name).Length < 2;
                        }
                        if (!firstInstance)
                        {
                            MessageBox.Show("puush is already running and couldn't kill the existing copy.  Please manually end the process using Task Manager.",
                                            name,
                                            MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                            return;
                        }
                    }
                }
            }
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                FormNetRequest req = new FormNetRequest(puush.getApiUrl("oshi"));
                req.AddField("e", puush.config.GetValue<string>("username", ""));
                req.AddField("v", INTERNAL_VERSION.ToString());
                req.AddField("l", e.ExceptionObject.ToString());

                req.BlockingPerform();
            }
            catch { }

            //This ensures we can get rid oF the tray icon if possible.
            try
            {
                if (MainForm.Instance != null)
                    MainForm.Instance.trayIcon.Dispose();
            }
            catch { }

            if (RecoveringFromError)
            {
                try
                {
                    MessageBox.Show("Unfortunately, something is going very wrong. While your error has been reported, it seems to be happening consistently and we can't make puush start properly... If you need assistance, please contact puush@puush.me", "puush is dying...", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch { }
            }
            else
                Process.Start("puush.exe", "-ohnoes");

            Environment.Exit(-1);
        }

        private static void CompleteUpdate()
        {
            const int TRIES = 20;

            int tryCount = TRIES;

            if (!WaitThenKill("puush"))
                Environment.Exit(-1);

            while (File.Exists("puush.exe"))
            {
                try
                {
                    tryCount--;

                    if (tryCount == 0)
                    {
                        if (MessageBox.Show("Could not automatically delete old puush.exe.  Please delete manually to complete the update.", "puush", MessageBoxButtons.OKCancel, MessageBoxIcon.Error) == DialogResult.OK)
                            tryCount = TRIES;
                        else
                            Environment.Exit(-1);
                    }

                    Thread.Sleep(200);
                    File.Delete("puush.exe");
                }
                catch { }
            }


            GeneralHelper.RecursiveMove("update", ".\\");

            Process.Start("puush.exe");
        }

        private static bool WaitThenKill(string name)
        {
            return WaitThenKill(name, 1);
        }

        private static bool WaitThenKill(string name, int countz)
        {
            bool osuStillOpen = Process.GetProcessesByName(name).Length < countz;
            if (!osuStillOpen)
            {
                Thread.Sleep(1500);
                osuStillOpen = Process.GetProcessesByName(name).Length < countz;
                if (!osuStillOpen)
                {
                    {
                        int count = 0;
                        while (count++ < 10 && !osuStillOpen)
                        {
                            KillStuckProcesses(name);
                            Thread.Sleep(100);
                            osuStillOpen = Process.GetProcessesByName(name).Length < countz;
                        }
                        if (!osuStillOpen)
                        {
                            MessageBox.Show("Could not automatically delete old puush.exe.  Please delete manually to complete the update.", "puush", MessageBoxButtons.OKCancel, MessageBoxIcon.Error);
                            return false;
                        }

                    }
                }
            }
            return true;
        }

        private static void KillStuckProcesses(string name)
        {
            System.Diagnostics.Process myproc = new System.Diagnostics.Process();
            try
            {
                int handle = Process.GetCurrentProcess().Id;
                foreach (Process proc in Process.GetProcessesByName(name))
                    if (proc.Id != handle && !proc.CloseMainWindow())
                        proc.Kill();
            }
            catch (Exception e)
            {
            }
        }

        public static bool IsLoggedIn { get { return !string.IsNullOrEmpty(config.GetValue<string>("username", null)) && !string.IsNullOrEmpty(config.GetValue<string>("key", null)); } }

        public static bool EnsureLogin()
        {
            if (!IsLoggedIn)
            {
                Settings.ShowPreferences();
                if (Settings.Instance != null)
                {
                    Settings.Instance.Invoke(delegate
                    {
                        Settings.Instance.tabControl1.SelectedIndex = 2;
                    });
                }
                return false;
            }

            return true;
        }

        internal static void ViewAccount()
        {
            if (EnsureLogin())
                Process.Start(puush.getPuushUrl("login/go/?k=" + puush.config.GetValue<string>("key", "")));
        }

        internal static void SetPermissions()
        {
            try
            {
                SecurityIdentifier sid = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
                NTAccount acct = sid.Translate(typeof(NTAccount)) as NTAccount;
                string strEveryoneAccount = acct.ToString();

                GeneralHelper.AddDirectorySecurity(".\\", strEveryoneAccount, FileSystemRights.FullControl,
                                                   InheritanceFlags.None, PropagationFlags.NoPropagateInherit,
                                                   AccessControlType.Allow);
                GeneralHelper.AddDirectorySecurity(".\\", strEveryoneAccount, FileSystemRights.FullControl,
                                                   InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit,
                                                   PropagationFlags.InheritOnly, AccessControlType.Allow);
                GeneralHelper.RemoveReadOnlyRecursive(".\\");

                string folderName = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\puush";
                config = new pConfigManager(folderName + "\\puush.ini");
                config.WriteOnChange = true;
            }
            catch { }

            try
            {
                if (puush.config.GetValue<bool>("contextmenu", true))
                    ContextMenuHandler.Install();
            }
            catch { }
        }

        internal static void SetStartupBehaviour()
        {
            try
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run",
                                                                  true);

                if (key == null) return;

                if (puush.config.GetValue<bool>("startup", true))
                    key.SetValue("puush", Application.ExecutablePath.ToString());
                else
                    key.DeleteValue("puush", false);
            }
            catch { }
        }

        internal static void Logout()
        {
            puush.config.SetValue<string>("username", null);
            puush.config.SetValue<string>("key", null);

            if (Settings.Instance != null)
                Settings.Instance.ReloadConfig();
        }

        public static bool UploadCancellable
        {
            set
            {
                MainForm.Instance.Invoke(delegate
                {
                    MainForm.Instance.toolStripMenuItemCancelUpload.Visible = value;
                });
            }
        }

        internal static void HandleInvalidAuthentication()
        {
            puush.Logout();
            puush.EnsureLogin();
            MessageBox.Show(Settings.Instance, "Authentication failure.  Your API key may no longer be valid.", "puush", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        internal static void ShowErrorBalloon(string errorDescription)
        {
            ShowErrorBalloon(errorDescription, "An error occurred:");
        }

        internal static void ShowErrorBalloon(string description, string title)
        {
            if (description == null) return;

            MainForm.Instance.Invoke((MethodInvoker)delegate
            {
                MainForm.Instance.trayIcon.ShowBalloonTip(6000, title, description, ToolTipIcon.Error);
                MainForm.Instance.trayIcon.Tag = null;
            });
        }

        internal static string GetAccountTypeString(int accountTypeId)
        {
            string type;

            switch (accountTypeId)
            {
                default:
                case 0:
                    type = @"Free Account";
                    break;
                case 1:
                    type = @"Pro Account";
                    break;
                case 2:
                    type = @"Pro Tester";
                    break;
                case 9:
                    type = @"Haxor!";
                    break;
            }

            return type;
        }
    }
}
