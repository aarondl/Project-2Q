using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using Project2Q.Core;

namespace _QasQui {

    public partial class Project2QForm : Form 
    {

        public Project2QForm() {
            InitializeComponent();
        }

        public MemoryStream ms;
        public StreamReader sr;
        public ThreadSafeStreamWriter sw;
        public Thread p2qthread;
        public Project2QService qq;

        private void Form1_Load(object sender, EventArgs e) {

            CheckForIllegalCrossThreadCalls = false; //Proper way to do this? Probably not. But owell Q_Q

            ms = new MemoryStream( 1024 );
            sr = new StreamReader( ms );
            sw = new ThreadSafeStreamWriter( ms );

            sw.GotWrittenTo += new ThreadSafeStreamWriter.WrittenToDelegate( sw_GotWrittenTo );
            sw.AutoFlush = true;

            qq = new Project2QService( "../../../Config.xml", sr, sw );
            p2qthread = new Thread( new ParameterizedThreadStart( qq.OnStart ) );
            p2qthread.Start( null );
            icon.Visible = false;

        }

        public void sw_GotWrittenTo(string x) {
            Monitor.Enter( sw );
            sr.BaseStream.Seek( 0, SeekOrigin.Begin );

            while ( !sr.EndOfStream ) {
                this.txtMain.AppendText( sr.ReadLine() + System.Environment.NewLine );
            }

            sr.BaseStream.Seek( 0, SeekOrigin.Begin );
            Monitor.Exit( sw );
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e) {
            qq.OnStop();
            sw.Close();
            sr.Close();
            ms.Close();
        }

        private void txtInput_KeyDown(object sender, KeyEventArgs e) {
            if ( e.KeyCode == Keys.Enter ) {
                qq.SendMessage( 0, txtInput.Text );
                txtInput.Clear();
                this.ActiveControl = txtInput;
            }
        }

        private void Form_Resize(object sender, EventArgs e)
        {
            if (FormWindowState.Minimized == WindowState)
            {
                icon.Visible = true;
                Hide();
            }
        }

        private void icon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (FormWindowState.Minimized == WindowState)
            {
                Show();
                WindowState = FormWindowState.Normal;
                icon.Visible = false;
            }
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}