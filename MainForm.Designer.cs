using puush.Properties;
namespace puush
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItemVersion = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem5 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItemWindow = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemDesktop = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemSelection = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemUploadClipboard = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemUploadFile = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItemUploadDisabled = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItemCancelUpload = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.trayIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.trayResetTimer = new System.Windows.Forms.Timer(this.components);
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemVersion,
            this.toolStripMenuItem5,
            this.toolStripSeparator4,
            this.toolStripMenuItem3,
            this.toolStripSeparator3,
            this.toolStripMenuItemWindow,
            this.toolStripMenuItemDesktop,
            this.toolStripMenuItemSelection,
            this.toolStripMenuItemUploadClipboard,
            this.toolStripMenuItemUploadFile,
            this.toolStripSeparator2,
            this.toolStripMenuItemUploadDisabled,
            this.toolStripMenuItem2,
            this.toolStripSeparator1,
            this.toolStripMenuItemCancelUpload,
            this.toolStripMenuItem1});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(196, 292);
            // 
            // toolStripMenuItemVersion
            // 
            this.toolStripMenuItemVersion.Enabled = false;
            this.toolStripMenuItemVersion.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.toolStripMenuItemVersion.Name = "toolStripMenuItemVersion";
            this.toolStripMenuItemVersion.Size = new System.Drawing.Size(195, 22);
            this.toolStripMenuItemVersion.Text = "puush! v0.00";
            // 
            // toolStripMenuItem5
            // 
            this.toolStripMenuItem5.Name = "toolStripMenuItem5";
            this.toolStripMenuItem5.Size = new System.Drawing.Size(195, 22);
            this.toolStripMenuItem5.Text = "My Account";
            this.toolStripMenuItem5.Click += new System.EventHandler(this.toolStripMenuItem5_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(192, 6);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Enabled = false;
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(195, 22);
            this.toolStripMenuItem3.Text = "Recent Uploads";
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(192, 6);
            // 
            // toolStripMenuItemWindow
            // 
            this.toolStripMenuItemWindow.Image = global::puush.Properties.Resources.icon_window;
            this.toolStripMenuItemWindow.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripMenuItemWindow.Name = "toolStripMenuItemWindow";
            this.toolStripMenuItemWindow.Size = new System.Drawing.Size(195, 22);
            this.toolStripMenuItemWindow.Text = "Capture Current Window";
            this.toolStripMenuItemWindow.Click += new System.EventHandler(this.toolStripMenuItemWindow_Click);
            // 
            // toolStripMenuItemDesktop
            // 
            this.toolStripMenuItemDesktop.Image = global::puush.Properties.Resources.icon_fullscreen;
            this.toolStripMenuItemDesktop.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripMenuItemDesktop.Name = "toolStripMenuItemDesktop";
            this.toolStripMenuItemDesktop.Size = new System.Drawing.Size(195, 22);
            this.toolStripMenuItemDesktop.Text = "Capture Desktop";
            this.toolStripMenuItemDesktop.Click += new System.EventHandler(this.toolStripMenuItemDesktop_Click);
            // 
            // toolStripMenuItemSelection
            // 
            this.toolStripMenuItemSelection.Image = global::puush.Properties.Resources.icon_selection;
            this.toolStripMenuItemSelection.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripMenuItemSelection.Name = "toolStripMenuItemSelection";
            this.toolStripMenuItemSelection.Size = new System.Drawing.Size(195, 22);
            this.toolStripMenuItemSelection.Text = "Capture Area...";
            this.toolStripMenuItemSelection.Click += new System.EventHandler(this.toolStripMenuItemSelection_Click);
            // 
            // toolStripMenuItemUploadClipboard
            // 
            this.toolStripMenuItemUploadClipboard.Name = "toolStripMenuItemUploadClipboard";
            this.toolStripMenuItemUploadClipboard.Size = new System.Drawing.Size(195, 22);
            this.toolStripMenuItemUploadClipboard.Text = "Upload Clipboard";
            this.toolStripMenuItemUploadClipboard.Click += new System.EventHandler(this.toolStripMenuItemUploadClipboard_Click);
            // 
            // toolStripMenuItemUploadFile
            // 
            this.toolStripMenuItemUploadFile.Image = global::puush.Properties.Resources.icon_upload;
            this.toolStripMenuItemUploadFile.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripMenuItemUploadFile.Name = "toolStripMenuItemUploadFile";
            this.toolStripMenuItemUploadFile.Size = new System.Drawing.Size(195, 22);
            this.toolStripMenuItemUploadFile.Text = "Upload File...";
            this.toolStripMenuItemUploadFile.Click += new System.EventHandler(this.toolStripMenuItemUploadFile_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(192, 6);
            // 
            // toolStripMenuItemUploadDisabled
            // 
            this.toolStripMenuItemUploadDisabled.Name = "toolStripMenuItemUploadDisabled";
            this.toolStripMenuItemUploadDisabled.Size = new System.Drawing.Size(195, 22);
            this.toolStripMenuItemUploadDisabled.Text = "Disable puushing";
            this.toolStripMenuItemUploadDisabled.Click += new System.EventHandler(this.toolStripMenuItem3_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(195, 22);
            this.toolStripMenuItem2.Text = "Settings...";
            this.toolStripMenuItem2.Click += new System.EventHandler(this.toolStripMenuItem2_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(192, 6);
            // 
            // toolStripMenuItemCancelUpload
            // 
            this.toolStripMenuItemCancelUpload.Name = "toolStripMenuItemCancelUpload";
            this.toolStripMenuItemCancelUpload.Size = new System.Drawing.Size(195, 22);
            this.toolStripMenuItemCancelUpload.Text = "Cancel Upload";
            this.toolStripMenuItemCancelUpload.Visible = false;
            this.toolStripMenuItemCancelUpload.Click += new System.EventHandler(this.toolStripMenuItemCancelUpload_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(195, 22);
            this.toolStripMenuItem1.Text = "Exit";
            this.toolStripMenuItem1.Click += new System.EventHandler(this.toolStripMenuItem1_Click);
            // 
            // trayIcon
            // 
            this.trayIcon.ContextMenuStrip = this.contextMenuStrip1;
            this.trayIcon.Icon = global::puush.Properties.Resources.tray;
            this.trayIcon.Text = "puush";
            this.trayIcon.Visible = true;
            this.trayIcon.MouseClick += new System.Windows.Forms.MouseEventHandler(this.trayIcon_MouseClick);
            this.trayIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.trayIcon_MouseDoubleClick);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.Multiselect = true;
            this.openFileDialog1.Title = "Upload File";
            // 
            // trayResetTimer
            // 
            this.trayResetTimer.Interval = 2000;
            this.trayResetTimer.Tick += new System.EventHandler(this.trayResetTimer_Tick);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(0, 0);
            this.ControlBox = false;
            this.Enabled = false;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Location = new System.Drawing.Point(-10000, -10000);
            this.Name = "MainForm";
            this.Opacity = 0D;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "puush";
            this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        internal System.Windows.Forms.NotifyIcon trayIcon;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemVersion;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem5;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        internal System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSelection;
        internal System.Windows.Forms.ToolStripMenuItem toolStripMenuItemDesktop;
        internal System.Windows.Forms.ToolStripMenuItem toolStripMenuItemUploadFile;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Timer trayResetTimer;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem3;
        internal System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        internal System.Windows.Forms.ToolStripMenuItem toolStripMenuItemCancelUpload;
        internal System.Windows.Forms.ToolStripMenuItem toolStripMenuItemWindow;
        internal System.Windows.Forms.ToolStripMenuItem toolStripMenuItemUploadDisabled;
        internal System.Windows.Forms.ToolStripMenuItem toolStripMenuItemUploadClipboard;
    }
}

