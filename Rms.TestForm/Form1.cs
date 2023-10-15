using Newtonsoft.Json;
using Rms.Models.DataBase.Rms;
using Rms.Models.RabbitMq;
using Rms.Models.WebApi;
using Rms.Utils;
using RMS.Domain.Rms;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace Rms.TestForm
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var trans = new RabbitMqTransaction
            {
                TransactionName = "GetEPPD",
                EquipmentID = "EQTEST01",
                NeedReply = true,
                ReplyChannel = this.textBox_listenpath.Text,
                Parameters = new Dictionary<string, object>() { { "key1", "value1" } }
            };
            RichTextBoxAddText("Send RMQ");
            RichTextBoxAddText(trans);
            var rep = RabbitMqService.ProduceWaitReply(this.textBox_sendpath.Text, trans, 5);
            RichTextBoxAddText("Receive RMQ");
            RichTextBoxAddText(rep);
        }
        class infotech_igbt
        {
            public string uuid { get; set; }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            var db = new SqlSugarClient(new ConnectionConfig()
            {
                ConnectionString = "PORT=9999;DATABASE=sfis;HOST=10.0.4.220;PASSWORD=hiuser123;USER ID=hitachi", //必填
                DbType = SqlSugar.DbType.PostgreSQL, //必填
                IsAutoCloseConnection = true, //默认false
                InitKeyType = InitKeyType.Attribute,

            }); //默认SystemTable
                //var db = DbFactory.GetSqlSugarClient();
                //db.DbFirst.CreateClassFile("D:\\test\\1111", "Models");
                //var aa = db.Queryable<infotech_igbt>().ToList();
        }

        private void Form1_Shown(object sender, EventArgs e)
        {

            var db = DbFactory.GetSqlSugarClient();
            //初始化EQ combo box
            var eqps = db.Queryable<RMS_EQUIPMENT>().OrderBy(it => it.ORDERSORT).ToList();
            var showlist = eqps.ToDictionary(it => it.ID, it => it.ID);
            comboBox_eqpselect.DisplayMember = "Key";
            comboBox_eqpselect.ValueMember = "Value";
            comboBox_eqpselect.DataSource = new BindingSource(showlist, null);
            comboBox_urlselect.SelectedIndex = 0;
            //初始化Marking eq 机种 combobox
            var sql = @"select eq.ID EQID,eq.NAME EQNAME,eq.FLOW_ID FLOW_ID,r.ID RECIPEID,r.NAME RECIPENAME,
r.MARKING_LATEST_ID MARKING_LATEST_ID,r.MARKING_EFFECTIVE_ID MARKING_EFFECTIVE_ID,
mv.VERSION LATEST_VERSION,
mv2.VERSION EFFECTIVE_VERSION
 from RMS_EQUIPMENT eq
inner join RMS_RECIPE r  on eq.ID = r.EQUIPMENT_ID 
left join RMS_MARKING_VERSION mv  on mv.ID = r.MARKING_LATEST_ID
left join RMS_MARKING_VERSION mv2  on mv2.ID = r.MARKING_EFFECTIVE_ID
where eq.TYPE = 'WaferMarking'";
            var recipes = db.SqlQueryable<MarkingRecipe>(sql).ToList();
            var showrecipes = recipes.Select(it => it.EQID + "," + it.RECIPENAME).ToList();
            comboBox_markingrecipes.DataSource = showrecipes;
            comboBox_markingrecipes.SelectedIndex = 0;
        }

        private void RichTextBoxAddText(object text)
        {
            this.Invoke(new Action(() =>
            {
                richTextBox_compare.Text += DateTime.Now.ToString() + " " + JsonConvert.SerializeObject(text, Formatting.Indented) + "\n";
            }));
        }
        byte[] temp;
        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                var trans = new RabbitMqTransaction
                {
                    TransactionName = "GetUnfomattedRecipe",
                    EquipmentID = "EQTEST01",
                    NeedReply = true,
                    ReplyChannel = this.textBox_listenpath.Text,
                    Parameters = new Dictionary<string, object>() { { "RecipeName", this.textBox_recipename.Text } }
                };
                RichTextBoxAddText("Send RMQ");
                RichTextBoxAddText(trans);
                var rep = RabbitMqService.ProduceWaitReply(this.textBox_sendpath.Text, trans, 5);
                RichTextBoxAddText("Receive RMQ");
                RichTextBoxAddText(rep);
                if (rep.Parameters["ContentType"].ToString() == "secsByte")
                {
                    temp = Convert.FromBase64String(rep.Parameters["RecipeBody"].ToString());
                }
            }
            catch
            {

            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var body = Convert.ToBase64String(temp);
            var trans = new RabbitMqTransaction
            {
                TransactionName = "SetUnfomattedRecipe",
                EquipmentID = "EQTEST01",
                NeedReply = true,
                ReplyChannel = this.textBox_listenpath.Text,
                Parameters = new Dictionary<string, object>() { { "RecipeName", this.textBox_recipename.Text }, { "RecipeBody", body } }
            };
            RichTextBoxAddText("Send RMQ");
            RichTextBoxAddText(trans);
            var rep = RabbitMqService.ProduceWaitReply(this.textBox_sendpath.Text, trans, 5);
            RichTextBoxAddText("Receive RMQ");
            RichTextBoxAddText(rep);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            RabbitMqService.Initiate("192.168.53.174", "admin", "admin", 5672);
            //   RabbitMqService.BeginConsume("EAP.SecsClient.EQTEST01");
            RabbitMqService.BeginConsume(this.textBox_listenpath.Text);
            this.button4.Enabled = false;
            this.button1.Enabled = true;
            this.button_getrecipe.Enabled = true;
            this.button_setrecipe.Enabled = true;
            this.button_geteqpstatus.Enabled = true;
            this.textBox_listenpath.Enabled = false;
        }

        private void button_geteqpstatus_Click(object sender, EventArgs e)
        {
            try
            {
                var trans = new RabbitMqTransaction
                {
                    TransactionName = "GetEquipmentStatus",
                    EquipmentID = "EQTEST01",
                    NeedReply = true,
                    ReplyChannel = this.textBox_listenpath.Text,
                };
                RichTextBoxAddText("Send RMQ");
                RichTextBoxAddText(trans);
                var rep = RabbitMqService.ProduceWaitReply(this.textBox_sendpath.Text, trans, 5);
                RichTextBoxAddText("Receive RMQ");
                RichTextBoxAddText(rep);

            }
            catch (Exception)
            {


            }
        }

        private void button_geteppd_Click(object sender, EventArgs e)
        {
            try
            {
                string url = $"{comboBox_urlselect.SelectedItem.ToString()}" + "/api/geteppd";
                var body = JsonConvert.SerializeObject(new GetEppdRequest { EuipmentId = this.comboBox_eqpselect.SelectedValue.ToString() });
                RichTextBoxAddText("Send Web Api");
                RichTextBoxAddText(body);
                var apiresult = HTTPClientHelper.HttpPostRequestAsync4Json(url, body);
                RichTextBoxAddText("Get api result");
                RichTextBoxAddText(apiresult);
                if (apiresult != null)
                {
                    var rep = JsonConvert.DeserializeObject<GetEppdResponse>(apiresult);
                    List<string> recipelist = rep.EPPD;
                    RichTextBoxAddText("Update List Box");
                    this.listBox_recipelist.DataSource = recipelist;
                }

            }
            catch (Exception)
            {


            }
        }

        private void button_clearlog_Click(object sender, EventArgs e)
        {
            this.Invoke(new Action(() =>
            {
                richTextBox_compare.Text = "";
            }));
        }

        private void button_addrecipe_Click(object sender, EventArgs e)
        {
            string url = $"{comboBox_urlselect.SelectedItem.ToString()}" + "/api/addnewrecipe";
            var body = JsonConvert.SerializeObject(new AddNewRecipeRequest { EquipmentId = this.comboBox_eqpselect.SelectedValue.ToString(), RecipeName = this.listBox_recipelist.SelectedValue.ToString() });
            RichTextBoxAddText("Send Web Api");
            RichTextBoxAddText(body);
            var apiresult = HTTPClientHelper.HttpPostRequestAsync4Json(url, body);
            RichTextBoxAddText("Get api result");
            RichTextBoxAddText(apiresult);
            listBox_recipelist_SelectedValueChanged(null, null);
        }

        private void listBox_recipelist_SelectedValueChanged(object sender, EventArgs e)
        {
            try
            {
                var db = DbFactory.GetSqlSugarClient();
                var recipe = db.Queryable<RMS_RECIPE>().Where(it => it.EQUIPMENT_ID == this.comboBox_eqpselect.SelectedValue.ToString() && it.NAME == listBox_recipelist.SelectedValue.ToString())?.First();
                if (recipe == null)
                {
                    listBox_version.DataSource = null;
                    return;
                }
                var versions = db.Queryable<RMS_RECIPE_VERSION>().Where(it => it.RECIPE_ID == recipe.ID).OrderBy(it => it.VERSION, SqlSugar.OrderByType.Desc).ToList();
                var showlist = versions.ToDictionary(it => it.VERSION, it => it.ID);
                this.Invoke(new Action(() =>
                {
                    listBox_version.DisplayMember = "Key";
                    listBox_version.ValueMember = "Value";
                    listBox_version.DataSource = new BindingSource(showlist, null);
                }));


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button_addversion_Click(object sender, EventArgs e)
        {
            string url = $"{comboBox_urlselect.SelectedItem.ToString()}" + "/api/addnewrecipeversion";
            var db = DbFactory.GetSqlSugarClient();
            var recipe = db.Queryable<RMS_RECIPE>().Where(it => it.EQUIPMENT_ID == this.comboBox_eqpselect.SelectedValue.ToString() && it.NAME == listBox_recipelist.SelectedValue.ToString())?.First();
            if (recipe == null)
            {
                MessageBox.Show("请先Add Recipe之后再Add Version");
                return;
            }
            var body = JsonConvert.SerializeObject(new AddNewRecipeVersionRequest { EquipmentId = this.comboBox_eqpselect.SelectedValue.ToString(), RecipeName = this.listBox_recipelist.SelectedValue.ToString(), RecipeId = recipe.ID });
            RichTextBoxAddText("Send Web Api");
            RichTextBoxAddText(body);
            var apiresult = HTTPClientHelper.HttpPostRequestAsync4Json(url, body);
            RichTextBoxAddText("Get api result");
            RichTextBoxAddText(apiresult);
            listBox_recipelist_SelectedValueChanged(null, null);
        }

        private void button_reloadbody_Click(object sender, EventArgs e)
        {
            string url = $"{comboBox_urlselect.SelectedItem.ToString()}" + "/api/reloadrecipebody";
            var body = JsonConvert.SerializeObject(new ReloadRecipeBodyRequest { VersionId = this.listBox_version.SelectedValue.ToString(), RecipeName = this.listBox_recipelist.SelectedValue.ToString() });
            RichTextBoxAddText("Send Web Api");
            RichTextBoxAddText(body);
            var apiresult = HTTPClientHelper.HttpPostRequestAsync4Json(url, body);
            RichTextBoxAddText("Get api result");
            RichTextBoxAddText(apiresult);
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            string url = $"{comboBox_urlselect.SelectedItem.ToString()}" + "/api/comparerecipebody";
            var body = JsonConvert.SerializeObject(new CompareRecipeBodyRequest
            {
                EquipmentId = this.comboBox_eqpselect.SelectedValue.ToString(),
                RecipeName = this.listBox_recipelist.SelectedValue.ToString()
            });
            RichTextBoxAddText("Send Web Api");
            RichTextBoxAddText(body);
            var apiresult = HTTPClientHelper.HttpPostRequestAsync4Json(url, body);
            RichTextBoxAddText("Get api result");
            RichTextBoxAddText(apiresult);
        }

        private void button_downloadeffective_Click(object sender, EventArgs e)
        {
            string url = $"{comboBox_urlselect.SelectedItem.ToString()}" + "/api/downloadeffectiverecipetomachine";
            var body = JsonConvert.SerializeObject(new DownloadEffectiveRecipeToMachineRequest
            {
                EquipmentId = this.comboBox_eqpselect.SelectedValue.ToString(),
                RecipeName = this.listBox_recipelist.SelectedValue.ToString()
            });
            RichTextBoxAddText("Send Web Api");
            RichTextBoxAddText(body);
            var apiresult = HTTPClientHelper.HttpPostRequestAsync4Json(url, body);
            RichTextBoxAddText("Get api result");
            RichTextBoxAddText(apiresult);
        }

        private void button_getsv_Click(object sender, EventArgs e)
        {
            try
            {
                int[] vidlist;
                if (string.IsNullOrEmpty(textBox_svs.Text))
                {
                    vidlist = null;
                }
                else
                {
                    vidlist = textBox_svs.Text.Split(',').Select(it => int.Parse(it)).ToArray();
                }

                var trans = new RabbitMqTransaction
                {
                    TransactionName = "GetSvidValue",
                    EquipmentID = "EQTEST01",
                    NeedReply = true,
                    ReplyChannel = this.textBox_listenpath.Text,
                    Parameters = new Dictionary<string, object>() { { "VidList", vidlist } }
                };

                RichTextBoxAddText("Send RMQ");
                RichTextBoxAddText(trans);
                var rep = RabbitMqService.ProduceWaitReply(this.textBox_sendpath.Text, trans, 5);
                RichTextBoxAddText("Receive RMQ");
                RichTextBoxAddText(rep);

            }
            catch (Exception)
            {


            }
        }

        private void button_lock_Click(object sender, EventArgs e)
        {
            var trans = new RabbitMqTransaction
            {
                TransactionName = "LockMachine",
                EquipmentID = "EQTEST01",
                NeedReply = true,
                ReplyChannel = this.textBox_listenpath.Text,
                Parameters = new Dictionary<string, object> { { "IsHeld", true }, { "Message", "LockMessage" } }
            };
            RichTextBoxAddText("Send RMQ");
            RichTextBoxAddText(trans);
            var rep = RabbitMqService.ProduceWaitReply(this.textBox_sendpath.Text, trans, 5);
            RichTextBoxAddText("Receive RMQ");
            RichTextBoxAddText(rep);
        }

        private void button_unlock_Click(object sender, EventArgs e)
        {
            var trans = new RabbitMqTransaction
            {
                TransactionName = "LockMachine",
                EquipmentID = "EQTEST01",
                NeedReply = true,
                ReplyChannel = this.textBox_listenpath.Text,
                Parameters = new Dictionary<string, object> { { "IsHeld", false }, { "Message", "UnlockMessage" } }
            };
            RichTextBoxAddText("Send RMQ");
            RichTextBoxAddText(trans);
            var rep = RabbitMqService.ProduceWaitReply(this.textBox_sendpath.Text, trans, 5);
            RichTextBoxAddText("Receive RMQ");
            RichTextBoxAddText(rep);
        }

        private void button_getstatus_Click(object sender, EventArgs e)
        {
            string url = $"{comboBox_urlselect.SelectedItem.ToString()}" + "/api/getequipmentstatus";
            var body = JsonConvert.SerializeObject(new GetEquipmentStatusRequest
            {
                EquipmentId = this.comboBox_eqpselect.SelectedValue.ToString(),
            });
            RichTextBoxAddText("Send Web Api");
            RichTextBoxAddText(body);
            var apiresult = HTTPClientHelper.HttpPostRequestAsync4Json(url, body);
            RichTextBoxAddText("Get api result");
            RichTextBoxAddText(apiresult);
        }

        private void button2_Click_2(object sender, EventArgs e)
        {
            //var db = DbFactory.GetSqlSugarClient();
            //var aa = db.Queryable<RMS_MARKING_VERSION>().ToList();
            //string str = "abcdefghijkl";
            // var sub = str.Substring(1,)
            //string cc = $"{str}";
            //string dd = $cc;
            int index1 = 0;
            int index2 = 3;
            string[] strArr = { "123", "456", "789", "aaa" };
            string format = $"{{{index1}}}，{{{index2}}}";
            string name = "B";
            int age = 11;
            string reslut = string.Format(format, strArr);
        }

        private void button_getmktext1_Click(object sender, EventArgs e)
        {
            var eqid = comboBox_markingrecipes.Text.Split(',')[0];
            var recipename = comboBox_markingrecipes.Text.Split(',')[1];
            var sfispara = new Dictionary<string, string> { { "MODEL_NAME", recipename } };
            GetMarkingtext(eqid, recipename, sfispara);
        }

        private void GetMarkingtext(string eqid,string recipename,Dictionary<string,string> sfispara)
        {
            string url = $"{comboBox_urlselect.SelectedItem.ToString()}" + "/api/getmarkingtexts";
            var body = JsonConvert.SerializeObject(new GetMarkingTextsRequest
            {
                TrueName = "Test",
                EquipmentId = eqid,
                RecipeName = recipename,
                SfisParameter = sfispara
            });
            RichTextBoxAddText("Send Web Api");
            RichTextBoxAddText(body);
            var apiresult = HTTPClientHelper.HttpPostRequestAsync4Json(url, body);
            RichTextBoxAddText("Get api result");
            RichTextBoxAddText(apiresult);
        }

        private void button_getmktext2_Click(object sender, EventArgs e)
        {
            var eqid = textBox_mkeq.Text;
            var recipename = textBox_mkmodelname.Text;
            var sfispara = new Dictionary<string, string> { { "MODEL_NAME", recipename } };
            GetMarkingtext(eqid, recipename, sfispara);
        }
    }
}
