using System;
using System.Collections.Generic;
using System.Text;
using osu_common.Libraries.NetLib;
using System.Windows.Forms;
using ICSharpCode.SharpZipLib.Zip;
using System.IO;
using System.Diagnostics;
using puush.Libraries;

namespace puush
{
    internal static class UpdateManager
    {
        internal static bool IsChecking;

        internal static bool ProvideFeedback;

        internal static void CheckForUpdates(bool provideFeedback)
        {
            if (IsChecking) return;
            IsChecking = true;

            ProvideFeedback = false; //don't ever provide feedback for now.

            StringNetRequest nr = new StringNetRequest("http://puush.me/dl/puush-win.txt?check=" + DateTime.Now.Ticks);
            nr.onFinish += new StringNetRequest.RequestCompleteHandler(CheckForUpdates_onFinish);
            NetManager.AddRequest(nr);
        }

        static Timer timer;

        internal static void SetupTimedChecks()
        {
            if (timer == null)
            {
                //setup a timer which will check for updates every so often.
                timer = new Timer();
                timer.Tick += new EventHandler(timer_Tick);
                timer.Interval = 10000; //check after 10 seconds on the first check
            }

            timer.Start();
        }

        static void CheckForUpdates_onFinish(string _result, Exception e)
        {
            int version;

            if (e != null || _result.Length == 0 || !Int32.TryParse(_result, out version))
            {
                if (ProvideFeedback)
                    MessageBox.Show("An error occurred while checking for updates.");
                IsChecking = false;
                return;
            }

            puush.config.SetValue<string>("lastupdate", DateTime.Now.ToString());

            if (version > puush.INTERNAL_VERSION)
            {
                if (Settings.Instance != null)
                {
                    Settings.Instance.Invoke(delegate
                    {
                        Settings.Instance.groupBoxUpdate.Enabled = true;
                    });
                }

                if (ProvideFeedback)
                {
                    showUpdateAvailableDialog(null, null);
                }
                else
                {
                    MainForm.Instance.Invoke(GetUpdate);
                }
            }
            else
            {
                if (Settings.Instance != null)
                {
                    Settings.Instance.Invoke(delegate
                    {
                        Settings.Instance.ReloadConfig();

                        Settings.Instance.groupBoxUpdate.Enabled = true;

                        if (ProvideFeedback)
                        {
                            TopMostMessageBox.Show("You are running the latest version of puush.");
                        }
                    });
                }
            }

            IsChecking = false;
            ProvideFeedback = false;
        }

        private static void showUpdateAvailableDialog(object sender, EventArgs args)
        {
            MainForm.Instance.trayIcon.BalloonTipClicked -= showUpdateAvailableDialog;
            MainForm.Instance.trayIcon.MouseClick -= showUpdateAvailableDialog;

            MainForm.Instance.Invoke(
                        delegate
                        {
                            if (new UpdateAvailableDialog().ShowDialog(MainForm.Instance) == DialogResult.OK)
                                GetUpdate();
                        });
        }

        static void timer_Tick(object sender, EventArgs e)
        {
            timer.Interval = Math.Max(1000 * 60 * 30, Math.Min(1000 * 60 * 360, timer.Interval * 2)); //start at 30 minutes and keep doubling time until we hit 6 hours max.
            CheckForUpdates(false);
        }

        private static void GetUpdate()
        {
            MainForm.Instance.Invoke(
                        delegate
                        {
                            MainForm.Instance.trayIcon.ShowBalloonTip(5000, "Downloading update...", "puush will automatically restart when done!", ToolTipIcon.Info);
                            MainForm.Instance.trayIcon.Tag = null;
                        });

            //make sure we can write to the folder... (UAC)
            try
            {
                File.WriteAllText("test", "test");
            }
            catch
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.UseShellExecute = true;
                startInfo.WorkingDirectory = Environment.CurrentDirectory;
                startInfo.FileName = "puush.exe";
                startInfo.Arguments = "-setPermissions";
                startInfo.Verb = "runas";

                try
                {
                    Process pr = Process.Start(startInfo);
                    if (pr != null) pr.WaitForExit(8000);

                }
                catch (System.ComponentModel.Win32Exception)
                {
                    return;
                }
            }

            File.Delete("test");

            FileNetRequest nr = new FileNetRequest("puush-win.zip", "http://puush.me/dl/puush-win.zip");
            nr.onFinish += new FileNetRequest.RequestCompleteHandler(GetUpdate_onFinish);
            NetManager.AddRequest(nr);
        }

        static void GetUpdate_onFinish(string _fileLocation, Exception e)
        {
            if (e != null)
            {
                //retry, we failed...
                if (MessageBox.Show("Update failed, try again?", "puush", MessageBoxButtons.OKCancel) == DialogResult.OK)
                    GetUpdate();
                else
                {
                    if (Settings.Instance != null)
                    {
                        Settings.Instance.Invoke(delegate
                        {
                            Settings.Instance.groupBoxUpdate.Enabled = true;
                        });
                    }
                }
                return;
            }

            FastZip fz = new FastZip();
            fz.ExtractZip("puush-win.zip", "update", ".");
            File.Delete("puush-win.zip");

            File.Copy("puush.exe", "puush-old.exe");

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.UseShellExecute = true;
            startInfo.WorkingDirectory = Environment.CurrentDirectory;
            startInfo.FileName = "puush-old.exe";
            startInfo.Arguments = "-update";

            Process.Start(startInfo);
            MainForm.Instance.Close();
        }
    }
}
