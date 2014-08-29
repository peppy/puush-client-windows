using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Gma.UserActivityMonitor;
using System.Diagnostics;
using System.Runtime.InteropServices;
using osu_common.Helpers;
using System.IO;
using puush.Properties;
using System.Threading;
using puush.Libraries;
using System.Collections.Specialized;

namespace puush
{
    public partial class MainForm : pForm
    {
        public static MainForm Instance;

        public static QuickStart QuickStart;

        public static DockedPanel panel;

        public MainForm()
        {
            InitializeComponent();
            Instance = this;

            Visible = false;
            Hide();

            //TopMost = true;
            Icon = Resources.iconbundle;

            trayIcon.BalloonTipClicked += new EventHandler(trayIcon_BalloonTipClicked);

            toolStripMenuItemUploadDisabled.Checked = puush.config.GetValue<bool>("disableupload", false);

            toolStripMenuItemVersion.Text = string.Format("puush r{0}", puush.INTERNAL_VERSION);

            BindingManager.Bind();

            if (!puush.IsLoggedIn)
            {
                QuickStart = new QuickStart();
                QuickStart.Show(this);
            }
            else
            {
                Settings.UpdateAccountDetails();
            }

            if (puush.config.GetValue<bool>("Experimental", false))
                panel = new DockedPanel();

            try
            {
                if (File.Exists("puush-old.exe"))
                {
                    MainForm.Instance.trayIcon.ShowBalloonTip(5000, "puush was updated!", "You are now running " + toolStripMenuItemVersion.Text + "!", ToolTipIcon.Info);
                    MainForm.Instance.trayIcon.Tag = null;
                }

                File.Delete("puush-old.exe");
            }
            catch { }

            HistoryManager.Update();

            if (puush.RecoveringFromError)
                puush.ShowErrorBalloon("This has been reported automatically.\nSorry for any inconvenience", "puush has just recovered from an error.");

        }

        internal static List<Keys> PressedKeys = new List<Keys>();

        void HookManager_KeyUp(object sender, KeyEventArgs e)
        {
            PressedKeys.Remove(e.KeyData);
            handledKeyDownCount = PressedKeys.Count;
        }

        static int handledKeyDownCount;
        void HookManager_KeyDown(object sender, KeyEventArgs e)
        {
            if (Settings.IsKeyCapturing || puush.config.GetValue<bool>("disableupload", false)) return;

            e.Handled = true;

            if (captureMode && e.KeyCode == Keys.Escape)
            {
                EndSelection();
                return;
            }

            if (PressedKeys.Contains(e.KeyCode))
            {
                e.Handled = false;
                return;
            }

            PressedKeys.Add(e.KeyCode);

            e.Handled = false;
        }

        internal static void threadMeSome(MethodInvoker method)
        {
            if (method == null) return;

            try
            {
                Thread t = new Thread(() => method());
                t.IsBackground = true;
                t.Start();
            }
            catch
            {
                //do something here too..  
            }

        }

        internal static void invokeMeSome(MethodInvoker method)
        {
            if (method == null) return;
            try
            {
                Thread t = new Thread((ParameterizedThreadStart)delegate
                {
                    try
                    {
                        Instance.Invoke(delegate
                        {
                            method();
                        });
                    }
                    catch
                    {
                        //do something here?!
                    }
                });
                t.IsBackground = true;
                t.Start();
            }
            catch
            {

            }

        }

        void trayIcon_BalloonTipClicked(object sender, EventArgs e)
        {
            try
            {
                string tag = trayIcon.Tag as string;

                if (tag == null) return;

                if (tag.StartsWith("http") && !puush.config.GetValue<bool>("openbrowser", false))
                    Process.Start("http://" + tag.Replace("http://", ""));
            }
            catch { }
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        void toolStripMenuItem2_Click(object sender, System.EventArgs e)
        {
            Settings.ShowPreferences();
        }

        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            puush.ViewAccount();
        }

        void toolStripMenuItemSelection_Click(object sender, System.EventArgs e)
        {
            StartSelection();
        }

        void toolStripMenuItemDesktop_Click(object sender, System.EventArgs e)
        {
            ScreenshotCapture.Run();
        }

        void toolStripMenuItemWindow_Click(object sender, System.EventArgs e)
        {
            ScreenshotCapture.RunWindow();
        }

        void toolStripMenuItemUploadFile_Click(object sender, System.EventArgs e)
        {
            UploadFile();
        }

        void toolStripMenuItem3_Click(object sender, System.EventArgs e)
        {
            Toggle();
        }

        internal static void Toggle()
        {
            invokeMeSome(delegate
                             {
                                 Instance.toolStripMenuItemUploadDisabled.Checked =
                                     !Instance.toolStripMenuItemUploadDisabled.Checked;
                                 puush.config.SetValue<bool>("disableupload", Instance.toolStripMenuItemUploadDisabled.Checked);

                                 if (Instance.toolStripMenuItemUploadDisabled.Checked)
                                 {
                                     MainForm.Instance.trayIcon.ShowBalloonTip(2000, "puush was disabled!", "Shortcut keys will no longer be accepted.", ToolTipIcon.Info);
                                     MainForm.Instance.trayIcon.Tag = null;
                                 }
                                 else
                                 {
                                     MainForm.Instance.trayIcon.ShowBalloonTip(2000, "puush was enabled!", "Shortcut keys will now be accepted.", ToolTipIcon.Info);
                                     MainForm.Instance.trayIcon.Tag = null;
                                 }


                             });
        }

        private void trayIcon_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (FileUpload.CurrentUpload != null)
                    if (TopMostMessageBox.Show("Would you like to cancel the current puush?", "puush", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == System.Windows.Forms.DialogResult.Yes)
                        FileUpload.CancelCurrent();
            }
        }

        private void trayIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                switch ((DoubleClickBehaviour)puush.config.GetValue<int>("doubleclickbehaviour", 0))
                {
                    case DoubleClickBehaviour.OpenSettings:
                        Settings.ShowPreferences();
                        break;
                    case DoubleClickBehaviour.ScreenSelect:
                        StartSelection();
                        break;
                    case DoubleClickBehaviour.UploadFile:
                        UploadFile();
                        break;
                }
            }
        }

        /// <summary>The GetForegroundWindow function returns a handle to the foreground window.</summary>
        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [StructLayout(LayoutKind.Sequential, Pack = 2)]
        private struct BITMAPFILEHEADER
        {
            public static readonly short BM = 0x4d42; // BM

            public short bfType;
            public int bfSize;
            public short bfReserved1;
            public short bfReserved2;
            public int bfOffBits;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct BITMAPINFOHEADER
        {
            public int biSize;
            public int biWidth;
            public int biHeight;
            public short biPlanes;
            public short biBitCount;
            public int biCompression;
            public int biSizeImage;
            public int biXPelsPerMeter;
            public int biYPelsPerMeter;
            public int biClrUsed;
            public int biClrImportant;
        }

        public static class BinaryStructConverter
        {
            public static T FromByteArray<T>(byte[] bytes) where T : struct
            {
                IntPtr ptr = IntPtr.Zero;
                try
                {
                    int size = Marshal.SizeOf(typeof(T));
                    ptr = Marshal.AllocHGlobal(size);
                    Marshal.Copy(bytes, 0, ptr, size);
                    object obj = Marshal.PtrToStructure(ptr, typeof(T));
                    return (T)obj;
                }
                finally
                {
                    if (ptr != IntPtr.Zero)
                        Marshal.FreeHGlobal(ptr);
                }
            }

            public static byte[] ToByteArray<T>(T obj) where T : struct
            {
                IntPtr ptr = IntPtr.Zero;
                try
                {
                    int size = Marshal.SizeOf(typeof(T));
                    ptr = Marshal.AllocHGlobal(size);
                    Marshal.StructureToPtr(obj, ptr, true);
                    byte[] bytes = new byte[size];
                    Marshal.Copy(ptr, bytes, 0, size);
                    return bytes;
                }
                finally
                {
                    if (ptr != IntPtr.Zero)
                        Marshal.FreeHGlobal(ptr);
                }
            }
        }

        internal void UploadClipboard()
        {
            UploadClipboard("clip");
        }

        internal void UploadClipboard(string filename)
        {
            if (!puush.EnsureLogin()) return;

            byte[] content = null;

            IDataObject data = Clipboard.GetDataObject();

            if (data == null)
            {
                puush.ShowErrorBalloon("Nothing found in the clipboard!");
                return;
            }

            List<string> formats = new List<string>(data.GetFormats());

            if (formats.Contains(DataFormats.UnicodeText))
            {
                string html = (string)data.GetData(DataFormats.UnicodeText);
                content = UTF8Encoding.UTF8.GetBytes(html);
                filename = "clipboard.txt";
                FileUpload.Upload(content, filename);

            }
            else if (formats.Contains(DataFormats.Dib))
            {
                MemoryStream ms = Clipboard.GetData("DeviceIndependentBitmap") as MemoryStream;
                if (ms != null)
                {
                    byte[] dibBuffer = new byte[ms.Length];
                    ms.Read(dibBuffer, 0, dibBuffer.Length);

                    BITMAPINFOHEADER infoHeader =
                        BinaryStructConverter.FromByteArray<BITMAPINFOHEADER>(dibBuffer);

                    int fileHeaderSize = Marshal.SizeOf(typeof(BITMAPFILEHEADER));
                    int infoHeaderSize = infoHeader.biSize;
                    int fileSize = fileHeaderSize + infoHeader.biSize + infoHeader.biSizeImage;

                    BITMAPFILEHEADER fileHeader = new BITMAPFILEHEADER();
                    fileHeader.bfType = BITMAPFILEHEADER.BM;
                    fileHeader.bfSize = fileSize;
                    fileHeader.bfReserved1 = 0;
                    fileHeader.bfReserved2 = 0;
                    fileHeader.bfOffBits = fileHeaderSize + infoHeaderSize + infoHeader.biClrUsed * 4;

                    byte[] fileHeaderBytes =
                        BinaryStructConverter.ToByteArray<BITMAPFILEHEADER>(fileHeader);

                    MemoryStream msBitmap = new MemoryStream();
                    msBitmap.Write(fileHeaderBytes, 0, fileHeaderSize);
                    msBitmap.Write(dibBuffer, 0, dibBuffer.Length);
                    msBitmap.Seek(0, SeekOrigin.Begin);

                    FileUpload.UploadImage((Bitmap)Bitmap.FromStream(msBitmap), filename);
                    return;
                }
            }
            else if (formats.Contains(DataFormats.Bitmap))
            {
                FileUpload.UploadImage((Bitmap)data.GetData(DataFormats.Bitmap), filename);
                return;
            }
            else if (formats.Contains(DataFormats.FileDrop))
            {
                StringCollection selectedFiles = Clipboard.GetFileDropList();
                string list = "";
                foreach (string s in selectedFiles)
                    list += s + "\n";

                if (TopMostMessageBox.Show(list + "puush " + (selectedFiles.Count > 1 ? "these files?" : "this file?"), "puush", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                {
                    foreach (string file in selectedFiles)
                        FileUpload.Upload(file);
                }
                return;
            }
            else
            {
                puush.ShowErrorBalloon("Clipboard contents are in an unsupported format.");
            }
        }

        static bool uploadFileDialogVisible;

        private void UploadFile()
        {
            if (!puush.EnsureLogin()) return;

            if (uploadFileDialogVisible) return;
            uploadFileDialogVisible = true;

            //check is explorer is active

            IntPtr windowPtr = GetForegroundWindow();

            uint processId = 0;
            GetWindowThreadProcessId(windowPtr, out processId);

            Process activeWindow = Process.GetProcessById((int)processId);

            if (activeWindow.ProcessName == "explorer")
            {
                while (PressedKeys.Count > 0)
                    Thread.Sleep(100);

                bool didPuush = false;

                Invoke(delegate
                {
                    try
                    {
                        //store previous clipboard contents...
                        IDataObject lastContents = Clipboard.GetDataObject();
                        Clipboard.Clear();

                        SendKeys.SendWait("^c");

                        if (Clipboard.ContainsFileDropList())
                        {
                            StringCollection selectedFiles = Clipboard.GetFileDropList();
                            string list = "";
                            foreach (string s in selectedFiles)
                                list += s + "\n";

                            if (TopMostMessageBox.Show(list + "puush " + (selectedFiles.Count > 1 ? "these files?" : "this file?"), "puush", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                            {
                                foreach (string file in selectedFiles)
                                    FileUpload.Upload(file);
                            }

                            uploadFileDialogVisible = false;
                            didPuush = true;
                        }

                        //restore contents...
                        Clipboard.SetDataObject(lastContents);
                    }
                    catch
                    {
                        //likely a clipboard error
                    }
                }, true);

                if (didPuush) return;
            }

            bool wasSuccessful = false;

            Invoke(delegate
                {
                    wasSuccessful = openFileDialog1.ShowDialog(this) == DialogResult.OK && !string.IsNullOrEmpty(openFileDialog1.FileName);
                });

            if (wasSuccessful)
            {
                foreach (String file in openFileDialog1.FileNames)
                {
                    FileUpload.Upload(file);
                }
            }

            //topmostForm.Dispose();
            uploadFileDialogVisible = false;
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                // turn on WS_EX_TOOLWINDOW style bit
                cp.ExStyle |= 0x80;
                return cp;
            }
        }

        private void trayResetTimer_Tick(object sender, EventArgs e)
        {
            System.Windows.Forms.Timer t = sender as System.Windows.Forms.Timer;
            t.Stop();

            SetTrayIcon(Resources.tray, "puush");
        }

        private void UpdateArbitraryPosition(Point location)
        {
            if (selectionForm == null || !updatePosition) return;

            selectionForm.Location = new Point(location.X - 4, location.Y - 4);
            selectionForm.Size = new Size(8, 8);
            selectionForm.Opacity = 0.01;
        }

        internal static void SetTrayIcon(Icon icon, string tooltip)
        {
            SetTrayIcon(icon, tooltip, 0);
        }

        internal static void SetTrayIcon(Icon icon, string tooltip, int timeToReset)
        {
            Instance.Invoke(delegate
            {
                if (Instance.trayIcon.Icon != null)
                    Instance.trayIcon.Icon.Dispose();
                Instance.trayIcon.Icon = icon;
                Instance.trayIcon.Text = tooltip;

                if (timeToReset > 0)
                {
                    Instance.trayResetTimer.Interval = timeToReset;
                    Instance.trayResetTimer.Start();
                }
            });
        }


        void toolStripMenuItemCancelUpload_Click(object sender, System.EventArgs e)
        {
            FileUpload.CancelCurrent();
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_HOTKEY = 0x0312;

            switch (m.Msg)
            {
                case WM_HOTKEY:
                    {
                        switch (BindingManager.Check((short)m.WParam))
                        {
                            case KeyBinding.ScreenSelection:
                                PressedKeys.Clear();
                                if (puush.config.GetValue<bool>("disableupload", false))
                                    return;
                                invokeMeSome(StartSelection);
                                return;
                            case KeyBinding.FullscreenScreenshot:
                                PressedKeys.Clear();
                                if (puush.config.GetValue<bool>("disableupload", false))
                                    return;
                                invokeMeSome(ScreenshotCapture.Run);
                                return;
                            case KeyBinding.CurrentWindowScreenshot:
                                PressedKeys.Clear();
                                if (puush.config.GetValue<bool>("disableupload", false))
                                    return;
                                invokeMeSome(ScreenshotCapture.RunWindow);
                                return;
                            case KeyBinding.UploadClipboard:
                                PressedKeys.Clear();
                                if (puush.config.GetValue<bool>("disableupload", false))
                                    return;
                                invokeMeSome(UploadClipboard);
                                break;
                            case KeyBinding.UploadFile:
                                PressedKeys.Clear();
                                if (puush.config.GetValue<bool>("disableupload", false))
                                    return;
                                threadMeSome(UploadFile);
                                return;
                            case KeyBinding.Toggle:
                                Toggle();
                                return;
                        }

                        break;
                    }
                default:
                    {
                        base.WndProc(ref m);
                        break;
                    }
            }
        }

        private void toolStripMenuItemUploadClipboard_Click(object sender, EventArgs e)
        {
            UploadClipboard();
        }
    }
}
