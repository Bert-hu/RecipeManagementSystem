using log4net;
using Newtonsoft.Json;
using Rms.UserClient.Model;
using Rms.UserClient.Utils;
using Sunny.UI;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Principal;
using System.Text;
using System.Windows.Forms;

namespace Rms.UserClient
{
    public partial class MainForm : UIForm
    {
        private static log4net.ILog OnlyLog = LogManager.GetLogger("Logger");
        private static log4net.ILog TextboxLog = LogManager.GetLogger("TextBoxLogger");

        FormVariables variables;
        public MainForm()
        {
            try
            {
                variables = JsonConvert.DeserializeObject<FormVariables>(File.ReadAllText(System.Windows.Forms.Application.StartupPath + @"\config.json"));
            }
            catch (Exception)
            {
                variables = new FormVariables();
            }


            InitializeComponent();

            DataBinding();

        }

        private void DataBinding()
        {
            //注意这里 DataSourceUpdateMode.OnPropertyChanged  ，否则textbox只有在失去焦点的时候更新绑定对象的值
            // this.textBox_line.DataBindings.Add("Text", variables, nameof(variables.Line), false, DataSourceUpdateMode.OnPropertyChanged);
            // this.textBox_group.DataBindings.Add("Text", variables, nameof(variables.Group), false, DataSourceUpdateMode.OnPropertyChanged);
            // this.textBox_station.DataBindings.Add("Text", variables, nameof(variables.Station), false, DataSourceUpdateMode.OnPropertyChanged);
            // this.textBox_emp.DataBindings.Add("Text", variables, nameof(variables.EMP), false, DataSourceUpdateMode.OnPropertyChanged);
            // this.textBox_modelName.DataBindings.Add("Text", variables, nameof(variables.ModelName), false, //DataSourceUpdateMode.OnPropertyChanged);
            // this.textBox_reelId.DataBindings.Add("Text", variables, nameof(variables.ReelID), false, DataSourceUpdateMode.OnPropertyChanged);

            //var appender = LogManager.GetRepository().GetAppenders()[0] as RichTextBoxAppender;
            //Log4net 绑定
            var appender = LogManager.GetRepository().GetAppenders().First(it => it.Name == "RichTextBoxAppender") as RichTextBoxAppender;
            appender.RichTextBox = this.richTextBox1;
        }

        bool IsRunAsAdmin()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            var isadmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
            string compatLayer = Environment.GetEnvironmentVariable("__COMPAT_LAYER");
            bool isRUNASINVOKER = false;
            if (compatLayer != null && compatLayer.ToUpper().Equals("RUNASINVOKER"))
            {
                isRUNASINVOKER = true;
            }
            OnlyLog.Debug(compatLayer?.ToString());
            return isadmin || isRUNASINVOKER;
        }




        private bool SendMessageToSFIS(string message, ref string receiveMsg, ref string errMsg, int timeout = 5)
        {
            try
            {
                string ip = variables.SfisIp;

                int sfisport = variables.SfisPort;
                IPAddress serverip = IPAddress.Parse(ip);
                IPEndPoint point = new IPEndPoint(serverip, sfisport);
                TcpClient tcpSync = new TcpClient();
                tcpSync.Connect(point);
                if (tcpSync != null && tcpSync.Connected)
                {
                    NetworkStream nStream = tcpSync.GetStream();
                    byte[] data = new byte[1024];
                    data = Encoding.UTF8.GetBytes(message);
                    nStream.Write(data, 0, data.Length);
                    nStream.Flush();
                    TextboxLog.Info($"Send to SFIS: {message}");
                    int len = 0;
                    int trytimes = 1;//尝试接收答复次数
                    data = new byte[1024 * 30];

                    nStream.ReadTimeout = timeout * 1000;//读取超时
                    while (len == 0 && trytimes > 0)
                    {
                        nStream = tcpSync.GetStream();
                        try
                        {
                            len = nStream.Read(data, 0, data.Length);
                        }
                        catch (System.IO.IOException)
                        {
                            //debugLog.Error(e);
                            trytimes--;
                        }

                    }
                    if (len > 0)
                    {
                        receiveMsg = Encoding.UTF8.GetString(data, 0, len);
                        TextboxLog.Info($"Receive from SFIS: {receiveMsg}");
                        nStream.Flush();
                        tcpSync.Close();

                        return true;
                    }
                    else
                    {
                        errMsg = "SFIS Timeout";
                        tcpSync.Close();
                        return false;
                    }
                }
                else
                {
                    errMsg = "Can not connect to SFIS";
                    return false;
                }
            }
            catch (Exception ex)
            {
                TextboxLog.Error(ex);
                errMsg = "EAP send message to SFIS fail.";
                return false;
            }
        }




        private void MainForm_Shown(object sender, EventArgs e)
        {


        }


        private void SaveConfig()
        {
            var obj = new
            {
                Line = variables.Line,
                Group = variables.Group,
                Station = variables.Station,
                EMP = variables.EMP,
                ModelName = variables.ModelName,
                ReelID = variables.ReelID,
            };
            File.WriteAllText(System.Windows.Forms.Application.StartupPath + @"\config.json", JsonConvert.SerializeObject(variables, Newtonsoft.Json.Formatting.Indented));
        }

        private void textBox_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Control control = (Control)sender;
            ValueChangeForm form = new ValueChangeForm(control.Text);
            DialogResult result = form.ShowDialog();
            if (result == DialogResult.OK)
            {
                control.Text = form.Value;
                SaveConfig();
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }
    }
}
