using System.Net.Http;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Fiddler;
using FreeHttp.MyHelper;

namespace FreeHttp.WebService
{
    public class RemoteLogService
    {
        public enum RemoteLogOperation
        {
            Unknown,
            Exception,
            SilentUpgrade,
            CheckUpgrade,
            SessionTamp,
            WindowLoad,
            RuleUpload,
            RemoteRule,
            AddRule,
            ShareRule,
            EditRule,
            ExecuteRule,
            CommonBusiness
        }

        public enum RemoteLogType
        {
            Unknown,
            Info,
            Warning,
            Error
        }

        private static readonly HttpClient httpClient;

        static RemoteLogService()
        {
            httpClient = new HttpClient();
        }

        public static async Task ReportLogAsync(string message,
            RemoteLogOperation operation = RemoteLogOperation.Unknown, RemoteLogType type = RemoteLogType.Info)
        {
            // 关闭日志
            var remoteLogDetail = new RemoteLogDetail
            {
                UserToken = UserComputerInfo.UserToken,
                UserMac = UserComputerInfo.GetComputerMac(),
                MachineName = UserComputerInfo.GetMachineName(),
                Version = UserComputerInfo.GetFreeHttpVersion(),
                Type = type.ToString(),
                Operation = operation.ToString(),
                Message = message
            };
            try
            {
                FiddlerObject.log($"[ERROR]{MyJsonHelper.JsonDataContractJsonSerializer.ObjectToJsonStr(remoteLogDetail)}");
                //await httpClient.PostAsync(string.Format(@"{0}freehttp/UserLogReport", ConfigurationData.BaseUrl),
                //    new StringContent(MyJsonHelper.JsonDataContractJsonSerializer.ObjectToJsonStr(remoteLogDetail),
                //        Encoding.UTF8, "application/json"));
            }
            catch
            {
            }
        }

        [DataContract]
        private class RemoteLogDetail
        {
            [DataMember] public string UserToken { get; set; }

            [DataMember] public string UserMac { get; set; }

            [DataMember] public string MachineName { get; set; }

            [DataMember] public string Version { get; set; }

            [DataMember] public string Type { get; set; }

            [DataMember] public string Operation { get; set; }

            [DataMember] public string Message { get; set; }
        }
    }
}