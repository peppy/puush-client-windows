using System;
using System.Collections.Generic;
using System.Text;
using osu_common.Libraries.NetLib;
using System.Windows.Forms;
using System.Threading;

namespace puush
{
    internal static class HistoryManager
    {
        internal static List<HistoryItem> HistoryItems = new List<HistoryItem>();

        internal static bool UpdateScheduled;

        internal static void Update()
        {
            if (!puush.IsLoggedIn)
                return;

            FormNetRequest request = new FormNetRequest(puush.getApiUrl("hist"));
            request.request.Items.AddFormField("k", puush.config.GetValue<string>("key", ""));
            request.onFinish += new FormNetRequest.RequestCompleteHandler(historyRetrieval_onFinish);

            NetManager.AddRequest(request);
        }

        internal static void historyRetrieval_onFinish(string _result, Exception e)
        {
            // if (!UpdateScheduled)
            // {
            //     UpdateScheduled = true;

            //     MainForm.threadMeSome(delegate {
            //         Thread.Sleep(60000 * 5); //wait 5 minutes
            //         UpdateScheduled = false;
            //         Update();
            //     });
            // }

            if (e != null) return;

            MainForm.Instance.Invoke(delegate
            {
                ContextMenuStrip menu = MainForm.Instance.contextMenuStrip1;

                foreach (HistoryItem i in HistoryItems)
                {
                    menu.Items.Remove(i);
                    i.Dispose();
                }

                HistoryItems.Clear();

                int response;

                string[] lines = _result.Split('\n');

                if (!Int32.TryParse(lines[0], out response))
                    response = -2;

                switch (response)
                {
                    case -1:
                        //puush.HandleInvalidAuthentication();
                        //todo: reimplement this after server-side handles correctly.
                        return;
                    case -2:
                        //unknown/other
                        return;
                }


                int displayCount = 5;

                try
                {

                    bool firstLine = true;
                    foreach (string line in lines)
                    {
                        if (firstLine)
                        {
                            firstLine = false;
                            continue;
                        }

                        if (displayCount-- == 0) break;

                        if (line.Length == 0) break;

                        string[] parts = line.Split(',');

                        int id = Int32.Parse(parts[0]);
                        string date = parts[1];
                        string url = parts[2];
                        string filename = parts[3];
                        int viewCount = Int32.Parse(parts[4]);

                        HistoryItems.Insert(0,new HistoryItem(id, date, url, filename,viewCount));
                    }
                }
                catch { }

                foreach (HistoryItem i in HistoryItems) menu.Items.Insert(4, i);

            });

        }
    }
}
