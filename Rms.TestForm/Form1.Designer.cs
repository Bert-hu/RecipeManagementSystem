namespace Rms.TestForm
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_recipename = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox_sendpath = new System.Windows.Forms.TextBox();
            this.button_getrecipe = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.button_setrecipe = new System.Windows.Forms.Button();
            this.richTextBox_compare = new System.Windows.Forms.RichTextBox();
            this.button4 = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage_rabbit = new System.Windows.Forms.TabPage();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.button_reloadbody = new System.Windows.Forms.Button();
            this.listBox_version = new System.Windows.Forms.ListBox();
            this.button_addversion = new System.Windows.Forms.Button();
            this.button_addrecipe = new System.Windows.Forms.Button();
            this.listBox_recipelist = new System.Windows.Forms.ListBox();
            this.button_geteppd = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.textBox_apiurl = new System.Windows.Forms.TextBox();
            this.textBox_apieqid = new System.Windows.Forms.TextBox();
            this.button_clearlog = new System.Windows.Forms.Button();
            this.button_compare = new System.Windows.Forms.Button();
            this.tabControl1.SuspendLayout();
            this.tabPage_rabbit.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(23, 410);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(0, 12);
            this.label1.TabIndex = 3;
            // 
            // textBox_recipename
            // 
            this.textBox_recipename.Location = new System.Drawing.Point(294, 387);
            this.textBox_recipename.Name = "textBox_recipename";
            this.textBox_recipename.Size = new System.Drawing.Size(188, 21);
            this.textBox_recipename.TabIndex = 2;
            this.textBox_recipename.Text = "TestRecipe";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(223, 390);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 12);
            this.label3.TabIndex = 3;
            this.label3.Text = "RecipeName";
            // 
            // textBox_sendpath
            // 
            this.textBox_sendpath.Location = new System.Drawing.Point(82, 330);
            this.textBox_sendpath.Name = "textBox_sendpath";
            this.textBox_sendpath.Size = new System.Drawing.Size(188, 21);
            this.textBox_sendpath.TabIndex = 2;
            this.textBox_sendpath.Text = "EAP.SecsClient.EQP001";
            // 
            // button_getrecipe
            // 
            this.button_getrecipe.Enabled = false;
            this.button_getrecipe.Location = new System.Drawing.Point(25, 387);
            this.button_getrecipe.Name = "button_getrecipe";
            this.button_getrecipe.Size = new System.Drawing.Size(186, 23);
            this.button_getrecipe.TabIndex = 4;
            this.button_getrecipe.Text = "Get Unfomatted Recipe";
            this.button_getrecipe.UseVisualStyleBackColor = true;
            this.button_getrecipe.Click += new System.EventHandler(this.button2_Click);
            // 
            // button1
            // 
            this.button1.Enabled = false;
            this.button1.Location = new System.Drawing.Point(25, 357);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(186, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "Get EPPD";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button_setrecipe
            // 
            this.button_setrecipe.Enabled = false;
            this.button_setrecipe.Location = new System.Drawing.Point(25, 416);
            this.button_setrecipe.Name = "button_setrecipe";
            this.button_setrecipe.Size = new System.Drawing.Size(186, 23);
            this.button_setrecipe.TabIndex = 4;
            this.button_setrecipe.Text = "Set Unfomatted Recipe";
            this.button_setrecipe.UseVisualStyleBackColor = true;
            this.button_setrecipe.Click += new System.EventHandler(this.button3_Click);
            // 
            // richTextBox_compare
            // 
            this.richTextBox_compare.Location = new System.Drawing.Point(526, 37);
            this.richTextBox_compare.Name = "richTextBox_compare";
            this.richTextBox_compare.Size = new System.Drawing.Size(710, 492);
            this.richTextBox_compare.TabIndex = 1;
            this.richTextBox_compare.Text = "";
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(25, 16);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(75, 23);
            this.button4.TabIndex = 5;
            this.button4.Text = "Init";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(17, 333);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(59, 12);
            this.label2.TabIndex = 3;
            this.label2.Text = "Send Path";
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage_rabbit);
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Location = new System.Drawing.Point(12, 15);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(508, 518);
            this.tabControl1.TabIndex = 6;
            // 
            // tabPage_rabbit
            // 
            this.tabPage_rabbit.Controls.Add(this.button4);
            this.tabPage_rabbit.Controls.Add(this.button1);
            this.tabPage_rabbit.Controls.Add(this.button_setrecipe);
            this.tabPage_rabbit.Controls.Add(this.button_getrecipe);
            this.tabPage_rabbit.Controls.Add(this.textBox_sendpath);
            this.tabPage_rabbit.Controls.Add(this.label2);
            this.tabPage_rabbit.Controls.Add(this.textBox_recipename);
            this.tabPage_rabbit.Controls.Add(this.label3);
            this.tabPage_rabbit.Controls.Add(this.label1);
            this.tabPage_rabbit.Location = new System.Drawing.Point(4, 22);
            this.tabPage_rabbit.Name = "tabPage_rabbit";
            this.tabPage_rabbit.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_rabbit.Size = new System.Drawing.Size(500, 492);
            this.tabPage_rabbit.TabIndex = 1;
            this.tabPage_rabbit.Text = "RabbitMq";
            this.tabPage_rabbit.UseVisualStyleBackColor = true;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.button_compare);
            this.tabPage1.Controls.Add(this.button_reloadbody);
            this.tabPage1.Controls.Add(this.listBox_version);
            this.tabPage1.Controls.Add(this.button_addversion);
            this.tabPage1.Controls.Add(this.button_addrecipe);
            this.tabPage1.Controls.Add(this.listBox_recipelist);
            this.tabPage1.Controls.Add(this.button_geteppd);
            this.tabPage1.Controls.Add(this.label5);
            this.tabPage1.Controls.Add(this.label4);
            this.tabPage1.Controls.Add(this.textBox_apiurl);
            this.tabPage1.Controls.Add(this.textBox_apieqid);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(500, 492);
            this.tabPage1.TabIndex = 2;
            this.tabPage1.Text = "WebApi";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // button_reloadbody
            // 
            this.button_reloadbody.Location = new System.Drawing.Point(354, 137);
            this.button_reloadbody.Name = "button_reloadbody";
            this.button_reloadbody.Size = new System.Drawing.Size(140, 23);
            this.button_reloadbody.TabIndex = 6;
            this.button_reloadbody.Text = "Reload Body";
            this.button_reloadbody.UseVisualStyleBackColor = true;
            this.button_reloadbody.Click += new System.EventHandler(this.button_reloadbody_Click);
            // 
            // listBox_version
            // 
            this.listBox_version.FormattingEnabled = true;
            this.listBox_version.ItemHeight = 12;
            this.listBox_version.Location = new System.Drawing.Point(194, 116);
            this.listBox_version.Name = "listBox_version";
            this.listBox_version.Size = new System.Drawing.Size(140, 352);
            this.listBox_version.TabIndex = 5;
            // 
            // button_addversion
            // 
            this.button_addversion.Location = new System.Drawing.Point(354, 107);
            this.button_addversion.Name = "button_addversion";
            this.button_addversion.Size = new System.Drawing.Size(143, 23);
            this.button_addversion.TabIndex = 4;
            this.button_addversion.Text = "Add New Version";
            this.button_addversion.UseVisualStyleBackColor = true;
            this.button_addversion.Click += new System.EventHandler(this.button_addversion_Click);
            // 
            // button_addrecipe
            // 
            this.button_addrecipe.Location = new System.Drawing.Point(354, 78);
            this.button_addrecipe.Name = "button_addrecipe";
            this.button_addrecipe.Size = new System.Drawing.Size(143, 23);
            this.button_addrecipe.TabIndex = 4;
            this.button_addrecipe.Text = "Add New Recipe";
            this.button_addrecipe.UseVisualStyleBackColor = true;
            this.button_addrecipe.Click += new System.EventHandler(this.button_addrecipe_Click);
            // 
            // listBox_recipelist
            // 
            this.listBox_recipelist.FormattingEnabled = true;
            this.listBox_recipelist.ItemHeight = 12;
            this.listBox_recipelist.Location = new System.Drawing.Point(27, 116);
            this.listBox_recipelist.Name = "listBox_recipelist";
            this.listBox_recipelist.Size = new System.Drawing.Size(137, 352);
            this.listBox_recipelist.TabIndex = 3;
            this.listBox_recipelist.SelectedValueChanged += new System.EventHandler(this.listBox_recipelist_SelectedValueChanged);
            // 
            // button_geteppd
            // 
            this.button_geteppd.Location = new System.Drawing.Point(27, 78);
            this.button_geteppd.Name = "button_geteppd";
            this.button_geteppd.Size = new System.Drawing.Size(137, 23);
            this.button_geteppd.TabIndex = 2;
            this.button_geteppd.Text = "Get EPPD";
            this.button_geteppd.UseVisualStyleBackColor = true;
            this.button_geteppd.Click += new System.EventHandler(this.button_geteppd_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(25, 52);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(47, 12);
            this.label5.TabIndex = 1;
            this.label5.Text = "Api Url";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(25, 25);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(29, 12);
            this.label4.TabIndex = 1;
            this.label4.Text = "EQID";
            // 
            // textBox_apiurl
            // 
            this.textBox_apiurl.Location = new System.Drawing.Point(90, 49);
            this.textBox_apiurl.Name = "textBox_apiurl";
            this.textBox_apiurl.Size = new System.Drawing.Size(199, 21);
            this.textBox_apiurl.TabIndex = 0;
            this.textBox_apiurl.Text = "http://192.168.53.210:8085";
            // 
            // textBox_apieqid
            // 
            this.textBox_apieqid.Location = new System.Drawing.Point(90, 22);
            this.textBox_apieqid.Name = "textBox_apieqid";
            this.textBox_apieqid.Size = new System.Drawing.Size(199, 21);
            this.textBox_apieqid.TabIndex = 0;
            this.textBox_apieqid.Text = "EQP002";
            // 
            // button_clearlog
            // 
            this.button_clearlog.Location = new System.Drawing.Point(1161, 8);
            this.button_clearlog.Name = "button_clearlog";
            this.button_clearlog.Size = new System.Drawing.Size(75, 23);
            this.button_clearlog.TabIndex = 7;
            this.button_clearlog.Text = "Clear";
            this.button_clearlog.UseVisualStyleBackColor = true;
            this.button_clearlog.Click += new System.EventHandler(this.button_clearlog_Click);
            // 
            // button_compare
            // 
            this.button_compare.Location = new System.Drawing.Point(354, 166);
            this.button_compare.Name = "button_compare";
            this.button_compare.Size = new System.Drawing.Size(140, 23);
            this.button_compare.TabIndex = 7;
            this.button_compare.Text = "Compare Body";
            this.button_compare.UseVisualStyleBackColor = true;
            this.button_compare.Click += new System.EventHandler(this.button2_Click_1);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1248, 545);
            this.Controls.Add(this.button_clearlog);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.richTextBox_compare);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.Shown += new System.EventHandler(this.Form1_Shown);
            this.tabControl1.ResumeLayout(false);
            this.tabPage_rabbit.ResumeLayout(false);
            this.tabPage_rabbit.PerformLayout();
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.RichTextBox richTextBox_compare;
        private System.Windows.Forms.Button button_setrecipe;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button_getrecipe;
        private System.Windows.Forms.TextBox textBox_sendpath;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox_recipename;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage_rabbit;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.ListBox listBox_recipelist;
        private System.Windows.Forms.Button button_geteppd;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBox_apieqid;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBox_apiurl;
        private System.Windows.Forms.Button button_clearlog;
        private System.Windows.Forms.Button button_addrecipe;
        private System.Windows.Forms.Button button_addversion;
        private System.Windows.Forms.ListBox listBox_version;
        private System.Windows.Forms.Button button_reloadbody;
        private System.Windows.Forms.Button button_compare;
    }
}

