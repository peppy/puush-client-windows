using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using puush;

/// <summary> This class allows you to manage a hotkey </summary>
public class GlobalHotKey : IDisposable
{
    [DllImport("user32", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool RegisterHotKey(IntPtr hwnd, int id, uint fsModifiers, uint vk);
    [DllImport("user32", SetLastError = true)]
    public static extern int UnregisterHotKey(IntPtr hwnd, int id);
    [DllImport("kernel32", SetLastError = true)]
    public static extern short GlobalAddAtom(string lpString);
    [DllImport("kernel32", SetLastError = true)]
    public static extern short GlobalDeleteAtom(short nAtom);

    public const int MOD_ALT = 1;
    public const int MOD_CONTROL = 2;
    public const int MOD_SHIFT = 4;
    public const int MOD_WIN = 8;

    public const int WM_HOTKEY = 0x312;

    int hotkey, modifiers;

    public GlobalHotKey(int hotkey, int modifiers)
    {
        Handle = MainForm.Instance.Handle;
        this.hotkey = hotkey;
        this.modifiers = modifiers;
    }

    /// <summary>Handle of the current process</summary>
    public IntPtr Handle;

    /// <summary>The ID for the hotkey</summary>
    public short id { get; private set; }

    /// <summary>Register the hotkey. Re-registers if already registered.</summary>
    public void Register()
    {
        UnregisterGlobalHotKey();

        try
        {
            // use the GlobalAddAtom API to get a unique ID (as suggested by MSDN)
            string atomName = Thread.CurrentThread.ManagedThreadId.ToString("X8") + "puush" + hotkey + modifiers;
            id = GlobalAddAtom(atomName);
            if (id == 0)
                throw new Exception("Unable to generate unique hotkey ID. Error: " + Marshal.GetLastWin32Error().ToString());

            // register the hotkey, throw if any error
            if (!RegisterHotKey(Handle, id, (uint)modifiers, (uint)hotkey))
                throw new Exception("Unable to register hotkey. Error: " + Marshal.GetLastWin32Error().ToString());

        }
        catch (Exception ex)
        {
            // clean up if hotkey registration failed
            Dispose();
            Console.WriteLine(ex);
        }
    }

    /// <summary>Unregister the hotkey</summary>
    public void UnregisterGlobalHotKey()
    {
        if (this.id != 0)
        {
            UnregisterHotKey(Handle, id);
            // clean up the atom list
            GlobalDeleteAtom(id);
            id = 0;
        }
    }

    public void Dispose()
    {
        UnregisterGlobalHotKey();
    }
}