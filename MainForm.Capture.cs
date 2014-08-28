using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using Gma.UserActivityMonitor;
using System.Windows.Forms;

namespace puush
{
    partial class MainForm
    {
        private static object SelectionLock = new object();

        bool updatePosition = true;

        private void StartSelection()
        {
            if (!puush.EnsureLogin()) return;

            lock (SelectionLock) //ensure we only enter here once.
            {
                if (captureMode) return; //we are already mid-capture.

                HookManager.MouseMove += HookManager_MouseMove;
                HookManager.MouseDown += HookManager_MouseDown;
                HookManager.MouseUp += HookManager_MouseUp;
                HookManager.KeyDown += HookManager_KeyDown;

                captureMode = true;
            }

            p1 = new Point(-1, -1);
            p2 = new Point(-1, -1);

            if (selectionForm != null)
                selectionForm.Dispose();

            updatePosition = puush.config.GetValue<bool>("selectionrectangle", true);
            if (!updatePosition && puush.config.GetValue<bool>("notificationsound", true)) Console.Beep(500, 150);

            selectionForm = new ScreenSelection();
            selectionForm.Show();

            UpdateArbitraryPosition(Cursor.Position);
        }

        private void EndSelection()
        {
            HookManager.MouseMove -= HookManager_MouseMove;
            HookManager.MouseDown -= HookManager_MouseDown;
            HookManager.MouseUp -= HookManager_MouseUp;
            HookManager.KeyDown -= HookManager_KeyDown;

            //Cursor.Current = Cursors.Arrow;

            captureMode = false;
            selectionMode = false;

            if (selectionForm != null)
            {
                selectionForm.Close();
                selectionForm.Dispose();
            }
            selectionForm = null;
        }

        bool captureMode;
        bool selectionMode;
        ScreenSelection selectionForm;
        Point p1;
        Point p2;

        void HookManager_MouseUp(object sender, MouseEventExtArgs e)
        {
            if (!selectionMode) return;

            EndSelection();

            if (Math.Abs(p1.X - p2.X) + Math.Abs(p1.Y - p2.Y) > 10 && p2.X != -1 && p2.Y != -1)
            {
                Invoke(delegate { ScreenshotCapture.Run(Math.Min(p1.X, p2.X), Math.Max(p1.X, p2.X), Math.Min(p1.Y, p2.Y), Math.Max(p1.Y, p2.Y)); });
            }
            else
                Invoke(StartSelection);

            e.Handled = true;
        }

        void HookManager_MouseDown(object sender, MouseEventExtArgs e)
        {
            //selectionForm.panel1.Visible = true;
            if (selectionForm != null) selectionForm.Opacity = updatePosition ? 0.3 : 0.05;
            selectionMode = true;
            e.Handled = true;

            p1 = e.Location;
        }

        void HookManager_MouseMove(object sender, MouseEventExtArgs e)
        {
            if (selectionMode)
            {
                p2 = e.Location;

                if (selectionForm != null && updatePosition)
                    selectionForm.Bounds = new Rectangle(
                        Math.Min(p1.X, p2.X) - 1, Math.Min(p1.Y, p2.Y) - 1,
                        Math.Max(p1.X, p2.X) - selectionForm.Location.X + 2, Math.Max(p1.Y, p2.Y) - selectionForm.Location.Y + 2);
            }
            else
            {
                UpdateArbitraryPosition(e.Location);
            }
        }
    }
}
