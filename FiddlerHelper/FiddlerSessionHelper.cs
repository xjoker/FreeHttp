using System.Collections.Generic;
using Fiddler;

namespace FreeHttp.FiddlerHelper
{
    internal class FiddlerSessionHelper
    {
        /// <summary>
        ///     在指定rule列表中寻找匹配rule列表
        /// </summary>
        /// <typeparam name="T">IFiddlerHttpTamper 类型</typeparam>
        /// <param name="oSession">目标oSession</param>
        /// <param name="ruleList">目标rule列表</param>
        /// <param name="isRequest">是否是request （如果不是则为response）</param>
        /// <param name="webSocketMessage">是否为WebSocket规则</param>
        /// <returns>匹配成功的rule列表</returns>
        public static List<IFiddlerHttpTamper> FindMatchTamperRule<T>(Session oSession, List<T> ruleList,
            bool isRequest, WebSocketMessage webSocketMessage = null) where T : IFiddlerHttpTamper
        {
            if (oSession == null || ruleList == null || ruleList.Count == 0) return null;
            var matchRuleList = new List<IFiddlerHttpTamper>();
            var isMatchWebsocket = webSocketMessage != null;
            foreach (T tempItem in ruleList)
            {
                if (!tempItem.IsEnable) continue;
                if (isMatchWebsocket)
                {
                    // WebSocket流程
                    if (tempItem.TamperProtocol == TamperProtocalType.Http) continue;
                    if (!oSession.BitFlags.HasFlag(SessionFlags.IsWebSocketTunnel)) continue;
                    if (tempItem.HttpFilter.Match(oSession, isRequest, webSocketMessage)) matchRuleList.Add(tempItem);
                }
                else
                {
                    // HTTP 流程
                    if (tempItem.TamperProtocol == TamperProtocalType.WebSocket) continue;
                    if (tempItem.HttpFilter.Match(oSession, isRequest)) matchRuleList.Add(tempItem);
                }
            }

            return matchRuleList;
        }
    }
}