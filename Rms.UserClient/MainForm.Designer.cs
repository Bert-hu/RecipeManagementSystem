using Rms.UserClient.Model;
using Sunny.UI;

namespace Rms.UserClient
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.groupBox3 = new Sunny.UI.UIGroupBox();
            this.richTextBox1 = new Sunny.UI.UIRichTextBox();
            this.uiTabControl1 = new Sunny.UI.UITabControl();
            this.tabPage_WaferGrinding = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.uiSymbolButton_portA = new Sunny.UI.UISymbolButton();
            this.uiSymbolButton1 = new Sunny.UI.UISymbolButton();
            this.groupBox3.SuspendLayout();
            this.uiTabControl1.SuspendLayout();
            this.tabPage_WaferGrinding.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.richTextBox1);
            this.groupBox3.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBox3.Location = new System.Drawing.Point(20, 389);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBox3.MinimumSize = new System.Drawing.Size(1, 1);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(0, 32, 0, 0);
            this.groupBox3.Size = new System.Drawing.Size(804, 185);
            this.groupBox3.Style = Sunny.UI.UIStyle.Custom;
            this.groupBox3.TabIndex = 6;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = null;
            this.groupBox3.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // richTextBox1
            // 
            this.richTextBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBox1.FillColor = System.Drawing.Color.White;
            this.richTextBox1.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.richTextBox1.Location = new System.Drawing.Point(0, 32);
            this.richTextBox1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.richTextBox1.MinimumSize = new System.Drawing.Size(1, 1);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Padding = new System.Windows.Forms.Padding(2);
            this.richTextBox1.ReadOnly = true;
            this.richTextBox1.ShowText = false;
            this.richTextBox1.Size = new System.Drawing.Size(804, 153);
            this.richTextBox1.Style = Sunny.UI.UIStyle.Custom;
            this.richTextBox1.TabIndex = 0;
            this.richTextBox1.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // uiTabControl1
            // 
            this.uiTabControl1.Controls.Add(this.tabPage_WaferGrinding);
            this.uiTabControl1.Controls.Add(this.tabPage2);
            this.uiTabControl1.DrawMode = System.Windows.Forms.TabDrawMode.OwnerDrawFixed;
            this.uiTabControl1.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiTabControl1.ItemSize = new System.Drawing.Size(150, 25);
            this.uiTabControl1.Location = new System.Drawing.Point(20, 50);
            this.uiTabControl1.MainPage = "";
            this.uiTabControl1.MenuStyle = Sunny.UI.UIMenuStyle.Custom;
            this.uiTabControl1.Name = "uiTabControl1";
            this.uiTabControl1.SelectedIndex = 0;
            this.uiTabControl1.Size = new System.Drawing.Size(801, 342);
            this.uiTabControl1.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.uiTabControl1.Style = Sunny.UI.UIStyle.Custom;
            this.uiTabControl1.TabBackColor = System.Drawing.Color.DarkGray;
            this.uiTabControl1.TabIndex = 7;
            this.uiTabControl1.TipsFont = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            // 
            // tabPage_WaferGrinding
            // 
            this.tabPage_WaferGrinding.BackColor = System.Drawing.Color.White;
            this.tabPage_WaferGrinding.Controls.Add(this.uiSymbolButton1);
            this.tabPage_WaferGrinding.Controls.Add(this.uiSymbolButton_portA);
            this.tabPage_WaferGrinding.Location = new System.Drawing.Point(0, 25);
            this.tabPage_WaferGrinding.Name = "tabPage_WaferGrinding";
            this.tabPage_WaferGrinding.Size = new System.Drawing.Size(801, 317);
            this.tabPage_WaferGrinding.TabIndex = 0;
            this.tabPage_WaferGrinding.Text = "Wafer Grinding";
            // 
            // tabPage2
            // 
            this.tabPage2.BackColor = System.Drawing.Color.White;
            this.tabPage2.Location = new System.Drawing.Point(0, 25);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Size = new System.Drawing.Size(801, 317);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Laser Grooving";
            // 
            // uiSymbolButton_portA
            // 
            this.uiSymbolButton_portA.Cursor = System.Windows.Forms.Cursors.Hand;
            this.uiSymbolButton_portA.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiSymbolButton_portA.Location = new System.Drawing.Point(58, 234);
            this.uiSymbolButton_portA.MinimumSize = new System.Drawing.Size(1, 1);
            this.uiSymbolButton_portA.Name = "uiSymbolButton_portA";
            this.uiSymbolButton_portA.Size = new System.Drawing.Size(100, 35);
            this.uiSymbolButton_portA.Symbol = 61584;
            this.uiSymbolButton_portA.TabIndex = 0;
            this.uiSymbolButton_portA.Text = "PortA";
            this.uiSymbolButton_portA.TipsFont = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            // 
            // uiSymbolButton1
            // 
            this.uiSymbolButton1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.uiSymbolButton1.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiSymbolButton1.Location = new System.Drawing.Point(239, 234);
            this.uiSymbolButton1.MinimumSize = new System.Drawing.Size(1, 1);
            this.uiSymbolButton1.Name = "uiSymbolButton1";
            this.uiSymbolButton1.Size = new System.Drawing.Size(100, 35);
            this.uiSymbolButton1.Symbol = 61584;
            this.uiSymbolButton1.TabIndex = 0;
            this.uiSymbolButton1.Text = "PortB";
            this.uiSymbolButton1.TipsFont = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            // 
            // MainForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(851, 597);
            this.Controls.Add(this.uiTabControl1);
            this.Controls.Add(this.groupBox3);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(160)))), ((int)(((byte)(200)))));
            this.Style = Sunny.UI.UIStyle.Custom;
            this.Text = "EAP GUI";
            this.ZoomScaleRect = new System.Drawing.Rectangle(15, 15, 685, 472);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.Shown += new System.EventHandler(this.MainForm_Shown);
            this.groupBox3.ResumeLayout(false);
            this.uiTabControl1.ResumeLayout(false);
            this.tabPage_WaferGrinding.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private UIGroupBox groupBox3;
        private UIRichTextBox richTextBox1;
        private UITabControl uiTabControl1;
        private System.Windows.Forms.TabPage tabPage_WaferGrinding;
        private System.Windows.Forms.TabPage tabPage2;
        private UISymbolButton uiSymbolButton_portA;
        private UISymbolButton uiSymbolButton1;
    }
}

