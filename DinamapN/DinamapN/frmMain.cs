using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace DinamapN
{
    public partial class frmMain : Form
    {
        private string visitID;

        public frmMain()
        {
            InitializeComponent();
        }

        public frmMain(string visit)
        {
            InitializeComponent();
            visitID = visit;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void scanMeasurementToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmMeasurement newMDIChild = new frmMeasurement();
            newMDIChild.MdiParent = this;
            newMDIChild.WindowState = FormWindowState.Maximized;
            newMDIChild.Show();
        }

        private void generalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmOptions newMDIChild = new frmOptions();
            newMDIChild.MdiParent = this;
            newMDIChild.WindowState = FormWindowState.Maximized;
            newMDIChild.Show();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmAbout newMDIChild = new frmAbout();
            newMDIChild.MdiParent = this;
            newMDIChild.WindowState = FormWindowState.Maximized;
            newMDIChild.Show();  
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
                frmMeasurement newMDIChild = new frmMeasurement(visitID);

                scanMeasurementToolStripMenuItem.Enabled = true;
                newMDIChild.MdiParent = this;
                newMDIChild.WindowState = FormWindowState.Maximized;
                newMDIChild.Show();
         }

        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmHelp newMDIChild = new frmHelp();
            newMDIChild.MdiParent = this;
            newMDIChild.WindowState = FormWindowState.Maximized;
            newMDIChild.Show();
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to exit?", "Confirm exit", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                e.Cancel = true;
        }

        private void frmMain_FormClosed(object sender, FormClosedEventArgs e)
        {
                Application.Exit();
        }
    }
}