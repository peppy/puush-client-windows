using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Gma.UserActivityMonitor;

namespace puush
{


    public partial class ScreenSelection : Form
    {
        private const int WM_NCMOUSEMOVE = 0xa0;
        private const int WM_NCLBUTTONDOWN = 0xa1;
        private const int WM_NCLBUTTONUP = 0xa2;

        private Bitmap b;

        public ScreenSelection()
        {
            InitializeComponent();
        }
    }
}
