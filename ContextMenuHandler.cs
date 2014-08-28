using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Microsoft.Win32;

namespace puush
{
    class ContextMenuHandler
    {
        internal const string MenuName = "*\\shell\\puush";
        internal const string Command = "*\\shell\\puush\\command";

        internal static bool Install()
        {
            RegistryKey regmenu = null;
            RegistryKey regcmd = null;

            try
            {
                regmenu = Registry.ClassesRoot.OpenSubKey(MenuName);
                if (regmenu != null) return true;
                regmenu = Registry.CurrentUser.OpenSubKey("Software").OpenSubKey("Classes").OpenSubKey("*").OpenSubKey(MenuName);
                if (regmenu != null) return true;

                try
                {
                    regmenu = Registry.ClassesRoot.CreateSubKey(MenuName);
                }
                catch
                {
                    regmenu = Registry.CurrentUser.OpenSubKey("Software").OpenSubKey("Classes").OpenSubKey("*").CreateSubKey(MenuName);
                }

                if (regmenu != null)
                    regmenu.SetValue("", "puush");

                regcmd = Registry.ClassesRoot.CreateSubKey(Command);
                if (regcmd != null)
                {
                    string cmdLine = Environment.CommandLine;
                    string[] parts = cmdLine.Split(' ');

                    cmdLine = "";
                    foreach (string part in parts)
                    {
                        if (part.StartsWith("-"))
                            break;
                        cmdLine += " " + part;
                    }

                    cmdLine = cmdLine.Trim();

                    regcmd.SetValue("", cmdLine + " -upload %1");
                }
            }
            catch (Exception ex)
            {
                if (!puush.IsRunningElevated && !puush.config.GetValue<bool>("contextmenuattempted", false))
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.UseShellExecute = true;
                    startInfo.WorkingDirectory = Environment.CurrentDirectory;
                    startInfo.FileName = "puush.exe";
                    startInfo.Arguments = "-setPermissions";
                    startInfo.Verb = "runas";

                    try
                    {
                        puush.config.SetValue<bool>("contextmenuattempted", true);
                        puush.config.SaveConfig();

                        Process pr = Process.Start(startInfo);
                        if (pr != null) pr.WaitForExit(8000);
                        return true;
                    }
                    catch (System.ComponentModel.Win32Exception)
                    {
                        return false;
                    }
                }
            }
            finally
            {
                if (regmenu != null)
                    regmenu.Close();
                if (regcmd != null)
                    regcmd.Close();
            }

            return true;
        }

        internal static void Remove()
        {
            try
            {
                RegistryKey reg = Registry.ClassesRoot.OpenSubKey(Command);
                if (reg != null)
                {
                    reg.Close();
                    Registry.ClassesRoot.DeleteSubKey(Command);
                }
                reg = Registry.ClassesRoot.OpenSubKey(MenuName);
                if (reg != null)
                {
                    reg.Close();
                    Registry.ClassesRoot.DeleteSubKey(MenuName);
                }
            }
            catch (Exception ex)
            {
                if (!puush.IsRunningElevated)
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.UseShellExecute = true;
                    startInfo.WorkingDirectory = Environment.CurrentDirectory;
                    startInfo.FileName = "puush.exe";
                    startInfo.Arguments = "-removeContext";
                    startInfo.Verb = "runas";

                    try
                    {
                        Process pr = Process.Start(startInfo);
                        if (pr != null) pr.WaitForExit(8000);
                    }
                    catch (System.ComponentModel.Win32Exception)
                    {
                    }
                }
            }
        }
    }
}
