using Android_Cache_Viewer.Plugins;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Android_Cache_Viewer
{
    public partial class AndroidCacheViewer : Form
    {
        string selected_file = "";

        public AndroidCacheViewer()
        {
            InitializeComponent();
            //
            //openFileDlg.Filter = "YouTube|*.cache|Android Gallery3D|*.*|cache_r.0|cache_r.0|cache_bd.0|cache_bd.0";
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(openFileDlg.ShowDialog(this)==DialogResult.OK)
            {
                selected_file = openFileDlg.FileName;
                timerProcess.Start();
            }
        }

        private void timerProcess_Tick(object sender, EventArgs e)
        {
            timerProcess.Stop();
            displayPanel.Controls.Clear();
            this.Text = "Android Cache Viewer : " + Path.GetFileName(selected_file);
            CacheInterface cache = CacheIdentity.Identify(selected_file);
            displayPanel.Controls.Clear();
            displayPanel.Controls.Add(cache.showCache(selected_file));
        }


        private void supportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            displayPanel.Controls.Clear();
            Label lbl = new Label();           
            lbl.Text = "\r\nThe caches of the following apps are supported:\r\n\r\n";
            lbl.Text += "YouTube (.cache), Android Gallery3D, cache_r.0, cache_bd.0";
            lbl.Dock = DockStyle.Fill;
            displayPanel.Controls.Add(lbl); 
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            displayPanel.Controls.Clear();
            Label lbl = new Label();
            lbl.Text = "\r\nAndroid Cache Viewer is an open source software is developed by Felix Jeyareuben Chandrakumar as a part of the thesis submitted to the University of South Australia School of Computer & Information Science in fulfilment of the requirements for the degree of Master of Science (Cyber Security and Forensic Computing). The thesis was supervised by Dr Kim-Kwang Raymond Choo and Ben Martini.\r\n\r\nIcon - http://www.iconarchive.com/artist/martz90.html";
            lbl.Dock = DockStyle.Fill;
            displayPanel.Controls.Add(lbl); 
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
