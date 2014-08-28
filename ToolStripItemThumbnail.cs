using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace puush
{
    class ToolStripItemThumbnail : ToolStripItem
    {
        Image thumbnail;

        public ToolStripItemThumbnail(Image thumbnail, int width) : base()
        {
            this.thumbnail = thumbnail;
            AutoSize = false;
            Width = 170;
            Height = thumbnail.Height;
        }

        protected override void Dispose(bool disposing)
        {
            if (thumbnail != null)
            {
                thumbnail.Dispose();
                thumbnail = null;
            }

            base.Dispose(disposing);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.DrawImage(thumbnail, new Point((Width - thumbnail.Width) / 2,0));
            base.OnPaint(e);
        }
    }
}
