using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing;
using puush.Libraries;
using System.IO;
using osu_common.Libraries.NetLib;

namespace puush
{
    internal class HistoryItem : ToolStripMenuItem
    {
        internal int id;
        internal string date;
        internal string url;
        internal string filename;
        internal int viewCount;

        bool thumbnailLoaded = false;


        public HistoryItem(int id, string date, string url, string filename, int viewCount)
            : base(filename)
        {
            DropDownOpening += HistoryItem_DropDownOpening;
            this.id = id;
            this.date = date;
            this.url = url;
            this.filename = filename;
            this.viewCount = viewCount;

            string extension = Path.GetExtension(filename);

            try
            {
                using (Icon icon = Icons.IconFromExtensionShell(extension, Icons.SystemIconSize.Small))
                {
                    if (icon != null)
                    {
                        Image = icon.ToBitmap();
                        Icons.DestroyIcon(icon.Handle);
                    }
                }
            }
            catch
            {
                //this could fail, but in this case we just display no icon at all.
            }

            DropDownItems.Add("Uploaded: " + date);
            DropDownItems[DropDownItems.Count - 1].Enabled = false;

            DropDownItems.Add("Views: " + viewCount);
            DropDownItems[DropDownItems.Count - 1].Enabled = false;

            DropDownItems.Add("Open in browser", null, OpenURL);
            DropDownItems.Add("Copy link to clipboard", null, CopyToClipboard);
            DropDownItems.Add(new ToolStripSeparator());
            DropDownItems.Add("Delete", null, Delete);
        }

        protected override void Dispose(bool disposing)
        {
            if (Image != null)
                Image.Dispose();

            base.Dispose(disposing);
        }

        void HistoryItem_DropDownOpening(object sender, EventArgs e)
        {
            if (!thumbnailLoaded)
            {
                thumbnailLoaded = true;

                FormDataNetRequest request = new FormDataNetRequest(puush.getApiUrl("thumb"));
                request.request.Items.AddFormField("k", puush.config.GetValue<string>("key", ""));
                request.request.Items.AddFormField("i", id.ToString());
                request.onFinish += new FormDataNetRequest.RequestCompleteHandler(request_onFinish);

                NetManager.AddRequest(request);
            }
        }

        void request_onFinish(byte[] thumb, Exception e)
        {
            if (thumb.Length > 0)
            {
                MainForm.invokeMeSome(delegate
                {
                    try
                    {
                        Image image = Image.FromStream(new MemoryStream(thumb), false, false);

                        ToolStripItemThumbnail item = new ToolStripItemThumbnail(image, Width);
                        item.Click += OpenURL;

                        DropDownItems.Insert(0, item);
                    }
                    catch
                    {
                        //getting here means the image we downloaded is likely no an image or corrupt. ignore.
                    }
                });
            }
        }

        public void OpenURL(object sender, EventArgs args)
        {
            Process.Start(url);
        }

        public void CopyToClipboard(object sender, EventArgs args)
        {
            try
            {
                Clipboard.SetText(url);
            }
            catch
            {
                TopMostMessageBox.Show("Failed to copy URL to clipboard.  Please report this if you believe it shouldn't have failed!", "puush", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void Delete(object sender, EventArgs args)
        {
            FormNetRequest req = new FormNetRequest(puush.getApiUrl("del"));
            req.request.Items.AddFormField("k", puush.config.GetValue<string>("key", ""));
            req.request.Items.AddFormField("i", id.ToString());
            req.request.Items.AddFormField("z", "poop");
            req.onFinish += HistoryManager.historyRetrieval_onFinish;

            NetManager.AddRequest(req);
        }

    }


}
