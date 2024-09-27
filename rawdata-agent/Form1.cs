using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace rawdata_agent {
    public partial class Form1 : Form {

        public Form1() {
            InitializeComponent();
        }

        private void Form1_FormClosing (object sender, FormClosingEventArgs e) {
            if (e.CloseReason == CloseReason.UserClosing) {
                e.Cancel = true;
                this.Hide();
            }
        }

        private void 열기ToolStripMenuItem_Click(object sender, EventArgs e) {
            this.Show();
        }

        private void 종료ToolStripMenuItem_Click(object sender, EventArgs e) {
            Application.Exit();
        }


        private void Form1_Deactivate(object sender, EventArgs e) {
            this.Hide();
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e) {
            this.Show();
        }
    }
}
