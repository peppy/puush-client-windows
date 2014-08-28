using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using osu_common.Libraries.NetLib;
using System.Diagnostics;
using osu_common.Helpers;
using Microsoft.Win32;
using Gma.UserActivityMonitor;
using System.IO;

namespace puush
{
    enum UploadQuality
    {
        Best,
        High,
        Medium
    }

    enum FullscreenCaptureMode
    {
        AllScreens,
        Mouse,
        Primary
    }

    enum DoubleClickBehaviour
    {
        OpenSettings,
        ScreenSelect,
        UploadFile
    }

    public partial class Settings : pForm
    {
        public static void ShowPreferences()
        {
            if (MainForm.QuickStart != null && !puush.IsLoggedIn)
                return;

            MainForm.Instance.Invoke(delegate
            {
                if (Instance == null)
                    Instance = new Settings();

                Instance.Show();
                Instance.Focus();
                Instance.BringToFront();
                Instance.TopMost = true;
                Instance.TopMost = false;
            });
        }

        internal static Settings Instance;

        protected override void OnShown(EventArgs e)
        {
            UpdateAccountDetails();

            base.OnShown(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            Instance = null;

            EndKeyCapture();

            CheckSaveLocation();
            puush.config.SetValue<string>("saveimagepath", textBoxSaveLocation.Text);

            base.OnClosed(e);

            this.Dispose();
        }

        public Settings()
        {
            InitializeComponent();

            ReloadConfig();
        }

        /// <summary>
        /// Some toggles in reloading config cause events to be triggered.  We don't want to end up reloading more than we need to.
        /// </summary>
        bool isReloading;

        internal void ReloadConfig()
        {
            if (isReloading) return;
            isReloading = true;

            Invoke(delegate
            {
                bool isLoggedIn = puush.IsLoggedIn;

                displayUsername.Text = puush.config.GetValue<string>("username", null);
                displayApiKey.Text = puush.config.GetValue<string>("key", null);

                int accountTypeId = puush.config.GetValue<int>("type", 0);
                long usage = puush.config.GetValue<long>("usage", 0);

                displayAccountType.Text = puush.GetAccountTypeString(accountTypeId);
                displayDiskUsage.Text = accountTypeId == 0 ? string.Format("{0}/200mb", usage / 1048576) : string.Format("{0}mb", usage / 1048576);
                displayExpiryDate.Text = puush.config.GetValue<string>("expiry", "Unknown");

                groupAccountDetails.Visible = isLoggedIn;
                groupLogin.Visible = !isLoggedIn;

                checkBoxBrowser.Checked = puush.config.GetValue<bool>("openbrowser", false);
                checkBoxSound.Checked = puush.config.GetValue<bool>("notificationsound", true);
                checkBoxClipboard.Checked = puush.config.GetValue<bool>("copytoclipboard", true);
                checkBoxStartup.Checked = puush.config.GetValue<bool>("startup", true);

                checkBoxContextMenu.Checked = puush.config.GetValue<bool>("contextmenu", true);

                labelLastUpdate.Text = puush.config.GetValue<string>("lastupdate", "Never");

                checkBoxSaveImage.Checked = puush.config.GetValue<bool>("saveimages", false);
                textBoxSaveLocation.Text = puush.config.GetValue<string>("saveimagepath", Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
                panelSaveImage.Enabled = checkBoxSaveImage.Checked;

                folderBrowserDialog1.ShowNewFolderButton = true;
                folderBrowserDialog1.SelectedPath = textBoxSaveLocation.Text;

                buttonScreenSelectionBinding.Text = BindingManager.GetStringRepresentationFor(KeyBinding.ScreenSelection);
                buttonFullScreenBinding.Text = BindingManager.GetStringRepresentationFor(KeyBinding.FullscreenScreenshot);
                buttonWindowBinding.Text = BindingManager.GetStringRepresentationFor(KeyBinding.CurrentWindowScreenshot);
                buttonUploadBinding.Text = BindingManager.GetStringRepresentationFor(KeyBinding.UploadFile);
                buttonUploadClipboardBinding.Text = BindingManager.GetStringRepresentationFor(KeyBinding.UploadClipboard);
                buttonToggleBinding.Text = BindingManager.GetStringRepresentationFor(KeyBinding.Toggle);

                DoubleClickBehaviour doubleClickBehaviour = (DoubleClickBehaviour)puush.config.GetValue<int>("doubleclickbehaviour", 0);

                radioButton1.Checked = doubleClickBehaviour == DoubleClickBehaviour.OpenSettings;
                radioButton2.Checked = doubleClickBehaviour == DoubleClickBehaviour.ScreenSelect;
                radioButton3.Checked = doubleClickBehaviour == DoubleClickBehaviour.UploadFile;

                FullscreenCaptureMode fullscreenMode = (FullscreenCaptureMode)puush.config.GetValue<int>("FullscreenMode", 0);

                fullscreenCursor.Checked = fullscreenMode == FullscreenCaptureMode.Mouse;
                fullscreenAllScreens.Checked = fullscreenMode == FullscreenCaptureMode.AllScreens;
                fullscreenPrimary.Checked = fullscreenMode == FullscreenCaptureMode.Primary;

                UploadQuality quality = (UploadQuality)puush.config.GetValue<int>("uploadquality", 1);

                qualityBest.Checked = quality == UploadQuality.Best;
                qualityHigh.Checked = quality == UploadQuality.High;

                checkBoxTesting.Checked = puush.config.GetValue<bool>("Experimental", false);
                checkBoxSelectionRectangle.Checked = !puush.config.GetValue<bool>("selectionrectangle", true);

                isReloading = false;
            });
        }

        private void buttonLogout_Click(object sender, EventArgs e)
        {
            puush.Logout();

            textBoxPassword.Text = "";
        }

        private void buttonLogin_Click(object sender, EventArgs e)
        {
            FormNetRequest loginRequest = new FormNetRequest(puush.getApiUrl("auth"));
            loginRequest.AddField("e", textBoxUsername.Text);
            loginRequest.AddField("p", textBoxPassword.Text);
            loginRequest.AddField("z", "poop");
            loginRequest.onFinish += new FormNetRequest.RequestCompleteHandler(login_onFinish);
            NetManager.AddRequest(loginRequest);

            groupLogin.Enabled = false;
        }

        internal static void UpdateAccountDetails()
        {
            if (puush.IsLoggedIn)
            {
                FormNetRequest loginRequest = new FormNetRequest(puush.getApiUrl("auth"));
                loginRequest.AddField("e", puush.config.GetValue<string>("username", ""));
                loginRequest.AddField("k", puush.config.GetValue<string>("key", ""));
                loginRequest.AddField("z", "poop");
                loginRequest.onFinish += login_onFinish;
                NetManager.AddRequest(loginRequest);
            }
        }

        static void login_onFinish(string _result, Exception e)
        {
            int status = -1;

            string[] split = null;

            try
            {
                split = _result.Split(',');
                status = Int32.Parse(split[0]);
            }
            catch
            {
                if (Settings.Instance != null)
                    puush.ShowErrorBalloon("Connection with server went wrong.  Please check your connection and try again.", "Authentication Failure");
                return;
            }

            if (status < 0)
            {
                puush.ShowErrorBalloon("The username or password you entered is incorrect.", "Authentication Failure");
                puush.config.SetValue<string>("key", null);
            }
            else
            {
                int accountTypeId = status;

                string expiry = split[2];

                if (expiry.Length == 0)
                    expiry = "Never";

                long usage = Int64.Parse(split[3]);

                puush.config.SetValue<string>("key", split[1]);
                puush.config.SetValue<int>("type", accountTypeId);
                puush.config.SetValue<long>("usage", usage);
                puush.config.SetValue<string>("expiry", expiry);

                HistoryManager.Update();
            }

            if (Settings.Instance != null)
                Settings.Instance.UpdateAfterLogin();
        }

        private void UpdateAfterLogin()
        {
            Invoke((MethodInvoker)delegate
            {
                if (textBoxUsername.Text.Length > 0)
                    puush.config.SetValue<string>("username", textBoxUsername.Text);

                groupLogin.Enabled = true;

                ReloadConfig();
            });
        }

        private void textBox_TextChanged(object sender, EventArgs e)
        {
            UpdateLoginButton();
        }

        private void UpdateLoginButton()
        {
            buttonLogin.Enabled = textBoxUsername.TextLength > 0 && textBoxPassword.TextLength > 0;
        }

        private void textBoxUsername_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                buttonLogin.PerformClick();
                e.Handled = true;
            }
        }

        private void checkBoxSound_CheckedChanged(object sender, EventArgs e)
        {
            puush.config.SetValue<bool>("notificationsound", checkBoxSound.Checked);
        }

        private void checkBoxOpenInBrowser_CheckedChanged(object sender, EventArgs e)
        {
            puush.config.SetValue<bool>("openbrowser", checkBoxBrowser.Checked);
        }

        private void checkBoxClipboard_CheckedChanged(object sender, EventArgs e)
        {
            puush.config.SetValue<bool>("copytoclipboard", checkBoxClipboard.Checked);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            puush.ViewAccount();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            groupBoxUpdate.Enabled = false;
            UpdateManager.CheckForUpdates(true);
        }

        private void checkBoxStartup_CheckedChanged(object sender, EventArgs e)
        {
            puush.config.SetValue<bool>("startup", checkBoxStartup.Checked);

            puush.SetStartupBehaviour();
        }

        private void checkBoxSaveImage_CheckedChanged(object sender, EventArgs e)
        {
            puush.config.SetValue<bool>("saveimages", checkBoxSaveImage.Checked);

            ReloadConfig();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                puush.config.SetValue<string>("saveimagepath", folderBrowserDialog1.SelectedPath);
                ReloadConfig();
            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                puush.config.SetValue<int>("doubleclickbehaviour", 0);
                ReloadConfig();
            }
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked)
            {
                puush.config.SetValue<int>("doubleclickbehaviour", 1);
                ReloadConfig();
            }
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton3.Checked)
            {
                puush.config.SetValue<int>("doubleclickbehaviour", 2);
                ReloadConfig();
            }
        }

        private void Settings_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                Close();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://puush.me/register/");
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://puush.me/reset_password");
        }

        private void buttonScreenSelection_Click(object sender, EventArgs e)
        {
            StartKeyCapture(buttonScreenSelectionBinding, KeyBinding.ScreenSelection);
        }

        private void buttonFullScreenBinding_Click(object sender, EventArgs e)
        {
            StartKeyCapture(buttonFullScreenBinding, KeyBinding.FullscreenScreenshot);
        }

        private void buttonWindowBinding_Click(object sender, EventArgs e)
        {
            StartKeyCapture(buttonWindowBinding, KeyBinding.CurrentWindowScreenshot);
        }

        private void buttonUploadBinding_Click(object sender, EventArgs e)
        {
            StartKeyCapture(buttonUploadBinding, KeyBinding.UploadFile);
        }

        private void textBoxSaveLocation_Leave(object sender, EventArgs e)
        {
            CheckSaveLocation();
        }

        private void CheckSaveLocation()
        {
            if (!Directory.Exists(textBoxSaveLocation.Text))
                textBoxSaveLocation.Text = puush.config.GetValue<string>("saveimagepath", Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
        }

        private void buttonToggleBinding_Click(object sender, EventArgs e)
        {
            StartKeyCapture(buttonToggleBinding, KeyBinding.Toggle);
        }

        private void qualityBest_CheckedChanged(object sender, EventArgs e)
        {
            if (qualityBest.Checked)
                puush.config.SetValue<int>("uploadquality", (int)UploadQuality.Best);
        }

        private void qualityHigh_CheckedChanged(object sender, EventArgs e)
        {
            if (qualityHigh.Checked)
                puush.config.SetValue<int>("uploadquality", (int)UploadQuality.High);
        }

        private void buttonUploadClipboardBinding_Click(object sender, EventArgs e)
        {
            StartKeyCapture(buttonUploadClipboardBinding, KeyBinding.UploadClipboard);
        }

        private void checkBoxContextMenu_CheckedChanged(object sender, EventArgs e)
        {
            puush.config.SetValue<bool>("contextmenu", checkBoxContextMenu.Checked);
            if (checkBoxContextMenu.Checked)
                ContextMenuHandler.Install();
            else
                ContextMenuHandler.Remove();
        }

        private void fullscreenAllScreens_CheckedChanged(object sender, EventArgs e)
        {
            FullscreenCaptureMode mode = fullscreenAllScreens.Checked ? FullscreenCaptureMode.AllScreens :
                (fullscreenCursor.Checked ? FullscreenCaptureMode.Mouse : FullscreenCaptureMode.Primary);
            puush.config.SetValue<int>("FullscreenMode", (int)mode);
        }

        private void checkBoxTesting_CheckedChanged(object sender, EventArgs e)
        {
            if (puush.config.GetValue<bool>("Experimental", false) != checkBoxTesting.Checked)
            {
                puush.config.SetValue("Experimental", checkBoxTesting.Checked);
                Process.Start("puush.exe");
                Environment.Exit(-1);
            }
        }

        private void checkBoxSelectionRectangle_CheckedChanged(object sender, EventArgs e)
        {
            puush.config.SetValue<bool>("selectionrectangle", !checkBoxSelectionRectangle.Checked);
        }
    }
}
