using Newtonsoft.Json;
using System;
using System.Configuration;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Security;


public class SsoUser
{
    public string name { get; set; }
    public string mail { get; set; }
    public string department { get; set; }
    public string userId { get; set; }
}
public class SsoHelper
{
    public static string ssoAdsignIn { get; set; } = ConfigurationManager.AppSettings["SSO.ssoAdsignIn"].ToString();
    public static string ssoSignOut { get; set; } = ConfigurationManager.AppSettings["SSO.ssoAdsignOut"].ToString();

    public static SsoUser GetUserWithIp(string ip)
    {
        try
        {
            ip = (ip == "::1") ? "127.0.0.2" : ip;
            string ssourl = ssoAdsignIn;
            string apiId = "s0003";
            string timestamp = GetTimeStamp();
            string key = "ssoAdsignIn";
            string str4Encrypt = apiId + timestamp + key;
            string token = Encrypt32(str4Encrypt).ToLower();
            var msg = JsonConvert.SerializeObject(new
            {
                token = token,
                timestamp = timestamp,
                ip = ip
                //loginname = model.UserName,
                //password = model.Password
            });
            string retmsg = HttpPostRequestAsync4Json(ssourl, msg);
            var jobj = JsonConvert.DeserializeObject(retmsg) as Newtonsoft.Json.Linq.JObject;
            if (jobj!=null && jobj["code"].ToString().Equals("200"))
            {
                var user = JsonConvert.DeserializeObject<SsoUser>(jobj["data"].ToString());
                return user;
            }
            else
            {
                return null;
            }
        }
        catch (Exception)
        {
            return null;
        }
    }
    public static SsoUser GetUserWithUP(string ip,string username,string password)
    {
        try
        {
            ip = (ip == "::1") ? "127.0.0.2" : ip;
            string ssourl = ssoAdsignIn;
            string apiId = "s0003";
            string timestamp = GetTimeStamp();
            string key = "ssoAdsignIn";
            string str4Encrypt = apiId + timestamp + key;
            string token = Encrypt32(str4Encrypt).ToLower();
            var msg = JsonConvert.SerializeObject(new
            {
                token = token,
                timestamp = timestamp,
                ip = ip,
                loginname = username,
                password = password
            });
            string retmsg = HttpPostRequestAsync4Json(ssourl, msg);
            var jobj = JsonConvert.DeserializeObject(retmsg) as Newtonsoft.Json.Linq.JObject;
            if (jobj["code"].ToString().Equals("200"))
            {
                var user = JsonConvert.DeserializeObject<SsoUser>(jobj["data"].ToString());
                return user;
            }
            else
            {
                return null;
            }
        }
        catch (Exception )
        {
            return null;
        }
    }
    public static SsoUser SignOut(string ip)
    {
        ip = (ip == "::1") ? "127.0.0.2" : ip;
        string ssourl = ssoSignOut;
        string apiId = "s0004";
        string timestamp = GetTimeStamp();
        string key = "ssoAdsignOut";
        string str4Encrypt = apiId + timestamp + key;
        string token = Encrypt32(str4Encrypt).ToLower();
        var msg = JsonConvert.SerializeObject(new
        {
            token = token,
            timestamp = timestamp,
            ip = ip
        });
        string retmsg = HttpPostRequestAsync4Json(ssourl, msg);
        var jobj = JsonConvert.DeserializeObject(retmsg) as Newtonsoft.Json.Linq.JObject;
        if (jobj["code"].ToString().Equals("200"))
        {
            var user = JsonConvert.DeserializeObject<SsoUser>(jobj["data"].ToString());
            return user;
        }
        else
        {
            return null;
        }
    }

    public static string GetTimeStamp()
    {
        TimeSpan ts = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        return Convert.ToInt64(ts.TotalSeconds).ToString();
    }
    public static string Encrypt32(string str)
    {
        string cl = str;
        string pwd = "";
        MD5 md5 = MD5.Create();//实例化一个md5对像
                               // 加密后是一个字节类型的数组，这里要注意编码UTF8/Unicode等的选择　
        byte[] s = md5.ComputeHash(Encoding.UTF8.GetBytes(cl));
        // 通过使用循环，将字节类型的数组转换为字符串，此字符串是常规字符格式化所得
        for (int i = 0; i < s.Length; i++)
        {
            // 将得到的字符串使用十六进制类型格式。格式后的字符是小写的字母，如果使用大写（X）则格式后的字符是大写字符

            pwd = pwd + s[i].ToString("X2");

        }
        return pwd;
    }
    public static string HttpPostRequestAsync4Json(string Url, string strBody)
    {
        string result = "";

        try
        {

            using (HttpClient http = new HttpClient())
            {
                http.Timeout = new TimeSpan(0, 0, 0, 0, 2000);//500ms未收到回复抛出异常，return Empty
                http.DefaultRequestHeaders.Add("User-Agent", @"Mozilla/5.0 (compatible; Baiduspider/2.0; +http://www.baidu.com/search/spider.html)");
                http.DefaultRequestHeaders.Add("Accept", @"text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");

                HttpResponseMessage message = null;
                using (Stream dataStream = new MemoryStream(Encoding.UTF8.GetBytes(strBody) ?? new byte[0]))
                {
                    using (HttpContent content = new StreamContent(dataStream))
                    {
                        content.Headers.Add("Content-Type", "application/json");
                        var task = http.PostAsync(Url, content);
                        message = task.Result;
                    }
                }
                if (message != null && message.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    using (message)
                    {
                        result = message.Content.ReadAsStringAsync().Result;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // throw ex;
            Console.WriteLine(ex.Message);
        }
        return result;
    }
}