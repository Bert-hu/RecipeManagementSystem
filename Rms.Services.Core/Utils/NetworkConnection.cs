using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Rms.Services.Core.Utils
{
    public class NetworkConnection : IDisposable
    {
        private string _networkName;

        public NetworkConnection(string networkName, NetworkCredential credentials)
        {
            _networkName = networkName;

            // 创建网络资源使用的网络访问器
            var netResource = new NetResource
            {
                dwType = 1,
                lpRemoteName = networkName
            };
            WNetCancelConnection2(_networkName, 0, true);
            // 连接到共享路径
            var result = WNetAddConnection2(netResource, credentials.Password, credentials.UserName, 0);
            if (result == 1219)
            {
                WNetCancelConnection2(networkName, 0, true);
                result = WNetAddConnection2(netResource, credentials.Password, credentials.UserName, 0);
            }
            if (result != 0 && result != 1219)
            {
                throw new IOException($"无法连接到网络资源。错误码: {result}");
            }
        }

        ~NetworkConnection()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            WNetCancelConnection2(_networkName, 0, true);
        }

        [DllImport("mpr.dll")]
        private static extern int WNetAddConnection2(NetResource netResource,
            string password, string username, int flags);

        [DllImport("mpr.dll")]
        private static extern int WNetCancelConnection2(string name, int flags,
            bool force);
    }

    [Flags]
    public enum ResourceScope
    {
        Connected = 1,
        GlobalNetwork,
        Remembered,
        Recent,
        Context
    };

    [Flags]
    public enum ResourceType
    {
        Any = 0,
        Disk = 1,
        Print = 2,
        Reserved = 8,
    }

    [Flags]
    public enum ResourceDisplaytype
    {
        Generic = 0x0,
        Domain = 0x01,
        Server = 0x02,
        Share = 0x03,
        File = 0x04,
        Group = 0x05,
        Network = 0x06,
        Root = 0x07,
        Shareadmin = 0x08,
        Directory = 0x09,
        Tree = 0x0a,
        Ndscontainer = 0x0b
    }

    [StructLayout(LayoutKind.Sequential)]
    public class NetResource
    {
        public int dwScope = 0;
        public int dwType = 0;
        public int dwDisplayType = 0;
        public int dwUsage = 0;
        public string lpLocalName = null;
        public string lpRemoteName = null;
        public string lpComment = null;
        public string lpProvider = null;
    }
}
