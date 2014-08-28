using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;

namespace osu_common.Helpers
{
    public class pForm : Form
    {
        protected override void OnLoad(EventArgs e)
        {
            FixFonts();

            base.OnLoad(e);
        }

        private void FixFonts()
        {
            Font newFont;

            AutoScaleMode = AutoScaleMode.None;



            foreach (Control c in Controls)
                c.Font = Font;
        }

        public override Font Font
        {
            get
            {
                if (Environment.OSVersion.Version.Major < 6)
                {
                    //Windows 2000 (5.0), Windows XP (5.1), Windows Server 2003 and XP Pro x64 Edtion v2003 (5.2)
                    return SystemFonts.DialogFont; //Tahoma hopefully
                }
                else
                {
                    //Vista and above
                    return SystemFonts.MessageBoxFont; //should be SegoiUI
                }
            }
            set
            {
                base.Font = value;
            }
        }

        public void Invoke(MethodInvoker moo)
        {
            Invoke(moo, false);
        }
        
        public void Invoke(MethodInvoker moo, bool force)
        {
            try
            {
                if (InvokeRequired)
                {
                    if (!base.IsHandleCreated)
                    {
                        Thread.Sleep(3000);
                        if (!base.IsHandleCreated)
                            return;
                    }

                    if (Disposing || IsDisposed)
                        return;

                    int tryCount = 5;
                    while (tryCount-- > 0)
                    {
                        if (IsHandleCreated)
                        {
                            base.Invoke(moo);
                            break;
                        }
                        else
                            Thread.Sleep(600);
                    }
                }
                else
                    moo();
            }
            catch
            { }
        }
    }
}