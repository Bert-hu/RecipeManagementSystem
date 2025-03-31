using log4net;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Rms.Web.Utils
{
    public class MailHelper
    {
        private static log4net.ILog Log = LogManager.GetLogger("Debug");

        public static bool SendMail(string[] MessageTo, string MessageSubject, string MessageBody, List<Attachment> attachments =null)
        {
            //bool isDebugMode = ConfigurationManager.AppSettings["IsDedugMode"].ToString().Trim().ToUpper() == "TRUE";
            //if (isDebugMode)
            //{
            //    MessageTo = new string[]{
            //    "bert_hu@usiglobal.com"
            //    };
            //}

            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(ValidateServerCertificate);//此处注册只为防止个别邮箱证书验证失败问题
            MailAddress MessageFrom = new MailAddress("zj.sys.smd_systems@usiglobal.com");
            MailMessage message = new MailMessage();
            message.From = MessageFrom;
            foreach (var mt in MessageTo)
            {
                message.To.Add(mt);             //收件人邮箱地址可以是多个以实现群发
            }
            message.Subject = MessageSubject;
            message.Body = MessageBody;
            message.IsBodyHtml = true;              //是否为html格式
            message.Priority = MailPriority.High;  //发送邮件的优先等级
            if (attachments != null)
            {
                foreach (var attachment in attachments)
                {
                    message.Attachments.Add(attachment);
                }
            }

            SmtpClient sc = new SmtpClient();
            sc.Host = "hybrid.usiglobal.com";              //指定发送邮件的服务器地址或IP
            sc.Port = 25;//指定发送邮件端口
            sc.UseDefaultCredentials = false;
            sc.EnableSsl = false;
            sc.Credentials = new NetworkCredential("zj.sys.smd_systems@usiglobal.com", "Usish@2022");//指定登录服务器的用户名和密码
            try
            {
                sc.Send(message);      //发送邮件
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                Log.Error(ex.Message);
                return false;
            }
            return true;
        }
        private static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
    }
}
