using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Runtime.Remoting;

namespace puush
{
    public class IpcLoader : MarshalByRefObject
    {
        public void puush(string filename)
        {
            FileUpload.Upload(filename);
        }
    }

    internal static class IPC
    {
        internal static IpcChannel Channel;
        internal static void AcceptConnections()
        {
            bool accepted = acceptConnections();

            if (accepted)
                return;

            //kill other instances?
            return;
        }

        internal static void Unregister()
        {
            if (Channel != null)
                ChannelServices.UnregisterChannel(Channel);
        }

        private static bool killOtherOsu()
        {
            try
            {
                IpcChannel ipcCh = new IpcChannel("puush-incoming");
                ChannelServices.RegisterChannel(ipcCh, false);

                IpcLoader otherOsu = (IpcLoader)Activator.GetObject(typeof(IpcLoader), "ipc://puush/puush");

                ChannelServices.UnregisterChannel(ipcCh);

                return true;
            }
            catch { }

            return false;
        }

        private static bool acceptConnections()
        {
            try
            {
                Channel = new IpcChannel("puush");

                ChannelServices.RegisterChannel(Channel, false);
                RemotingConfiguration.RegisterWellKnownServiceType(typeof(IpcLoader), "puush", WellKnownObjectMode.Singleton);

                return true;
            }
            catch
            {
                Channel = null;
            }

            return false;
        }

        internal static void LoadFile(string file)
        {
            IpcChannel ipcCh = new IpcChannel("puush-incoming");
            ChannelServices.RegisterChannel(ipcCh, false);

            IpcLoader obj =
                (IpcLoader)Activator.GetObject
                                (typeof(IpcLoader), "ipc://puush/puush");
            obj.puush(file);

            ChannelServices.UnregisterChannel(ipcCh);
        }
    }
}
