using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FreeHttp.AutoTest.RunTimeStaticData;
using FreeHttp.FiddlerHelper;
using FreeHttp.MyHelper;
using FreeHttp.WebService.DataModel;

namespace FreeHttp.WebService
{
    public class RemoteRuleService
    {
        protected const string _getRuleUrl = @"{0}freehttp/RuleDetails?userToken={1}";
        protected const string _uploadRuleUrl = @"{0}freehttp/RuleDetails?ruleversion={1}&{2}";


        protected static HttpClient httpClient;

        static RemoteRuleService()
        {
            httpClient = new HttpClient();
        }

        public static async Task<RuleDetails> GetRemoteRuleAsync(string token, string apiUrl = _getRuleUrl)
        {
            var responseMessage = await httpClient.GetAsync(string.Format(apiUrl, ConfigurationData.BaseUrl, token));
            if (responseMessage.StatusCode != HttpStatusCode.OK) return null;
            var ruleDetails =
                MyJsonHelper.JsonDataContractJsonSerializer.JsonStringToObject<RuleDetails>(
                    await responseMessage.Content.ReadAsStringAsync());
            if (ruleDetails == null) return null;

            var nowVersion = UserComputerInfo.GetRuleVersion();

            if (ruleDetails.RuleStaticDataCell != null)
                //if (ruleDetails.RuleStaticData.RuleVersion == nowVersion)
                ruleDetails.StaticDataCollection =
                    MyJsonHelper.JsonDataContractJsonSerializer.JsonStringToObject<ActuatorStaticDataCollection>(
                        ruleDetails.RuleStaticDataCell.RuleContent);

            if (ruleDetails.RuleGroupCell != null)
                //if (ruleDetails.RuleStaticData.RuleVersion == nowVersion)
                ruleDetails.RuleGroup =
                    MyJsonHelper.JsonDataContractJsonSerializer.JsonStringToObject<FiddlerRuleGroup>(ruleDetails
                        .RuleGroupCell.RuleContent);

            if (ruleDetails.RequestRuleCells != null || ruleDetails.ResponseRuleCells != null)
            {
                ruleDetails.ModificHttpRuleCollection = new FiddlerModificHttpRuleCollection();
                ruleDetails.ModificHttpRuleCollection.RequestRuleList = new List<FiddlerRequestChange>();
                ruleDetails.ModificHttpRuleCollection.ResponseRuleList = new List<FiddlerResponseChange>();
                //fill RequestRule
                foreach (var cell in ruleDetails.RequestRuleCells)
                    if (cell.RuleVersion != nowVersion)
                    {
                        ruleDetails.ModificHttpRuleCollection.RequestRuleList.Add(new FiddlerRequestChange
                        {
                            IsEnable = false,
                            HttpFilter = new FiddlerHttpFilter
                            {
                                Name = "unmatch rule version",
                                UriMatch = new FiddlerUriMatch
                                {
                                    MatchMode = FiddlerUriMatchMode.Is,
                                    MatchUri = "unmatch rule version"
                                }
                            }
                        });
                    }
                    else
                    {
                        var tmepRequestChange =
                            MyJsonHelper.JsonDataContractJsonSerializer.JsonStringToObject<FiddlerRequestChange>(
                                cell.RuleContent);
                        ruleDetails.ModificHttpRuleCollection.RequestRuleList.Add(tmepRequestChange ??
                            new FiddlerRequestChange
                            {
                                IsEnable = false,
                                HttpFilter = new FiddlerHttpFilter
                                {
                                    Name = "can not parse this rule",
                                    UriMatch = new FiddlerUriMatch
                                    {
                                        MatchMode = FiddlerUriMatchMode.Is,
                                        MatchUri = "can not parse this rule"
                                    }
                                }
                            });
                    }

                //fill ResponseRule
                foreach (var cell in ruleDetails.ResponseRuleCells)
                    if (cell.RuleVersion != nowVersion)
                    {
                        ruleDetails.ModificHttpRuleCollection.ResponseRuleList.Add(new FiddlerResponseChange
                        {
                            IsEnable = false,
                            HttpFilter = new FiddlerHttpFilter
                            {
                                Name = "unmatch rule version",
                                UriMatch = new FiddlerUriMatch
                                {
                                    MatchMode = FiddlerUriMatchMode.Is,
                                    MatchUri = "unmatch rule version"
                                }
                            }
                        });
                    }
                    else
                    {
                        var tmepRequestChange =
                            MyJsonHelper.JsonDataContractJsonSerializer.JsonStringToObject<FiddlerResponseChange>(
                                cell.RuleContent);
                        ruleDetails.ModificHttpRuleCollection.ResponseRuleList.Add(tmepRequestChange ??
                            new FiddlerResponseChange
                            {
                                IsEnable = false,
                                HttpFilter = new FiddlerHttpFilter
                                {
                                    Name = "can not parse this rule",
                                    UriMatch = new FiddlerUriMatch
                                    {
                                        MatchMode = FiddlerUriMatchMode.Is,
                                        MatchUri = "can not parse this rule"
                                    }
                                }
                            });
                    }
            }

            return ruleDetails;
        }

        public static async Task<string> UploadRulesAsync<T1, T2>(List<T1> requestRules, List<T2> responseRules,
            ActuatorStaticDataCollection staticDataCollection = null, FiddlerRuleGroup ruleGroup = null,
            string executeUrl = null) where T1 : IFiddlerHttpTamper where T2 : IFiddlerHttpTamper
        {
            var multipartFormData = new MultipartFormDataContent();
            if (staticDataCollection != null)
                multipartFormData.Add(
                    new StringContent(
                        MyJsonHelper.JsonDataContractJsonSerializer.ObjectToJsonStr(staticDataCollection)),
                    "staticData");
            if (ruleGroup != null)
                multipartFormData.Add(
                    new StringContent(MyJsonHelper.JsonDataContractJsonSerializer.ObjectToJsonStr(ruleGroup)),
                    "groupData");
            if (requestRules != null)
                foreach (var request in requestRules)
                    multipartFormData.Add(
                        new StringContent(MyJsonHelper.JsonDataContractJsonSerializer.ObjectToJsonStr(request)),
                        "requestRule");
            if (responseRules != null)
                foreach (var response in responseRules)
                    multipartFormData.Add(
                        new StringContent(MyJsonHelper.JsonDataContractJsonSerializer.ObjectToJsonStr(response)),
                        "responseRule");

            try
            {
                var httpResponseMessage = await httpClient.PostAsync(
                    string.Format(executeUrl ?? _uploadRuleUrl, ConfigurationData.BaseUrl,
                        UserComputerInfo.GetRuleVersion(), UserComputerInfo.GetFreeHttpUser()), multipartFormData);
                if (httpResponseMessage.IsSuccessStatusCode)
                    return await httpResponseMessage.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                await RemoteLogService.ReportLogAsync(ex.ToString(), RemoteLogService.RemoteLogOperation.RuleUpload,
                    RemoteLogService.RemoteLogType.Error);
            }

            return null;
        }
    }
}