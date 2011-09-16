namespace _QasQui {
    partial class Project2QForm {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if ( disposing && ( components != null ) ) {
                components.Dispose();
            }

            base.Dispose( disposing );
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager( typeof( Project2QForm ) );
            this.txtMain = new System.Windows.Forms.TextBox();
            this.txtInput = new System.Windows.Forms.TextBox();
            this.icon = new System.Windows.Forms.NotifyIcon( this.components );
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip( this.components );
            this.closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtMain
            // 
            this.txtMain.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.txtMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtMain.Font = new System.Drawing.Font( "Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ) );
            this.txtMain.Location = new System.Drawing.Point( 0, 0 );
            this.txtMain.MaxLength = 5000;
            this.txtMain.Multiline = true;
            this.txtMain.Name = "txtMain";
            this.txtMain.ReadOnly = true;
            this.txtMain.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtMain.Size = new System.Drawing.Size( 558, 547 );
            this.txtMain.TabIndex = 1;
            // 
            // txtInput
            // 
            this.txtInput.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.txtInput.Font = new System.Drawing.Font( "Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ) );
            this.txtInput.Location = new System.Drawing.Point( 0, 547 );
            this.txtInput.Name = "txtInput";
            this.txtInput.Size = new System.Drawing.Size( 558, 21 );
            this.txtInput.TabIndex = 0;
            this.txtInput.KeyDown += new System.Windows.Forms.KeyEventHandler( this.txtInput_KeyDown );
            // 
            // icon
            // 
            this.icon.ContextMenuStrip = this.contextMenuStrip1;
            this.icon.Icon = ( (System.Drawing.Icon)( resources.GetObject( "icon.Icon" ) ) );
            this.icon.Text = "Project Q2";
            this.icon.Visible = true;
            this.icon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler( this.icon_MouseDoubleClick );
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange( new System.Windows.Forms.ToolStripItem[] {
            this.closeToolStripMenuItem} );
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.ShowImageMargin = false;
            this.contextMenuStrip1.Size = new System.Drawing.Size( 87, 26 );
            // 
            // closeToolStripMenuItem
            // 
            this.closeToolStripMenuItem.Name = "closeToolStripMenuItem";
            this.closeToolStripMenuItem.Size = new System.Drawing.Size( 86, 22 );
            this.closeToolStripMenuItem.Text = "Close";
            this.closeToolStripMenuItem.Click += new System.EventHandler( this.closeToolStripMenuItem_Click );
            // 
            // Project2QForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF( 6F, 13F );
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size( 558, 568 );
            this.ContextMenuStrip = this.contextMenuStrip1;
            this.Controls.Add( this.txtMain );
            this.Controls.Add( this.txtInput );
            this.Icon = ( (System.Drawing.Icon)( resources.GetObject( "$this.Icon" ) ) );
            this.Name = "Project2QForm";
            this.Text = "Project 2Q";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler( this.Form1_FormClosed );
            this.Resize += new System.EventHandler( this.Form_Resize );
            this.Load += new System.EventHandler( this.Form1_Load );
            this.contextMenuStrip1.ResumeLayout( false );
            this.ResumeLayout( false );
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtMain;
        private System.Windows.Forms.TextBox txtInput;
        private System.Windows.Forms.NotifyIcon icon;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem closeToolStripMenuItem;
    }
}

