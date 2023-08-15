
using Newtonsoft.Json;
using Rms.Services.Services.ApiHandle;
using System;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace EAP.Services.Utils
{
    public class APIListener
    {
        HttpListener listener;
        private static log4net.ILog Log = log4net.LogManager.GetLogger("Logger");
        //private string route;


        public APIListener(string port, string route = "api")
        {
            string serverip = string.Empty;
            var AddressList = Dns.GetHostEntry(Dns.GetHostName()).AddressList.Where(it => it.AddressFamily == AddressFamily.InterNetwork).ToList();
            //this.route = route;
            listener = new HttpListener();

            listener.Prefixes.Add($"http://*:{port}/{route.Trim('/')}/");


        }

        public void StartListen()
        {
            try
            {
                if (listener != null && !listener.IsListening)
                {
                    listener.Start();

                    //listener.BeginGetContext(new AsyncCallback(ListenerHandle), listener);
                    Task.Run(() =>
                    {
                        while (true)
                        {
                            HttpListenerContext context = listener.GetContext();
                            Task.Run(() => ProcessRequest(context));
                        }
                    });
                }
            }

            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        public void StopListen()
        {
            try
            {
                if (listener != null && listener.IsListening)
                {
                    listener.Stop();
                }
            }
            catch
            {

            }
        }
        private void ProcessRequest(HttpListenerContext context)
        {
            try
            {
                HttpListenerRequest request = context.Request;
                string content = string.Empty;
                string ResponseJsonMsg = string.Empty;
                string route = request.RawUrl;
                if (request.HttpMethod == "POST")
                {
                    Stream stream = context.Request.InputStream;
                    StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                    content = reader.ReadToEnd();
                    Log.Info($"{route},{content}");
                    ApiMessageHandler handler = new ApiMessageHandler();
                    var ret = handler.HandleMessage(route, content);
                    context.Response.StatusCode = 200;
                    ResponseJsonMsg = JsonConvert.SerializeObject(ret);

                }
                else
                {
                    context.Response.StatusCode = 404;
                }
                //构造Response响应
                HttpListenerResponse response = context.Response;
                response.Headers.Add("Access-Control-Allow-Origin", "*");
                response.ContentType = "application/json";
                response.ContentEncoding = Encoding.UTF8;
                byte[] buffer = Encoding.UTF8.GetBytes(ResponseJsonMsg);
                context.Response.ContentLength64 = buffer.Length;
                var output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
                output.Close();
                response.Close();
            }
            catch (Exception ex)
            {
                Log.Error(ex);

            }

        }

        public static bool PortInUse(int port)
        {
            bool inUse = false;
            IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] iPEndPoints = ipProperties.GetActiveTcpListeners();

            foreach (IPEndPoint endPoint in iPEndPoints)
            {
                if (endPoint.Port == port)
                {
                    inUse = true;
                    break;
                }
            }
            return inUse;
        }
    }
}
