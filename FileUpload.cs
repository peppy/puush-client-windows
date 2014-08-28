using System;
using System.Collections.Generic;
using System.Text;
using osu_common.Libraries.NetLib;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;
using puush.Properties;
using System.Drawing;
using osu_common.Helpers;
using System.Drawing.Imaging;

namespace puush
{
    internal static class FileUpload
    {

        [DllImport("winmm.dll")]
        internal static extern bool PlaySound(string filename, int module, SoundFlags flags);

        [DllImport("winmm.dll", SetLastError = true)]
        internal static extern bool PlaySound(byte[] pszSound, int module, SoundFlags flags);

        [Flags]
        public enum SoundFlags
        {
            /// <summary>play synchronously (default)</summary>
            SND_SYNC = 0x0000,
            /// <summary>play asynchronously</summary>
            SND_ASYNC = 0x0001,
            /// <summary>silence (!default) if sound not found</summary>
            SND_NODEFAULT = 0x0002,
            /// <summary>pszSound points to a memory file</summary>
            SND_MEMORY = 0x0004,
            /// <summary>loop the sound until next sndPlaySound</summary>
            SND_LOOP = 0x0008,
            /// <summary>don't stop any currently playing sound</summary>
            SND_NOSTOP = 0x0010,
            /// <summary>Stop Playing Wave</summary>
            SND_PURGE = 0x40,
            /// <summary>don't wait if the driver is busy</summary>
            SND_NOWAIT = 0x00002000,
            /// <summary>name is a registry alias</summary>
            SND_ALIAS = 0x00010000,
            /// <summary>alias is a predefined id</summary>
            SND_ALIAS_ID = 0x00110000,
            /// <summary>name is file name</summary>
            SND_FILENAME = 0x00020000,
            /// <summary>name is resource name or atom</summary>
            SND_RESOURCE = 0x00040004
        }

        internal static void Upload(string filename)
        {
            byte[] fileBytes = null;

            try
            {
                using (FileStream stream = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    fileBytes = new byte[stream.Length];

                    if (stream.Length > Int32.MaxValue)
                    {
                        puush.ShowErrorBalloon("Yeah sure, nice try.");
                        return;
                    }

                    stream.Read(fileBytes, 0, (int)stream.Length);
                }

                if (fileBytes == null)
                {
                    puush.ShowErrorBalloon("Failed to read specified file.");
                    return;
                }
            }
            catch
            {
                puush.ShowErrorBalloon("Failed to upload file. May be in use by another application.");
                return;
            }

            Upload(fileBytes, Path.GetFileName(filename));
        }

        internal static void Upload(byte[] p, string filename)
        {
            //check whether we have enough account quota...
            if (puush.config.GetValue<int>("type", 0) == 0)
            {
                long usage = puush.config.GetValue<long>("usage", 0);
                if (p.Length + usage > 190 * 1048576)
                    puush.ShowErrorBalloon("Your account seems quite full. Please consider upgrading to pro or deleting some files!");
            }

            puush.UploadCancellable = true;

            FileUploadNetRequest fileUpload = new FileUploadNetRequest(puush.getApiUrl("up"), p, filename, "f");
            fileUpload.request.Items.AddFormField("k", puush.config.GetValue<string>("key", ""));
            fileUpload.request.Items.AddFormField("c", CryptoHelper.GetMd5String(p));
            fileUpload.request.Items.AddFormField("z", "poop");
            fileUpload.onFinish += new FileUploadNetRequest.RequestCompleteHandler(fileUpload_onFinish);
            fileUpload.onUpdate += new NetRequest.RequestUpdateHandler(fileUpload_onUpdate);

            lock (PendingUploads)
            {
                PendingUploads.Enqueue(fileUpload);
                if (MainForm.panel != null) MainForm.panel.StartUpload(fileUpload);
                ProcessQueue();
            }

        }

        private static void ProcessQueue()
        {
            lock (PendingUploads)
            {
                if (CurrentUpload == null && PendingUploads.Count > 0)
                {
                    retryCount = RETRIES_ALLOWED;
                    CurrentUpload = PendingUploads.Dequeue();
                    NetManager.AddRequest(CurrentUpload);
                }
            }
        }

        static Queue<FileUploadNetRequest> PendingUploads = new Queue<FileUploadNetRequest>();
        internal static FileUploadNetRequest CurrentUpload;

        static void fileUpload_onUpdate(object sender, long current, long total)
        {
            SetProgress((float)current / total * 100);
        }

        internal static void SetProgress(float percentage)
        {
            if (MainForm.panel != null) MainForm.panel.SetProgress(percentage);

            int intPercentage = (int)(percentage / 10) * 10 + (percentage % 10 > 5 ? 10 : 0);

            Icon icon = null;

            switch (intPercentage)
            {
                case 0:
                    icon = Resources.progress0;
                    break;
                case 10:
                    icon = Resources.progress10;
                    break;
                case 20:
                    icon = Resources.progress20;
                    break;
                case 30:
                    icon = Resources.progress30;
                    break;
                case 40:
                    icon = Resources.progress40;
                    break;
                case 50:
                    icon = Resources.progress50;
                    break;
                case 60:
                    icon = Resources.progress60;
                    break;
                case 70:
                    icon = Resources.progress70;
                    break;
                case 80:
                    icon = Resources.progress80;
                    break;
                case 90:
                    icon = Resources.progress90;
                    break;
                case 100:
                    icon = Resources.progress100;
                    break;
                default:
                    icon = Resources.tray;
                    break;
            }

            MainForm.SetTrayIcon(icon, string.Format("puush: Uploading ({0:0.00}%)", percentage));
        }

        const int RETRIES_ALLOWED = 2;

        static int retryCount;

        static void fileUpload_onFinish(string _result, Exception e)
        {
            string[] split = null;

            int response = -2; //unknown error

            if (e == null && !string.IsNullOrEmpty(_result))
            {
                split = _result.Split(',');

                if (!Int32.TryParse(split[0], out response))
                    response = -2;

                long usage = Int64.Parse(split[3]);
                puush.config.SetValue<long>("usage", usage);
            }

            if (e is AbortedException)
            {
                //early exit
                CurrentUpload = null;
                puush.UploadCancellable = false;
                ProcessQueue();

                MainForm.SetTrayIcon(Resources.tray, "puush");

                MainForm.panel.FinishUpload(null, null);

                return;
            }

            bool shouldRetry = false;

            if (response < 0)
            {
                string errorDescription;
                //handle errors
                switch (response)
                {
                    case -1:
                        errorDescription = "Authentication failure";
                        puush.HandleInvalidAuthentication();
                        break;
                    case -2:
                    default:
                        errorDescription = "Connection error";
                        shouldRetry = true;
                        break;
                    case -3:
                        errorDescription = "Checksum error";
                        shouldRetry = true;
                        break;
                    case -4:
                        errorDescription = "Insufficient account storage remaining. Please delete some files or consider upgrading to a pro account!";
                        shouldRetry = false;
                        break;
                }

                if (shouldRetry && retryCount > 0)
                {
                    retryCount--;
                    errorDescription += " - Retrying " + retryCount + " more times...";

                    NetManager.AddRequest(CurrentUpload);
                }
                else
                {
                    CurrentUpload = null;
                    ProcessQueue();
                }

                if (MainForm.panel != null) MainForm.panel.FinishUpload(null, null);
                puush.ShowErrorBalloon(errorDescription);

                return;
            }

            HistoryManager.Update();

            string uploadUrl = split[1];
            if (MainForm.panel != null) MainForm.panel.FinishUpload(uploadUrl, CurrentUpload);

            if (uploadUrl.Length > 0)
            {
                MainForm.SetTrayIcon(Resources.complete, "puush: upload complete!", 2000);

                MainForm.Instance.Invoke((MethodInvoker)delegate
                {
                    try
                    {
                        MainForm.Instance.trayIcon.ShowBalloonTip(5000, "puush complete!", uploadUrl, ToolTipIcon.Info);
                        MainForm.Instance.trayIcon.Tag = uploadUrl;

                        if (puush.config.GetValue<bool>("notificationsound", true))
                        {
                            PlaySound(Resources.success, 0, SoundFlags.SND_ASYNC | SoundFlags.SND_MEMORY);
                        }

                        if (puush.config.GetValue<bool>("copytoclipboard", true))
                            Clipboard.SetText(uploadUrl);

                        if (puush.config.GetValue<bool>("openbrowser", false))
                            Process.Start(uploadUrl);
                    }
                    catch { /*yes a lot could happen in here and we should care, right? no.*/ }
                });
            }

            CurrentUpload = null;
            puush.UploadCancellable = false;

            ProcessQueue();
        }

        internal static void CancelCurrent()
        {
            NetRequest current = CurrentUpload;
            try
            {
                if (current != null)
                {
                    current.Abort();
                    MainForm.Instance.Invoke((MethodInvoker)delegate
                    {
                        MainForm.Instance.trayIcon.ShowBalloonTip(1000, "puush","Upload cancelled!", ToolTipIcon.Info);
                        MainForm.Instance.trayIcon.Tag = null;
                    });
                }

            }
            catch { }
        }

        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {

            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }

        internal static void UploadImage(Bitmap b, string filename)
        {
            if (!puush.EnsureLogin()) return;

            filename += string.Format(" ({0:yyyy-MM-dd} at {0:hh.mm.ss})", DateTime.Now);

            UploadQuality quality = (UploadQuality)puush.config.GetValue<int>("uploadquality", (int)UploadQuality.High);
            byte[] image = null;

            switch (quality)
            {
                case UploadQuality.Best:
                    using (MemoryStream stream = new MemoryStream())
                    {
                        b.Save(stream, ImageFormat.Png);
                        filename += ".png";
                        image = stream.ToArray();
                    }
                    break;
                case UploadQuality.High:
                case UploadQuality.Medium:

                    using (MemoryStream pngStream = new MemoryStream())
                    using (MemoryStream jpgStream = new MemoryStream())
                    {
                        b.Save(pngStream, ImageFormat.Png);

                        ImageCodecInfo jpegEncoder = GetEncoder(ImageFormat.Jpeg);
                        System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Quality;
                        EncoderParameters param = new EncoderParameters(1);
                        param.Param[0] = new EncoderParameter(myEncoder, quality == UploadQuality.High ? 95 : 80);
                        b.Save(jpgStream, jpegEncoder, param);

                        if (pngStream.Length < jpgStream.Length)
                        {
                            filename += ".png";
                            image = pngStream.ToArray();
                        }
                        else
                        {
                            filename += ".jpg";
                            image = jpgStream.ToArray();
                        }
                    }
                    //find the number of unique colours in the image    
                    /*BitmapData bitmapData = b.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, b.PixelFormat);
                    Dictionary<int, bool> colours = new Dictionary<int, bool>();
                    IntPtr pixels = bitmapData.Scan0;
                    byte* ptr = bitmapData.Stride > 0
                                    ? (byte*) pixels.ToPointer()
                                    : (byte*) pixels.ToPointer() + bitmapData.Stride*(height - 1);

                    uint stride = (uint) Math.Abs(bitmapData.Stride);
                            
                    int increment = (int) stride - width*3;
                    for (int row = 0; row < height; ++row)
                    {
                        for (int col = 0; col < width; ++col)
                        {
                            colours[(ptr[0] << 16) + (ptr[1] << 8) + ptr[2]] = true;
                            ptr += 3;
                        }
                        ptr += increment;
                    }
                    b.UnlockBits(bitmapData);

                    if (colours.Count < 2000)
                    {
                        filename = filename.Replace("jpg", "png"); //messy
                        b.Save(stream, ImageFormat.Png);
                    }
                    else
                    {
                        ImageCodecInfo jpegEncoder = GetEncoder(ImageFormat.Jpeg);
                        System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Quality;
                        EncoderParameters param = new EncoderParameters(1);
                        param.Param[0] = new EncoderParameter(myEncoder, quality == UploadQuality.High ? 96 : 80);
                        b.Save(stream, jpegEncoder, param);
                    }*/
                    break;
            }

            if (puush.config.GetValue<bool>("saveimages", false))
            {
                try
                {
                    string filepath = puush.config.GetValue<string>("saveimagepath", "") + "\\" + filename;
                    File.WriteAllBytes(filepath, image);
                }
                catch { }
            }

            if (image != null)
                FileUpload.Upload(image, filename);
        }
    }
}
