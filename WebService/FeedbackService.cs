//#define NET4_5UP

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Fiddler.WebFormats;
using FreeHttp.MyHelper;

namespace FreeHttp.WebService
{
    public class FeedbackService
    {
        [DataContract]
        public class Feedback
        {
            public Feedback(string token, string mac, string machine, string contact, string content)
            {
                user_token = token;
                user_mac = mac;
                machine_name = machine;
                contact_infomation = contact;
                feedback_content = content;
            }

            [DataMember] public string user_token { get; set; }

            [DataMember] public string user_mac { get; set; }

            [DataMember] public string machine_name { get; set; }

            [DataMember] public string contact_infomation { get; set; }

            [DataMember] public string feedback_content { get; set; }
        }

#if NET4_5UP
        public static async Task<int> SubmitFeedbackAsync(string userToken, string mac, string machine, string contact,
            string content)
        {
            return await SubmitFeedbackAsync(new Feedback(userToken, mac, machine, contact, content));
        }

        public static async Task<int> SubmitFeedbackAsync(Feedback feedback)
        {
            if (feedback == null) return -1;
            Func<int> SubmitFeedbackTask = () =>
            {
                //使用 Fiddler.WebFormats.JSON.JsonEncode 不要引入 第三方 库， 或者需要使用DataContract注解
                var feedbackBody = string.Format(
                    "{{ \"user_mac\":{0},\"machine_name\":{1},\"feedback_content\":{2},\"contact_infomation\": {3}}}"
                    , JSON.JsonEncode(feedback.user_mac),
                    JSON.JsonEncode(feedback.machine_name),
                    JSON.JsonEncode(feedback.feedback_content),
                    JSON.JsonEncode(feedback.contact_infomation));
                var responseCode = new MyWebTool.MyHttp().SendHttpRequest(
                        string.Format("{0}freehttp/Feedback", ConfigurationData.BaseUrl),
                        MyJsonHelper.JsonDataContractJsonSerializer.ObjectToJsonStr(feedback), "POST",
                        new List<KeyValuePair<string, string>>
                            { new KeyValuePair<string, string>("Content-Type", "application/json") }, false, null, null)
                    .StatusCode;
                return responseCode;
            };
            var code = await Task.Run(SubmitFeedbackTask);
            return code;
        }
#endif

#if NET4
        //因为.net4.5 以下没有Task.Run，该方法用于低版本编译
        public static void SubmitFeedbackTask(string mac, string contact, string content,Action<int> showResult)
        {
            Task<int> submitFeedback = new Task<int>(() =>
            {
                Feedback feedback = new Feedback(mac, contact, content);
                string feedbackBody =
 String.Format("{{ \"user_mac\":{0},\"feedback_content\":{1},\"contact_infomation\": {2}}}"
                    , Fiddler.WebFormats.JSON.JsonEncode(feedback.user_mac),
                    Fiddler.WebFormats.JSON.JsonEncode(feedback.feedback_content),
                    Fiddler.WebFormats.JSON.JsonEncode(feedback.contact_infomation));
                int responseCode =
 (new WebService.MyWebTool.MyHttp()).SendHttpRequest(string.Format("{0}freehttp/Feedback", ConfigurationData.BaseUrl), feedbackBody, "POST",  new List<KeyValuePair<string, string>>() { new KeyValuePair<string, string>("Content-Type", "application/json") }, false, null, null).StatusCode;
                return responseCode;
            });
            submitFeedback.Start();
            submitFeedback.ContinueWith((task) => { showResult(task.Result); }) ;
        }
#endif
    }
}