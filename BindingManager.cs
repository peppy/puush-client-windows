using System;
using System.Collections.Generic;
using System.Text;
using osu_common.Helpers;
using System.Windows.Forms;

namespace puush
{
    enum KeyBinding
    {
        None = 0,
        ScreenSelection,
        FullscreenScreenshot,
        UploadFile,
        CurrentWindowScreenshot,
        Toggle,
        UploadClipboard
    }

    internal static class BindingManager
    {
        static Dictionary<KeyBinding, List<Keys>> bindings = new Dictionary<KeyBinding, List<Keys>>();
        static Dictionary<KeyBinding, GlobalHotKey> boundKeys = new Dictionary<KeyBinding, GlobalHotKey>();

        /// <summary>
        /// Binding configuration file values to internal dictionary.  Also sets defaults if they are not initialized.
        /// </summary>
        internal static void Bind()
        {
            if (boundKeys.Count > 0)
            {
                foreach (GlobalHotKey k in boundKeys.Values)
                    k.Register();
            }
            else
            {
                ReadBindingFromConfig(KeyBinding.ScreenSelection, new Keys[] { Keys.LControlKey, Keys.LShiftKey, Keys.D4 });
                ReadBindingFromConfig(KeyBinding.FullscreenScreenshot, new Keys[] { Keys.LControlKey, Keys.LShiftKey, Keys.D3 });
                ReadBindingFromConfig(KeyBinding.CurrentWindowScreenshot, new Keys[] { Keys.LControlKey, Keys.LShiftKey, Keys.D2 });
                ReadBindingFromConfig(KeyBinding.UploadFile, new Keys[] { Keys.LControlKey, Keys.LShiftKey, Keys.U });
                ReadBindingFromConfig(KeyBinding.UploadClipboard, new Keys[] { Keys.LControlKey, Keys.LShiftKey, Keys.D5 });
                ReadBindingFromConfig(KeyBinding.Toggle, new Keys[] { Keys.LControlKey, Keys.LMenu, Keys.P });
            }
        }

        /// <summary>
        /// Reads a single binding combination from config file.
        /// </summary>
        /// <param name="keyBinding"></param>
        /// <param name="keys"></param>
        private static void ReadBindingFromConfig(KeyBinding keyBinding, Keys[] keys)
        {
            string configValue = puush.config.GetValue<string>("b_" + keyBinding.ToString(), null);

            List<Keys> finalKeys = new List<Keys>(keys);

            if (!string.IsNullOrEmpty(configValue))
            {
                finalKeys.Clear();

                string[] spl = configValue.Split('-');  

                foreach (string s in spl)
                {
                    try
                    {
                        finalKeys.Add((Keys)Enum.Parse(typeof(Keys), s));
                    }
                    catch
                    {
                        continue;
                    }
                }
            }

            SetBindingFor(keyBinding, finalKeys.Count > 0 ? finalKeys : new List<Keys>(keys));
        }

        internal static void SetBindingFor(KeyBinding keyBinding, List<Keys> finalKeys)
        {
            if (finalKeys.Count == 0) return;

            bindings[keyBinding] = finalKeys;

            string representation = "";

            foreach (Keys k in finalKeys)
                representation += k.ToString() + "-";

            representation = representation.TrimEnd('-');

            puush.config.SetValue<string>("b_" + keyBinding.ToString(), representation);

            switch (keyBinding)
            {
                case KeyBinding.FullscreenScreenshot:
                    MainForm.Instance.Invoke(delegate { MainForm.Instance.toolStripMenuItemDesktop.ShortcutKeyDisplayString = GetStringRepresentationFor(keyBinding); });
                    break;
                case KeyBinding.CurrentWindowScreenshot:
                    MainForm.Instance.Invoke(delegate { MainForm.Instance.toolStripMenuItemWindow.ShortcutKeyDisplayString = GetStringRepresentationFor(keyBinding); });
                    break;
                case KeyBinding.ScreenSelection:
                    MainForm.Instance.Invoke(delegate { MainForm.Instance.toolStripMenuItemSelection.ShortcutKeyDisplayString = GetStringRepresentationFor(keyBinding); });
                    break;
                case KeyBinding.UploadFile:
                    MainForm.Instance.Invoke(delegate { MainForm.Instance.toolStripMenuItemUploadFile.ShortcutKeyDisplayString = GetStringRepresentationFor(keyBinding); });
                    break;
                case KeyBinding.UploadClipboard:
                    MainForm.Instance.Invoke(delegate { MainForm.Instance.toolStripMenuItemUploadClipboard.ShortcutKeyDisplayString = GetStringRepresentationFor(keyBinding); });
                    break;
                case KeyBinding.Toggle:
                    MainForm.Instance.Invoke(delegate { MainForm.Instance.toolStripMenuItemUploadDisabled.ShortcutKeyDisplayString = GetStringRepresentationFor(keyBinding); });
                    break;
            }

            if (boundKeys.ContainsKey(keyBinding))
                boundKeys[keyBinding].Dispose();

            int key = 0;
            int modifiers = 0;
            foreach (Keys k in finalKeys)
            {
                switch (k)
                {
                    case Keys.Shift:
                    case Keys.LShiftKey:
                    case Keys.RShiftKey:
                        modifiers |= GlobalHotKey.MOD_SHIFT;
                        break;
                    case Keys.Alt:
                    case Keys.LMenu:
                    case Keys.RMenu:
                        modifiers |= GlobalHotKey.MOD_ALT;
                        break;
                    case Keys.Control:
                    case Keys.LControlKey:
                    case Keys.RControlKey:
                        modifiers |= GlobalHotKey.MOD_CONTROL;
                        break;
                    case Keys.LWin:
                    case Keys.RWin:
                        modifiers |= GlobalHotKey.MOD_WIN;
                        break;
                    default:
                        key = (int)k;
                        break;
                }
            }

            GlobalHotKey hkey = new GlobalHotKey(key, modifiers);
            hkey.Register();

            boundKeys[keyBinding] = hkey;
        }

        internal static string GetStringRepresentationFor(KeyBinding keyBinding)
        {
            return GetStringRepresentationFor(bindings[keyBinding]);
        }

        internal static string GetStringRepresentationFor(List<Keys> keys)
        {
            string representation = "";

            bool ctrl = false;
            bool shift = false;
            bool win = false;
            bool alt = false;

            foreach (Keys k in keys)
            {
                switch (k)
                {
                    default:
                        string keyStr = k.ToString();
                        int intTest = 0;
                        if (keyStr.Length > 1 && Int32.TryParse(keyStr.Substring(1), out intTest))
                            representation += intTest.ToString();
                        else
                            representation += k.ToString();
                            
                        break;
                    case Keys.LControlKey:
                    case Keys.RControlKey:
                        ctrl = true;
                        break;
                    case Keys.LShiftKey:
                    case Keys.RShiftKey:
                        shift = true;
                        break;
                    case Keys.LWin:
                    case Keys.RWin:
                        win = true;
                        break;
                    case Keys.LMenu:
                    case Keys.RMenu:
                        alt = true;
                        break;
                    
                }
            }

            if (alt) representation = "Alt+" + representation;
            if (shift) representation = "Shift+" + representation;
            if (ctrl) representation = "Ctrl+" + representation;
            if (win) representation = "Win+" + representation;

            return representation.TrimEnd('-');
        }

        internal static KeyBinding Check(List<Keys> pressedKeys)
        {
            foreach (KeyValuePair<KeyBinding, List<Keys>> kvp in bindings)
            {
                if (kvp.Value.Count == 0)
                    continue;

                bool success = true;

                foreach (Keys k in kvp.Value)
                {
                    if (!pressedKeys.Contains(k))
                    {
                        success = false;
                        break;
                    }
                }

                if (success)
                    return kvp.Key;
            }

            return KeyBinding.None;
        }

        internal static KeyBinding Check(short p)
        {
            foreach (KeyValuePair<KeyBinding, GlobalHotKey> kvp in boundKeys)
            {
                if (kvp.Value == null)
                    continue;

                if (kvp.Value.id == p)
                    return kvp.Key;
            }

            return KeyBinding.None;
        }
    }
}
