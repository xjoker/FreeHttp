using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using Fiddler;
using FreeHttp.AutoTest;
using FreeHttp.MyHelper;

namespace FreeHttp.FiddlerHelper
{
    /// <summary>
    /// 匹配模式
    /// </summary>
    public enum FiddlerUriMatchMode
    {
        Contain,
        StartWith,
        EndWith,
        Is,
        Regex,
        /// <summary>
        /// User-Agent 匹配模式
        /// </summary>
        UAContain,
        AllPass
    }

    [Serializable]
    [DataContract]
    public class FiddlerUriMatch
    {
        public FiddlerUriMatch()
        {
            MatchMode = FiddlerUriMatchMode.AllPass;
            MatchUri = null;
        }

        public FiddlerUriMatch(FiddlerUriMatchMode matchMode, string matchUri)
        {
            MatchMode = matchMode;
            MatchUri = matchUri;
        }

        [DataMember] public FiddlerUriMatchMode MatchMode { get; set; }

        [DataMember] public string MatchUri { get; set; }

        /// <summary>
        /// 匹配检测 增加UserAgent匹配检测
        /// </summary>
        /// <param name="session"></param>
        /// <param name="matchString"></param>
        /// <returns></returns>
        public bool Match(Session session,string matchString)
        {
            switch (MatchMode)
            {
                case FiddlerUriMatchMode.UAContain:
                    return session.RequestHeaders.Any(x =>
                        x.Name.ToLower() == "user-agent" && x.Value.Contains(MatchUri));
                case FiddlerUriMatchMode.Contain:
                case FiddlerUriMatchMode.StartWith:
                case FiddlerUriMatchMode.EndWith:
                case FiddlerUriMatchMode.Is:
                case FiddlerUriMatchMode.Regex:
                case FiddlerUriMatchMode.AllPass:
                default:
                    return Match(matchString);
            }
        }

        /// <summary>
        /// 匹配检测
        /// </summary>
        /// <param name="matchString"></param>
        /// <returns></returns>
        public bool Match(string matchString)
        {
            switch (MatchMode)
            {
                case FiddlerUriMatchMode.AllPass:
                    return true;
                case FiddlerUriMatchMode.Contain:
                    return matchString.Contains(MatchUri);
                case FiddlerUriMatchMode.Is:
                    return matchString == MatchUri;
                case FiddlerUriMatchMode.Regex:
                    return Regex.IsMatch(matchString, MatchUri);
                case FiddlerUriMatchMode.StartWith:
                    return matchString.StartsWith(MatchUri);
                case FiddlerUriMatchMode.EndWith:
                    return matchString.EndsWith(MatchUri);
                default:
                    return false;
            }
        }

        public bool Equals(FiddlerUriMatch targetUriMatch)
        {
            return MatchMode == targetUriMatch.MatchMode && MatchUri == targetUriMatch.MatchUri;
        }

        public new bool Equals(object targetFiddlerHttpTamper)
        {
            var fiddlerHttpTamper = targetFiddlerHttpTamper as IFiddlerHttpTamper;
            if (fiddlerHttpTamper == null) return false;
            return Equals(fiddlerHttpTamper.HttpFilter.UriMatch);
        }

        public new string ToString()
        {
            return $"[{MatchMode}] {(string.IsNullOrEmpty(MatchUri) ? "" : MatchUri)}";
        }
    }

    [Serializable]
    [DataContract]
    public class FiddlerHeadMatch
    {
        public FiddlerHeadMatch()
        {
            HeadsFilter = null;
        }

        public FiddlerHeadMatch(List<MyKeyValuePair<string, string>> headsFilter)
        {
            HeadsFilter = headsFilter;
        }

        [DataMember] public List<MyKeyValuePair<string, string>> HeadsFilter { get; set; }

        public void AddHeadMatch(MyKeyValuePair<string, string> yourHeadMatch)
        {
            if (HeadsFilter == null) HeadsFilter = new List<MyKeyValuePair<string, string>>();
            HeadsFilter.Add(yourHeadMatch);
        }

        public bool Match(HTTPHeaders matchHeaders)
        {
            if (HeadsFilter != null && HeadsFilter.Count > 0)
                foreach (var headFilter in HeadsFilter)
                    if (!matchHeaders.ExistsAndContains(headFilter.Key, headFilter.Value))
                        return false;
            return true;
        }

        public bool Equals(FiddlerHeadMatch yourFiddlerHeadMatch)
        {
            if (yourFiddlerHeadMatch.HeadsFilter.Count == HeadsFilter.Count)
            {
                var HeadsFilterForEqual = HeadsFilter.MyClone();
                foreach (var tempHead in yourFiddlerHeadMatch.HeadsFilter)
                    if (HeadsFilterForEqual.MyContains(tempHead))
                        HeadsFilterForEqual.Remove(tempHead);
                    else
                        return false;
                if (HeadsFilterForEqual.Count == 0) return true;
            }

            return false;
        }

        public new string ToString()
        {
            if (HeadsFilter == null || HeadsFilter.Count == 0) return null;
            var tempSb = new StringBuilder(HeadsFilter.Count * 30);
            foreach (var tempKv in HeadsFilter)
                tempSb.AppendLine($"{tempKv.Key} [contain] {tempKv.Value}");
            if (tempSb[tempSb.Length - 2] == '\r' && tempSb[tempSb.Length - 1] == '\n')
                tempSb.Remove(tempSb.Length - 2, 2);
            return tempSb.ToString();
        }
    }

    [Serializable]
    [DataContract]
    public class FiddlerBodyMatch : FiddlerUriMatch
    {
        private string bufferBodyBytesStr;

        public FiddlerBodyMatch()
        {
        }

        public FiddlerBodyMatch(FiddlerUriMatchMode matchMode, string matchData) //: base(matchMode, matchUri)
        {
            if (string.IsNullOrEmpty(matchData) && matchMode != FiddlerUriMatchMode.AllPass)
                throw new Exception("empty data is illegal for this mode");
            if (matchData.StartsWith("<hex>"))
            {
                if (matchMode == FiddlerUriMatchMode.Regex) throw new Exception("Regex can not use hex mode");
                MatchBodyBytes = MyBytes.HexStringToByte(matchData.Remove(0, "<hex>".Length), HexDecimal.hex16);
                if ((MatchBodyBytes == null || MatchBodyBytes.Length == 0) && matchMode != FiddlerUriMatchMode.AllPass)
                    throw new Exception("empty data is illegal for this mode");
                MatchMode = matchMode;
                MatchUri = string.Format("<hex>{0}", BitConverter.ToString(MatchBodyBytes));
            }
            else
            {
                MatchMode = matchMode;
                MatchUri = matchData;
            }
        }

        [DataMember] public byte[] MatchBodyBytes { get; set; }

        [XmlIgnore] public bool IsHexMatch => MatchBodyBytes != null;

        public new bool Match(string matchString)
        {
            if (IsHexMatch) return false;
            return base.Match(matchString);
        }

        public static FiddlerBodyMatch GetFiddlerBodyMatch(FiddlerUriMatchMode matchMode, string matchData)
        {
            try
            {
                return new FiddlerBodyMatch(matchMode, matchData);
            }
            catch
            {
                // ignored
            }

            return null;
        }

        public bool Match(byte[] matchBytes)
        {
            if (MatchBodyBytes != null && MatchBodyBytes == null && MatchBodyBytes.Length == 0) return false;
            if (bufferBodyBytesStr == null)
                if (MatchBodyBytes != null)
                    bufferBodyBytesStr = BitConverter.ToString(MatchBodyBytes);
            var matchString = BitConverter.ToString(matchBytes);
            switch (MatchMode)
            {
                case FiddlerUriMatchMode.AllPass:
                    return true;
                case FiddlerUriMatchMode.Contain:
                    return bufferBodyBytesStr != null && matchString.Contains(bufferBodyBytesStr);
                case FiddlerUriMatchMode.Is:
                    return matchString == bufferBodyBytesStr;
                case FiddlerUriMatchMode.Regex:
                    return false;
                case FiddlerUriMatchMode.StartWith:
                    return bufferBodyBytesStr != null && matchString.StartsWith(bufferBodyBytesStr);
                default:
                    return false;
            }
        }
    }

    [Serializable]
    [DataContract]
    public class FiddlerHttpFilter
    {
        public FiddlerHttpFilter()
        {
            UriMatch = null;
        }

        public FiddlerHttpFilter(FiddlerUriMatch uriMatch)
        {
            UriMatch = uriMatch;
        }

        [DataMember] public string Name { get; set; }

        [DataMember] public FiddlerUriMatch UriMatch { get; set; } //UriMatch  must not be null

        [DataMember] public FiddlerHeadMatch HeadMatch { get; set; }

        [DataMember] public FiddlerBodyMatch BodyMatch { get; set; }

        public bool Match(Session oSession, bool isRequest, WebSocketMessage webSocketMessage = null)
        {
            var isWebSocket = webSocketMessage != null; 
      
            if (isWebSocket)
            {
                if (!oSession.BitFlags.HasFlag(SessionFlags.IsWebSocketTunnel)) return false;
                if (!((isRequest && webSocketMessage.IsOutbound) || (!isRequest && !webSocketMessage.IsOutbound)))
                    return false;
                if (!UriMatch.Match(oSession,oSession.fullUrl)) return false;
                if (BodyMatch != null)
                {
                    if (webSocketMessage.FrameType == WebSocketFrameTypes.Binary && BodyMatch.IsHexMatch)
                    {
                        if (!BodyMatch.Match(webSocketMessage.PayloadAsBytes())) return false;
                    }
                    else if (webSocketMessage.FrameType == WebSocketFrameTypes.Text && !BodyMatch.IsHexMatch)
                    {
                        if (!BodyMatch.Match(webSocketMessage.PayloadAsString())) return false;
                    }
                    else if (webSocketMessage.FrameType == WebSocketFrameTypes.Continuation)
                    {
                        //延续帧
                        return false;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (UriMatch != null)
                    if (!UriMatch.Match(oSession, oSession.fullUrl))
                        return false;
                if (HeadMatch != null)
                    if (!HeadMatch.Match(oSession.RequestHeaders))
                        return false;
                if (BodyMatch != null)
                {
                    if (BodyMatch.IsHexMatch)
                    {
                        if (!BodyMatch.Match(oSession.requestBodyBytes))
                            return false;
                    }
                    else
                    {
                        if (!BodyMatch.Match(oSession.GetRequestBodyAsString())) return false;
                    }
                }
            }

            return true;
        }

        public bool Equals(FiddlerHttpFilter yourFiddlerHttpFilter)
        {
            if (!UriMatch.Equals(yourFiddlerHttpFilter.UriMatch)) return false;

            if ((HeadMatch == null || yourFiddlerHttpFilter.HeadMatch == null) &&
                !(HeadMatch == null && yourFiddlerHttpFilter.HeadMatch == null)) return false;
            if (HeadMatch != null && yourFiddlerHttpFilter.HeadMatch != null)
                if (!HeadMatch.Equals(yourFiddlerHttpFilter.HeadMatch))
                    return false;

            if ((BodyMatch == null || yourFiddlerHttpFilter.BodyMatch == null) &&
                !(BodyMatch == null && yourFiddlerHttpFilter.BodyMatch == null)) return false;
            if (BodyMatch != null && yourFiddlerHttpFilter.BodyMatch != null)
                if (!BodyMatch.Equals(yourFiddlerHttpFilter.BodyMatch))
                    return false;

            return true;
        }

        public new bool Equals(object targetFiddlerHttpFilter)
        {
            var fiddlerHttpTamper = targetFiddlerHttpFilter as IFiddlerHttpTamper;
            if (fiddlerHttpTamper == null) return false;
            return Equals(fiddlerHttpTamper.HttpFilter);
        }

        public string GetShowTitle()
        {
            if (!string.IsNullOrEmpty(Name)) return Name;
            if (UriMatch != null) return string.Format("【{0}】: {1}", UriMatch.MatchMode.ToString(), UriMatch.MatchUri);
            return default;
        }

        public new string ToString()
        {
            var tempSb = new StringBuilder(string.Format("Uri:\r\n{0}\r\n", UriMatch.ToString()));
            if (HeadMatch != null) tempSb.AppendLine(string.Format("Heads:\r\n{0}", HeadMatch.ToString()));
            if (BodyMatch != null) tempSb.AppendLine(string.Format("Body:\r\n{0}", BodyMatch.ToString()));
            return tempSb.ToString();
        }
    }
}