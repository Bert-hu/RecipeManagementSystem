namespace Rms.Services.Core.Utils
{
    public class BlockedIpMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IList<string> _blockedIps;

        public BlockedIpMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _blockedIps = configuration["BlockedIps"]
                .Split(',')
                .Select(ip => ip.Trim())
                .ToList();
        }

        public async Task Invoke(HttpContext context)
        {
            var remoteIp = context.Connection.RemoteIpAddress.ToString();
            if (_blockedIps.Contains(remoteIp))
            {
                context.Response.StatusCode = 403; // Forbidden
                await context.Response.WriteAsync("Access Forbidden.");
            }
            else
            {
                await _next(context);
            }
        }
    }
}
