using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using osu_common.Libraries.NetLib;
using System.Runtime.InteropServices;
using System.Diagnostics;
using puush.Libraries;
using System.Threading;

namespace puush
{
    public static class ScreenshotCapture
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        internal static void RunWindow()
        {
            if (!puush.EnsureLogin()) return;

            RECT rct;

            if (GetWindowRect(puush.LastForegroundWindow, out rct))
                Run(rct.Left, rct.Right, rct.Top, rct.Bottom);
        }

        internal static void Run()
        {
            if (!puush.EnsureLogin()) return;

            int? minX = null;
            int? minY = null;
            int? maxX = null;
            int? maxY = null;

            List<Screen> screens = new List<Screen>();

            switch ((FullscreenCaptureMode)puush.config.GetValue<int>("FullscreenMode", 0))
            {
                case FullscreenCaptureMode.AllScreens:
                    screens.AddRange(Screen.AllScreens);
                    break;
                case FullscreenCaptureMode.Mouse:
                    foreach (Screen s in Screen.AllScreens)
                    {
                        if (s.Bounds.Contains(Cursor.Position))
                            screens.Add(s);
                    }
                    break;
                case FullscreenCaptureMode.Primary:
                    screens.Add(Screen.PrimaryScreen);
                    break;
            }

            //first find the full range of available bounds...
            foreach (Screen s in screens)
            {
                if (minX == null || s.Bounds.Left < minX) minX = s.Bounds.Left;
                if (maxX == null || s.Bounds.Right > maxX) maxX = s.Bounds.Right;
                if (minY == null || s.Bounds.Top < minY) minY = s.Bounds.Top;
                if (maxY == null || s.Bounds.Bottom > maxY) maxY = s.Bounds.Bottom;
            }

            Run(minX.Value, maxX.Value, minY.Value, maxY.Value);
        }

        [DllImport("dwmapi.dll", EntryPoint = "DwmIsCompositionEnabled")]
        public static extern int DwmIsCompositionEnabled(out bool enabled);

        internal static unsafe void Run(int minX, int maxX, int minY, int maxY)
        {
            try
            {
                int width = maxX - minX;
                int height = maxY - minY;

                RECT rct;
                if (GetWindowRect(puush.LastForegroundWindow, out rct))
                {
                    Rectangle screen = Screen.PrimaryScreen.Bounds;

                    /*if (rct.Left == 0 && rct.Top == 0 && rct.Right == screen.Width && rct.Bottom == screen.Height)
                    {
                        SendKeys.SendWait("{PRTSC}");
                        Thread.Sleep(250);
                        MainForm.Instance.UploadClipboard("ss");
                        return;
                    }*/
                }

                using (Bitmap b = new Bitmap(width, height, PixelFormat.Format24bppRgb))
                using (Graphics g = Graphics.FromImage(b))
                {
                    try
                    {
                        bool aero;
                        DwmIsCompositionEnabled(out aero);
                        if (!aero) Thread.Sleep(200);
                    }
                    catch { }

                    g.CopyFromScreen(minX, minY, 0, 0, new Size(width, height), CopyPixelOperation.SourceCopy);

                    FileUpload.UploadImage(b, "ss");
                }
            }
            catch (Exception e)
            {
                TopMostMessageBox.Show("Failed to take screenshot.  Please report this if it should have worked!\n" + e.ToString(), "puush", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
