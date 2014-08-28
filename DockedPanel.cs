using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using osu_common.Helpers;
using osu_common.Libraries.NetLib;
using System.Diagnostics;

namespace puush
{
    public partial class DockedPanel : pForm
    {
        Timer animateTimer = new Timer() { Interval = 5 };
        private FileUploadNetRequest currentUpload;
        private bool mouseFocus;
        private bool windowFocus;
        private MouseBounds boundsChecker;

        const int VSIZE = 68;
        private bool Hiding;

        int progressAim;

        protected override bool ShowWithoutActivation
        {
            get { return true; }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
            base.OnClosing(e);
        }

        public DockedPanel()
        {
            InitializeComponent();
            animateTimer.Tick += animate;

            comboBox1.Text = "Coming soon...";

            boundsChecker = new MouseBounds(this);
            boundsChecker.MouseBoundsChanged += delegate { mouseFocus = boundsChecker.MouseInBounds; UpdateHidingState(); };
        }

        private void UpdateHidingState()
        {
            bool hiding = !mouseFocus && !windowFocus && currentUpload == null;

            if (hiding != Hiding)
            {
                Hiding = hiding;

                Opacity = 1;
                progressAim = progressBar1.Maximum;
                progressBar1.ForeColor = Color.FromArgb(161, 244, 15);
            }
        }

        void animate(object sender, EventArgs e)
        {
            bool wFocus = ContainsFocus && !focusStealer.Focused;
            if (wFocus != windowFocus)
            {
                windowFocus = wFocus;
                UpdateHidingState();
            }
            
            if (Hiding)
            {
                progressAim--;
                Opacity = 1 - Math.Pow(1 - (double)progressAim / progressBar1.Maximum, 4);
                if (progressAim == 0)
                {
                    animateTimer.Enabled = false;
                    Hide();
                    Application.RemoveMessageFilter(boundsChecker);
                }
            }
            else
            {
                if (Opacity < 1)
                    Opacity = Math.Min(1, Opacity + 0.03);
            }

            if (progressBar1.Value != progressAim)
            {
                if (progressBar1.Value > progressAim)
                    progressBar1.Value = (int)Math.Max(0, progressBar1.Value * 0.8 + progressAim * 0.2);
                else
                    progressBar1.Value = (int)Math.Max(Math.Min(progressBar1.Maximum, progressBar1.Value + 1), progressBar1.Value * 0.8 + progressAim * 0.2);
            }
        }

        internal void StartUpload(FileUploadNetRequest req)
        {
            currentUpload = req;
            animateTimer.Enabled = true;

            UpdateHidingState();

            Invoke(delegate
            {
                textBoxFilename.Text = req.Filename;
                Size = new Size(Size.Width, VSIZE - panel1.Size.Height);
                panel1.Visible = false;

                progressAim = 0;
                progressBar1.ForeColor = Color.FromArgb(244, 150, 15);

                Opacity = 0;
                focusStealer.Select();
                Show();

                Application.AddMessageFilter(boundsChecker);
            });
        }

        internal void FinishUpload(string link, FileUploadNetRequest req)
        {
            Invoke(delegate
            {
                if (link != null)
                {
                    linkLabel1.Text = link;
                    UpdateLinkFilename();
                }
                Size = new Size(Size.Width, VSIZE);
                panel1.Visible = true;

                currentUpload = null;
                UpdateHidingState();
            });
        }

        private void UpdateLinkFilename()
        {
            linkLabel2.Text = linkLabel1.Text + "/" + textBoxFilename.Text;
        }

        internal void SetProgress(float percentage)
        {
            Invoke(() => { progressAim = (int)(percentage * progressBar1.Maximum / 100f); });
        }

        private void linkLabel_Clicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            LinkLabel l = sender as LinkLabel;

            try
            {
                Process.Start(l.Text.Replace(" ", "%20"));
            }
            catch { }
        }

        private void textBoxFilename_TextChanged(object sender, EventArgs e)
        {
            UpdateLinkFilename();
        }

        private void textBoxFilename_Click(object sender, EventArgs e)
        {
            textBoxFilename.Select(0, textBoxFilename.Text.Length - 4);
        }
    }

    public class MouseBounds : IMessageFilter
    {
        private const int WM_NCMOUSEMOVE = 0x00A0;
        private const int WM_MOUSEMOVE = 0x200;
        private const int WM_NCMOUSELEAVE = 0x02A2;
        private const int WM_MOUSELEAVE = 0x02A3;

        private bool _mouseInBounds = false;

        public event EventHandler MouseBoundsChanged;

        private Form form;

        public MouseBounds(Form form)
        {
            this.form = form;
        }

        public bool PreFilterMessage(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_MOUSEMOVE:
                case WM_NCMOUSEMOVE:
                    CheckMouseBounds(true);
                    break;

                case WM_NCMOUSELEAVE:
                case WM_MOUSELEAVE:
                    CheckMouseBounds(false);
                    break;
            }

            return false; // dont actually filter the message
        }

        /// <summary>
        /// Checks if the current cursor position is contained within the bounds of the
        /// form and sets MouseInBounds which in turn fires event MouseBoundsChanged
        /// </summary>
        /// <param name="mouseMove"></param>
        private void CheckMouseBounds(bool mouseMove)
        {
            // Already know the mouse is in the bounds, so we dont
            // care that the mouse just moved (saves unnecessary checks
            // on the form bounds and OnMouseBoundsChanged calls)
            if ((MouseInBounds) && (mouseMove))
                return;

            // if the cursor is in the bounds of the current form 
            // set the bounds to true, else set the bounds to false
            SetMouseBounds(form.Bounds.Contains(Cursor.Position));
        }

        public void SetMouseBounds(bool mouseInBounds)
        {
            // prevent setting the bounds status to the same as it was
            // already set (shouldn't happen anyway, so this is just a
            // sanity check)
            if (mouseInBounds != _mouseInBounds)
            {
                _mouseInBounds = mouseInBounds;
                OnMouseBoundsChanged(EventArgs.Empty);
            }
        }

        private void OnMouseBoundsChanged(EventArgs e)
        {
            if (MouseBoundsChanged != null)
                MouseBoundsChanged(this, e);
        }

        public bool MouseInBounds
        {
            get
            {
                return _mouseInBounds;
            }
        }
    }
}
