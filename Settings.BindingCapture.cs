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

namespace puush
{
    public partial class Settings : pForm
    {
        internal static bool IsKeyCapturing;

        Button keyCapturingButton;
        private KeyBinding keyCapturingBinding;
        private List<Keys> capturedKeys;

        private void StartKeyCapture(Button button, KeyBinding keyBinding)
        {
            if (IsKeyCapturing)
                EndKeyCapture();
            
            IsKeyCapturing = true;

            button.Text = "Press some keys...";
            button.ForeColor = Color.Blue;

            keyCapturingButton = button;
            keyCapturingBinding = keyBinding;

            HookManager.KeyDown += new KeyEventHandler(HookManager_KeyDown);

            capturedKeys = new List<Keys>();

        }

        void HookManager_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                EndKeyCapture();
                return;
            }

            if (capturedKeys.Contains(e.KeyCode))
                return;

            capturedKeys.Add(e.KeyCode);

            Invoke(delegate { keyCapturingButton.Text = BindingManager.GetStringRepresentationFor(capturedKeys); });

            e.Handled = true;

            switch (e.KeyCode)
            {
                case Keys.LShiftKey:
                case Keys.RShiftKey:
                case Keys.LControlKey:
                case Keys.RControlKey:
                case Keys.LMenu:
                case Keys.RMenu:
                case Keys.LWin:
                case Keys.RWin:
                    return;
                default:
                    EndKeyCapture();
                    break;
            }
        }

        private void EndKeyCapture()
        {
            if (!IsKeyCapturing) return;

            Invoke(delegate { keyCapturingButton.ForeColor = Color.Black; });

            BindingManager.SetBindingFor(keyCapturingBinding, capturedKeys);
            keyCapturingButton.Text = BindingManager.GetStringRepresentationFor(keyCapturingBinding);

            keyCapturingButton = null;
            IsKeyCapturing = false;

            HookManager.KeyDown -= new KeyEventHandler(HookManager_KeyDown);
        }
    }

}