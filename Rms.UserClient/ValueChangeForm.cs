using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Rms.UserClient
{
    public partial class ValueChangeForm : Form
    {

        public string Value { get; set; }
        public ValueChangeForm()
        {
            InitializeComponent();
        }

        public ValueChangeForm(string oldtext)
        {
            InitializeComponent();
            Value = oldtext;
        }


        private void button_OK_Click(object sender, EventArgs e)
        {
          
            if (string.IsNullOrEmpty(textBox1.Text))
            {
                MessageBox.Show("Can not be null");
            }
            else
            {
                this.DialogResult = DialogResult.OK;
                Value = textBox1.Text;
                this.Close();
            }
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void ValueChangeForm_Shown(object sender, EventArgs e)
        {
            this.textBox1.Text = Value;
            this.textBox1.SelectAll();
        }
    }
}
