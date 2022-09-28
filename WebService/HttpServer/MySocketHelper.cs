using System.Net.NetworkInformation;

namespace FreeHttp.WebService.HttpServer
{
    public class MySocketHelper
    {
        public static bool IsPortInTcpListening(int port)
        {
            var ipProperties = IPGlobalProperties.GetIPGlobalProperties();
            var ipEndPoints = ipProperties.GetActiveTcpListeners();

            foreach (var endPoint in ipEndPoints)
                if (endPoint.Port == port)
                    return true;
            return false;
        }
    }
}