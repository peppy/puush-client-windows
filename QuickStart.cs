using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using osu_common.Libraries.NetLib;
using osu_common.Helpers;

namespace puush
{
    public partial class QuickStart : pForm
    {
        public QuickStart()
        {
            InitializeComponent();

            groupLogin.Visible = !puush.IsLoggedIn;
            buttonClose.Enabled = puush.IsLoggedIn;

            ShowInTaskbar = true;
            TopMost = false;
        }

        protected override void OnClosed(EventArgs e)
        {
            MainForm.QuickStart = null;
            base.OnClosed(e);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            
            puush.config.SetValue<bool>("quickstartshown", true);
            puush.config.SetValue<bool>("startup", checkBox1.Checked);
            puush.SetStartupBehaviour();
            
            Close();
        }

        private void textBox_TextChanged(object sender, EventArgs e)
        {
            UpdateLoginButton();
        }

        private void UpdateLoginButton()
        {
            buttonLogin.Enabled = textBoxUsername.TextLength > 0 && textBoxPassword.TextLength > 0;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Process.Start("http://puush.me/register/");
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://puush.me/reset_password");
        }

        private void buttonLogin_Click(object sender, EventArgs e)
        {
            FormNetRequest loginRequest = new FormNetRequest(puush.getApiUrl("auth"));
            loginRequest.AddField("e", textBoxUsername.Text);
            loginRequest.AddField("p", textBoxPassword.Text);
            loginRequest.AddField("z", "poop");
            loginRequest.onFinish += login_onFinish;
            NetManager.AddRequest(loginRequest);

            groupLogin.Enabled = false;
        }

        void login_onFinish(string _result, Exception e)
        {
            Invoke((MethodInvoker)delegate
            {
                groupLogin.Enabled = true;

                int status = -1;

                string[] split = _result.Split(',');

                try
                {
                    status = Int32.Parse(split[0]);
                }
                catch
                {
                    MessageBox.Show(this, "Connection with server went wrong.  Please check your connection and try again.", "Authentication Failure", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (status < 0)
                {
                    MessageBox.Show(this, "The username or password you entered is incorrect.", "Authentication Failure", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    puush.config.SetValue<string>("username", textBoxUsername.Text);
                    puush.config.SetValue<string>("key", split[1]);

                    groupLogin.Visible = false;
                    buttonClose.Enabled = true;

                    buttonClose.Select();
                }
            });
        }

        private void textBoxUsername_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                buttonLogin.PerformClick();
                e.Handled = true;
            }
        }
    }
}
