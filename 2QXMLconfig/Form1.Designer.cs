namespace _QXMLconfig
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose( bool disposing ) {
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.bot_cbo_autojoin = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.bot_txt_quit = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.bot_txt_info = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.bot_txt_username = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.bot_txt_altnick = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.bot_txt_nick = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.con_txt_irc = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.con_txt_port = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.con_txt_perform = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.mod_lbo_current = new System.Windows.Forms.ListBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.mod_btn_add = new System.Windows.Forms.Button();
            this.mod_btn_edit = new System.Windows.Forms.Button();
            this.mod_btn_delete = new System.Windows.Forms.Button();
            this.mod_txt_files = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.mod_txt_pretty = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.mod_txt_namespace = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.hiToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menu_new = new System.Windows.Forms.ToolStripMenuItem();
            this.menu_open = new System.Windows.Forms.ToolStripMenuItem();
            this.menu_save = new System.Windows.Forms.ToolStripMenuItem();
            this.menu_close = new System.Windows.Forms.ToolStripMenuItem();
            this.menu_about = new System.Windows.Forms.ToolStripMenuItem();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.label15 = new System.Windows.Forms.Label();
            this.con_txt_servername = new System.Windows.Forms.TextBox();
            this.label16 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add( this.bot_cbo_autojoin );
            this.groupBox1.Controls.Add( this.label6 );
            this.groupBox1.Controls.Add( this.bot_txt_quit );
            this.groupBox1.Controls.Add( this.label5 );
            this.groupBox1.Controls.Add( this.bot_txt_info );
            this.groupBox1.Controls.Add( this.label4 );
            this.groupBox1.Controls.Add( this.bot_txt_username );
            this.groupBox1.Controls.Add( this.label3 );
            this.groupBox1.Controls.Add( this.bot_txt_altnick );
            this.groupBox1.Controls.Add( this.label2 );
            this.groupBox1.Controls.Add( this.bot_txt_nick );
            this.groupBox1.Controls.Add( this.label1 );
            this.groupBox1.Location = new System.Drawing.Point( 12, 29 );
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size( 274, 267 );
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Bot Settings";
            // 
            // bot_cbo_autojoin
            // 
            this.bot_cbo_autojoin.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.bot_cbo_autojoin.FormattingEnabled = true;
            this.bot_cbo_autojoin.Items.AddRange( new object[] {
            "True",
            "False"} );
            this.bot_cbo_autojoin.Location = new System.Drawing.Point( 9, 233 );
            this.bot_cbo_autojoin.Name = "bot_cbo_autojoin";
            this.bot_cbo_autojoin.Size = new System.Drawing.Size( 111, 21 );
            this.bot_cbo_autojoin.TabIndex = 11;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point( 6, 217 );
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size( 114, 13 );
            this.label6.TabIndex = 10;
            this.label6.Text = "Auto Join on Invite";
            // 
            // bot_txt_quit
            // 
            this.bot_txt_quit.Location = new System.Drawing.Point( 9, 193 );
            this.bot_txt_quit.Name = "bot_txt_quit";
            this.bot_txt_quit.Size = new System.Drawing.Size( 254, 21 );
            this.bot_txt_quit.TabIndex = 9;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point( 6, 177 );
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size( 83, 13 );
            this.label5.TabIndex = 8;
            this.label5.Text = "Quit Message";
            // 
            // bot_txt_info
            // 
            this.bot_txt_info.Location = new System.Drawing.Point( 9, 153 );
            this.bot_txt_info.Name = "bot_txt_info";
            this.bot_txt_info.Size = new System.Drawing.Size( 254, 21 );
            this.bot_txt_info.TabIndex = 7;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point( 6, 137 );
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size( 74, 13 );
            this.label4.TabIndex = 6;
            this.label4.Text = "Information";
            // 
            // bot_txt_username
            // 
            this.bot_txt_username.Location = new System.Drawing.Point( 9, 113 );
            this.bot_txt_username.Name = "bot_txt_username";
            this.bot_txt_username.Size = new System.Drawing.Size( 254, 21 );
            this.bot_txt_username.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point( 6, 97 );
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size( 65, 13 );
            this.label3.TabIndex = 4;
            this.label3.Text = "Username";
            // 
            // bot_txt_altnick
            // 
            this.bot_txt_altnick.Location = new System.Drawing.Point( 9, 73 );
            this.bot_txt_altnick.Name = "bot_txt_altnick";
            this.bot_txt_altnick.Size = new System.Drawing.Size( 254, 21 );
            this.bot_txt_altnick.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point( 6, 57 );
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size( 119, 13 );
            this.label2.TabIndex = 2;
            this.label2.Text = "Alternate Nickname";
            // 
            // bot_txt_nick
            // 
            this.bot_txt_nick.Location = new System.Drawing.Point( 9, 33 );
            this.bot_txt_nick.Name = "bot_txt_nick";
            this.bot_txt_nick.Size = new System.Drawing.Size( 254, 21 );
            this.bot_txt_nick.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point( 6, 17 );
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size( 63, 13 );
            this.label1.TabIndex = 0;
            this.label1.Text = "Nickname";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point( 6, 57 );
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size( 72, 13 );
            this.label12.TabIndex = 0;
            this.label12.Text = "IRC Server";
            // 
            // con_txt_irc
            // 
            this.con_txt_irc.Location = new System.Drawing.Point( 9, 73 );
            this.con_txt_irc.Name = "con_txt_irc";
            this.con_txt_irc.Size = new System.Drawing.Size( 254, 21 );
            this.con_txt_irc.TabIndex = 1;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point( 6, 97 );
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size( 30, 13 );
            this.label11.TabIndex = 2;
            this.label11.Text = "Port";
            // 
            // con_txt_port
            // 
            this.con_txt_port.Location = new System.Drawing.Point( 9, 113 );
            this.con_txt_port.Name = "con_txt_port";
            this.con_txt_port.Size = new System.Drawing.Size( 254, 21 );
            this.con_txt_port.TabIndex = 3;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add( this.con_txt_servername );
            this.groupBox2.Controls.Add( this.label16 );
            this.groupBox2.Controls.Add( this.con_txt_perform );
            this.groupBox2.Controls.Add( this.label10 );
            this.groupBox2.Controls.Add( this.con_txt_port );
            this.groupBox2.Controls.Add( this.label11 );
            this.groupBox2.Controls.Add( this.con_txt_irc );
            this.groupBox2.Controls.Add( this.label12 );
            this.groupBox2.Location = new System.Drawing.Point( 292, 29 );
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size( 274, 267 );
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Connection Settings";
            // 
            // con_txt_perform
            // 
            this.con_txt_perform.Location = new System.Drawing.Point( 9, 153 );
            this.con_txt_perform.Multiline = true;
            this.con_txt_perform.Name = "con_txt_perform";
            this.con_txt_perform.Size = new System.Drawing.Size( 254, 101 );
            this.con_txt_perform.TabIndex = 5;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point( 6, 137 );
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size( 53, 13 );
            this.label10.TabIndex = 4;
            this.label10.Text = "Perform";
            // 
            // mod_lbo_current
            // 
            this.mod_lbo_current.FormattingEnabled = true;
            this.mod_lbo_current.Location = new System.Drawing.Point( 9, 33 );
            this.mod_lbo_current.Name = "mod_lbo_current";
            this.mod_lbo_current.Size = new System.Drawing.Size( 197, 147 );
            this.mod_lbo_current.TabIndex = 2;
            this.mod_lbo_current.SelectedIndexChanged += new System.EventHandler( this.mod_lbo_current_SelectedIndexChanged );
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add( this.mod_btn_add );
            this.groupBox3.Controls.Add( this.mod_btn_edit );
            this.groupBox3.Controls.Add( this.mod_btn_delete );
            this.groupBox3.Controls.Add( this.mod_txt_files );
            this.groupBox3.Controls.Add( this.label14 );
            this.groupBox3.Controls.Add( this.mod_txt_pretty );
            this.groupBox3.Controls.Add( this.label13 );
            this.groupBox3.Controls.Add( this.mod_txt_namespace );
            this.groupBox3.Controls.Add( this.label9 );
            this.groupBox3.Controls.Add( this.label8 );
            this.groupBox3.Controls.Add( this.label7 );
            this.groupBox3.Controls.Add( this.mod_lbo_current );
            this.groupBox3.Location = new System.Drawing.Point( 12, 302 );
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size( 554, 191 );
            this.groupBox3.TabIndex = 3;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Module Settings";
            // 
            // mod_btn_add
            // 
            this.mod_btn_add.Location = new System.Drawing.Point( 481, 156 );
            this.mod_btn_add.Name = "mod_btn_add";
            this.mod_btn_add.Size = new System.Drawing.Size( 62, 25 );
            this.mod_btn_add.TabIndex = 13;
            this.mod_btn_add.Text = "Add";
            this.mod_btn_add.UseVisualStyleBackColor = true;
            this.mod_btn_add.Click += new System.EventHandler( this.mod_btn_add_Click );
            // 
            // mod_btn_edit
            // 
            this.mod_btn_edit.Enabled = false;
            this.mod_btn_edit.Location = new System.Drawing.Point( 345, 156 );
            this.mod_btn_edit.Name = "mod_btn_edit";
            this.mod_btn_edit.Size = new System.Drawing.Size( 62, 25 );
            this.mod_btn_edit.TabIndex = 12;
            this.mod_btn_edit.Text = "Edit";
            this.mod_btn_edit.UseVisualStyleBackColor = true;
            this.mod_btn_edit.Click += new System.EventHandler( this.mod_btn_edit_Click );
            // 
            // mod_btn_delete
            // 
            this.mod_btn_delete.Enabled = false;
            this.mod_btn_delete.Location = new System.Drawing.Point( 413, 156 );
            this.mod_btn_delete.Name = "mod_btn_delete";
            this.mod_btn_delete.Size = new System.Drawing.Size( 62, 25 );
            this.mod_btn_delete.TabIndex = 11;
            this.mod_btn_delete.Text = "Delete";
            this.mod_btn_delete.UseVisualStyleBackColor = true;
            // 
            // mod_txt_files
            // 
            this.mod_txt_files.Location = new System.Drawing.Point( 225, 129 );
            this.mod_txt_files.Name = "mod_txt_files";
            this.mod_txt_files.Size = new System.Drawing.Size( 318, 21 );
            this.mod_txt_files.TabIndex = 10;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point( 222, 113 );
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size( 32, 13 );
            this.label14.TabIndex = 9;
            this.label14.Text = "Files";
            // 
            // mod_txt_pretty
            // 
            this.mod_txt_pretty.Location = new System.Drawing.Point( 225, 89 );
            this.mod_txt_pretty.Name = "mod_txt_pretty";
            this.mod_txt_pretty.Size = new System.Drawing.Size( 318, 21 );
            this.mod_txt_pretty.TabIndex = 8;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point( 222, 73 );
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size( 78, 13 );
            this.label13.TabIndex = 7;
            this.label13.Text = "Pretty Name";
            // 
            // mod_txt_namespace
            // 
            this.mod_txt_namespace.Location = new System.Drawing.Point( 225, 49 );
            this.mod_txt_namespace.Name = "mod_txt_namespace";
            this.mod_txt_namespace.Size = new System.Drawing.Size( 318, 21 );
            this.mod_txt_namespace.TabIndex = 6;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point( 222, 33 );
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size( 110, 13 );
            this.label9.TabIndex = 5;
            this.label9.Text = "NameSpace.Class";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point( 222, 17 );
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size( 72, 13 );
            this.label8.TabIndex = 4;
            this.label8.Text = "Edit Module";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point( 6, 17 );
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size( 101, 13 );
            this.label7.TabIndex = 3;
            this.label7.Text = "Current Modules";
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange( new System.Windows.Forms.ToolStripItem[] {
            this.hiToolStripMenuItem,
            this.menu_about} );
            this.menuStrip1.Location = new System.Drawing.Point( 0, 0 );
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size( 582, 24 );
            this.menuStrip1.TabIndex = 4;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // hiToolStripMenuItem
            // 
            this.hiToolStripMenuItem.DropDownItems.AddRange( new System.Windows.Forms.ToolStripItem[] {
            this.menu_new,
            this.menu_open,
            this.menu_save,
            this.menu_close} );
            this.hiToolStripMenuItem.Name = "hiToolStripMenuItem";
            this.hiToolStripMenuItem.Size = new System.Drawing.Size( 35, 20 );
            this.hiToolStripMenuItem.Text = "File";
            // 
            // menu_new
            // 
            this.menu_new.Name = "menu_new";
            this.menu_new.Size = new System.Drawing.Size( 100, 22 );
            this.menu_new.Text = "New";
            this.menu_new.Click += new System.EventHandler( this.menu_new_Click );
            // 
            // menu_open
            // 
            this.menu_open.Name = "menu_open";
            this.menu_open.Size = new System.Drawing.Size( 100, 22 );
            this.menu_open.Text = "Open";
            this.menu_open.Click += new System.EventHandler( this.menu_open_Click );
            // 
            // menu_save
            // 
            this.menu_save.Name = "menu_save";
            this.menu_save.Size = new System.Drawing.Size( 100, 22 );
            this.menu_save.Text = "Save";
            // 
            // menu_close
            // 
            this.menu_close.Name = "menu_close";
            this.menu_close.Size = new System.Drawing.Size( 100, 22 );
            this.menu_close.Text = "Close";
            // 
            // menu_about
            // 
            this.menu_about.Name = "menu_about";
            this.menu_about.Size = new System.Drawing.Size( 48, 20 );
            this.menu_about.Text = "About";
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "Project2Q Config";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point( 18, 496 );
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size( 48, 13 );
            this.label15.TabIndex = 5;
            this.label15.Text = "label15";
            // 
            // con_txt_servername
            // 
            this.con_txt_servername.Location = new System.Drawing.Point( 9, 33 );
            this.con_txt_servername.Name = "con_txt_servername";
            this.con_txt_servername.Size = new System.Drawing.Size( 254, 21 );
            this.con_txt_servername.TabIndex = 7;
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point( 6, 17 );
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size( 83, 13 );
            this.label16.TabIndex = 6;
            this.label16.Text = "Server Name";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point( 292, 535 );
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size( 38, 36 );
            this.button1.TabIndex = 6;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler( this.button1_Click );
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF( 7F, 13F );
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size( 582, 583 );
            this.Controls.Add( this.button1 );
            this.Controls.Add( this.label15 );
            this.Controls.Add( this.groupBox3 );
            this.Controls.Add( this.groupBox2 );
            this.Controls.Add( this.groupBox1 );
            this.Controls.Add( this.menuStrip1 );
            this.Font = new System.Drawing.Font( "Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( ( byte ) ( 0 ) ) );
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.Text = "Project 2Q Configuratoralator";
            this.Load += new System.EventHandler( this.Form1_Load );
            this.groupBox1.ResumeLayout( false );
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout( false );
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout( false );
            this.groupBox3.PerformLayout();
            this.menuStrip1.ResumeLayout( false );
            this.menuStrip1.PerformLayout();
            this.ResumeLayout( false );
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ComboBox bot_cbo_autojoin;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox bot_txt_quit;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox bot_txt_info;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox bot_txt_username;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox bot_txt_altnick;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox bot_txt_nick;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox con_txt_irc;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox con_txt_port;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox con_txt_perform;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.ListBox mod_lbo_current;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox mod_txt_files;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.TextBox mod_txt_pretty;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox mod_txt_namespace;
        private System.Windows.Forms.Button mod_btn_add;
        private System.Windows.Forms.Button mod_btn_edit;
        private System.Windows.Forms.Button mod_btn_delete;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem hiToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem menu_open;
        private System.Windows.Forms.ToolStripMenuItem menu_save;
        private System.Windows.Forms.ToolStripMenuItem menu_close;
        private System.Windows.Forms.ToolStripMenuItem menu_about;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.ToolStripMenuItem menu_new;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.TextBox con_txt_servername;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Button button1;
    }
}

