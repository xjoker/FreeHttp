using Fiddler;
using FreeHttp.AutoTest;
using FreeHttp.AutoTest.ParameterizationPick;
using FreeHttp.FiddlerHelper;
using FreeHttp.FreeHttpControl;
using FreeHttp.HttpHelper;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;

namespace FreeHttp
{
    public class FiddlerSessionTamper
    {
        /// <summary>
        ///     Modified the http request in oSession with your rule
        /// </summary>
        /// <param name="oSession">oSession</param>
        /// <param name="nowFiddlerRequestChange">FiddlerRequsetChange</param>
        /// <param name="showError"></param>
        /// <param name="showMes"></param>
        public static void ModifiedSessionRequest(Session oSession, FiddlerRequestChange nowFiddlerRequestChange,
            Action<string> showError, Action<string> showMes)
        {
            if (nowFiddlerRequestChange.ParameterPickList != null)
                PickSessionParameter(oSession, nowFiddlerRequestChange, showError, showMes, true);
            if (nowFiddlerRequestChange.IsRawReplace)
            {
                ReplaceSessionRequest(oSession, nowFiddlerRequestChange, showError, showMes);
            }
            else
            {
                string errMes;
                var nameValueCollection = new NameValueCollection();
                //Modified uri
                if (nowFiddlerRequestChange.UriModific != null &&
                    nowFiddlerRequestChange.UriModific.ModifiedMode != ContentModifiedMode.NoChange)
                {
                    try
                    {
                        oSession.fullUrl =
                            nowFiddlerRequestChange.UriModific.GetFinalContent(oSession.fullUrl, nameValueCollection,
                                out errMes);
                    }
                    catch (Exception ex)
                    {
                        errMes = ex.Message;
                    }

                    if (errMes != null)
                        showError($"error in GetFinalContent in UriModified that [{errMes}]");
                }

                //Modified body
                if (nowFiddlerRequestChange.BodyModific != null &&
                    nowFiddlerRequestChange.BodyModific.ModifiedMode != ContentModifiedMode.NoChange)
                {
                    if (nowFiddlerRequestChange.BodyModific.ModifiedMode == ContentModifiedMode.HexReplace)
                    {
                        try
                        {
                            oSession.RequestBody =
                                nowFiddlerRequestChange.BodyModific.GetFinalContent(oSession.requestBodyBytes);
                        }
                        catch (Exception ex)
                        {
                            showError($"error in GetFinalContent in HexReplace with [{ex.Message}]");
                        }
                    }
                    else
                    {
                        string sourceRequestBody = null;
                        try
                        {
                            sourceRequestBody = oSession.GetRequestBodyAsString();
                        }
                        catch (Exception ex)
                        {
                            showError($"error in GetRequestBodyAsString [{ex.Message}]");
                            oSession.utilDecodeRequest();
                            sourceRequestBody = oSession.GetRequestBodyEncoding().GetString(oSession.requestBodyBytes);
                        }
                        finally
                        {
                            if (nowFiddlerRequestChange.BodyModific.ModifiedMode == ContentModifiedMode.ReCode)
                            {
                                try
                                {
                                    oSession.RequestBody =
                                        nowFiddlerRequestChange.BodyModific.GetRecodeContent(sourceRequestBody);
                                }
                                catch (Exception ex)
                                {
                                    showError($"error in GetRecodeContent in ReCode with [{ex.Message}]");
                                }
                            }
                            else
                            {
                                var tempRequestBody = nowFiddlerRequestChange.BodyModific.GetFinalContent(sourceRequestBody,
                                                                        nameValueCollection, out errMes);
                                if (errMes != null)
                                    showError($"error in GetFinalContent in BodyModified that [{errMes}]");
                                if (tempRequestBody != sourceRequestBody) oSession.utilSetRequestBody(tempRequestBody);
                            }
                        }
                    }
                }

                //Modified heads
                if (nowFiddlerRequestChange.HeadDelList != null && nowFiddlerRequestChange.HeadDelList.Count > 0)
                    foreach (var tempDelHead in nowFiddlerRequestChange.HeadDelList)
                        oSession.RequestHeaders.Remove(tempDelHead);
                if (nowFiddlerRequestChange.HeadAddList != null && nowFiddlerRequestChange.HeadAddList.Count > 0)
                    foreach (var tempAddHead in nowFiddlerRequestChange.HeadAddList)
                        if (tempAddHead.Contains(": "))
                            oSession.RequestHeaders.Add(tempAddHead.Remove(tempAddHead.IndexOf(": ", StringComparison.Ordinal)),
                                tempAddHead.Substring(tempAddHead.IndexOf(": ", StringComparison.Ordinal) + 2));
                        else
                            showError($"error to deal add head string with [{tempAddHead}]");
                //other action
                if (nameValueCollection != null && nameValueCollection.Count > 0)
                    showMes($"[ParametrizationContent]:{nameValueCollection.MyToFormatString()}");
            }
        }

        /// <summary>
        ///     Replace the http request in oSession with your rule (it may call by ModifiedSessionRequest)
        /// </summary>
        /// <param name="oSession">oSession</param>
        /// <param name="nowFiddlerRequsetChange">FiddlerRequsetChange</param>
        public static void ReplaceSessionRequest(Session oSession, FiddlerRequestChange nowFiddlerRequsetChange,
            Action<string> ShowError, Action<string> ShowMes)
        {
            string errMes;
            NameValueCollection nameValueCollection;
            HttpRequest tempRequestHttpRequest;
            try
            {
                tempRequestHttpRequest =
                    nowFiddlerRequsetChange.HttpRawRequest.UpdateHttpRequest(out errMes, out nameValueCollection);
            }
            catch (Exception ex)
            {
                ShowError(string.Format("Fail to ReplaceSessionResponse :{0}", ex.Message));
                return;
            }

            if (errMes != null) ShowError(errMes);
            if (nameValueCollection != null && nameValueCollection.Count > 0)
                ShowMes(string.Format("[ParameterizationContent]:{0}", nameValueCollection.MyToFormatString()));

            oSession.oRequest.headers = new HTTPRequestHeaders();
            oSession.RequestMethod = tempRequestHttpRequest.RequestMethod;
            try
            {
                oSession.fullUrl = tempRequestHttpRequest.RequestUri;
            }
            catch (ArgumentException ex)
            {
                if (ex.Message == "URI scheme must be http, https, or ftp")
                    oSession.url = tempRequestHttpRequest.RequestUri;
                else
                    ShowError(ex.Message);
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }

            oSession.RequestHeaders.HTTPVersion = tempRequestHttpRequest.RequestVersions;
            if (tempRequestHttpRequest.RequestHeads != null)
                foreach (var tempHead in tempRequestHttpRequest.RequestHeads)
                {
                    if (tempHead.Key == "Host") oSession.oRequest.headers.Remove("Host");
                    oSession.oRequest.headers.Add(tempHead.Key, tempHead.Value);
                }

            oSession.requestBodyBytes = tempRequestHttpRequest.RequestEntity;
        }

        /// <summary>
        ///     Modific the http response in oSession with your rule (if IsRawReplace and  IsIsDirectRespons it will not call
        ///     ReplaceSessionResponse)
        /// </summary>
        /// <param name="oSession">oSession</param>
        /// <param name="nowFiddlerResponseChange">FiddlerResponseChange</param>
        /// <param name="showError"></param>
        /// <param name="showMes"></param>
        public static void ModifiedSessionResponse(Session oSession, FiddlerResponseChange nowFiddlerResponseChange,
            Action<string> showError, Action<string> showMes)
        {
            if (nowFiddlerResponseChange.ParameterPickList != null &&
                nowFiddlerResponseChange.ParameterPickList.Count > 0)
                PickSessionParameter(oSession, nowFiddlerResponseChange, showError, showMes, false);
            if (nowFiddlerResponseChange.IsRawReplace)
            {
                //if IsIsDirectRespons do nothing
                if (!nowFiddlerResponseChange.IsIsDirectRespons)
                    ReplaceSessionResponse(oSession, nowFiddlerResponseChange, showError, showMes);
            }
            else
            {
                string errMes;
                var nameValueCollection = new NameValueCollection();
                //modific body
                if (nowFiddlerResponseChange.BodyModific != null &&
                    nowFiddlerResponseChange.BodyModific.ModifiedMode != ContentModifiedMode.NoChange)
                {
                    if (nowFiddlerResponseChange.BodyModific.ModifiedMode == ContentModifiedMode.HexReplace)
                    {
                        try
                        {
                            oSession.ResponseBody =
                                nowFiddlerResponseChange.BodyModific.GetFinalContent(oSession.responseBodyBytes);
                        }
                        catch (Exception ex)
                        {
                            showError($"error in GetFinalContent in HexReplace with [{ex.Message}]");
                        }
                    }
                    else
                    {
                        //you should not change the media data as string
                        string sourceResponseBody = null;
                        try
                        {
                            sourceResponseBody =
                                oSession
                                    .GetResponseBodyAsString(); //if the head encode is change ,GetResponseBodyAsString maybe fail
                            if (nowFiddlerResponseChange.BodyModific.ParameterTargetKey.ToString().Contains('\n'))
                                sourceResponseBody = sourceResponseBody.Replace("\r\n", "\n");
                        }
                        catch (Exception ex)
                        {
                            showError(string.Format("error in GetResponseBodyAsString [{0}]", ex.Message));
                            oSession.utilDecodeResponse();
                            sourceResponseBody = oSession.GetResponseBodyEncoding().GetString(oSession.ResponseBody);
                        }
                        finally
                        {
                            if (nowFiddlerResponseChange.BodyModific.ModifiedMode == ContentModifiedMode.ReCode)
                            {
                                try
                                {
                                    oSession.ResponseBody =
                                        nowFiddlerResponseChange.BodyModific.GetRecodeContent(sourceResponseBody);
                                }
                                catch (Exception ex)
                                {
                                    showError(string.Format("error in GetRecodeContent in ReCode with [{0}]",
                                        ex.Message));
                                }
                            }
                            else
                            {
                                //oSession.utilSetResponseBody(nowFiddlerResponseChange.BodyModific.GetFinalContent(sourceResponseBody));
                                var tempResponseBody =
                                    nowFiddlerResponseChange.BodyModific.GetFinalContent(sourceResponseBody,
                                        nameValueCollection, out errMes);
                                if (errMes != null)
                                    showError(string.Format("error in GetFinalContent in BodyModific that [{0}]",
                                        errMes));
                                if (tempResponseBody != sourceResponseBody)
                                    oSession.utilSetResponseBody(tempResponseBody);
                            }
                        }

                        //you can use below code to modific the body
                        //oSession.utilDecodeResponse();
                        //oSession.utilReplaceInResponse("","");
                        //oSession.utilDeflateResponse();
                    }
                }

                //modific heads
                if (nowFiddlerResponseChange.HeadDelList != null && nowFiddlerResponseChange.HeadDelList.Count > 0)
                    foreach (var tempDelHead in nowFiddlerResponseChange.HeadDelList)
                        oSession.ResponseHeaders.Remove(tempDelHead);
                if (nowFiddlerResponseChange.HeadAddList != null && nowFiddlerResponseChange.HeadAddList.Count > 0)
                    foreach (var tempAddHead in nowFiddlerResponseChange.HeadAddList)
                        if (tempAddHead.Contains(": "))
                            oSession.ResponseHeaders.Add(tempAddHead.Remove(tempAddHead.IndexOf(": ")),
                                tempAddHead.Substring(tempAddHead.IndexOf(": ") + 2));
                        else
                            showError(string.Format("error to deal add head string with [{0}]", tempAddHead));
                //other action
                if (nameValueCollection != null && nameValueCollection.Count > 0)
                    showMes(string.Format("[ParameterizationContent]:{0}", nameValueCollection.MyToFormatString()));
            }
        }

        /// <summary>
        ///     Replace the http response in oSession with your rule
        /// </summary>
        /// <param name="oSession">oSession</param>
        /// <param name="nowFiddlerResponseChange">FiddlerResponseChange</param>
        public static void ReplaceSessionResponse(Session oSession, FiddlerResponseChange nowFiddlerResponseChange,
            Action<string> ShowError, Action<string> ShowMes)
        {
            var isClosePipeWhenReplace = false;
            byte[] tempResponseBytes;
            string errMes;
            NameValueCollection nameValueCollection;
            HttpResponse tempHttpResponse;
            try
            {
                tempHttpResponse =
                    nowFiddlerResponseChange.HttpRawResponse.UpdateHttpResponse(out errMes, out nameValueCollection);
                tempResponseBytes = tempHttpResponse.GetRawHttpResponse();
            }
            catch (Exception ex)
            {
                ShowError(string.Format("Fail to ReplaceSessionResponse :{0}", ex.Message));
                return;
            }

            if (errMes != null) ShowError(errMes);
            if (nameValueCollection != null && nameValueCollection.Count > 0)
                ShowMes(string.Format("[ParameterizationContent]:{0}", nameValueCollection.MyToFormatString()));
            using (var ms = new MemoryStream(tempResponseBytes))
            {
                if (!oSession.LoadResponseFromStream(ms, null))
                {
                    ShowError("error to oSession.LoadResponseFromStream");
                    ShowError("try to modific the response");

                    //modific the response
                    oSession.oResponse.headers = new HTTPResponseHeaders();
                    oSession.oResponse.headers.HTTPResponseCode = tempHttpResponse.ResponseCode;
                    oSession.ResponseHeaders.StatusDescription = tempHttpResponse.ResponseStatusDescription;
                    oSession.ResponseHeaders.HTTPVersion = tempHttpResponse.ResponseVersion;
                    if (tempHttpResponse.ResponseHeads != null && tempHttpResponse.ResponseHeads.Count > 0)
                        foreach (var tempHead in tempHttpResponse.ResponseHeads)
                            oSession.oResponse.headers.Add(tempHead.Key, tempHead.Value);
                    oSession.responseBodyBytes = tempHttpResponse.ResponseEntity;
                }
            }
        }

        /// <summary>
        ///     Modific the websocket message with your rule
        /// </summary>
        /// <param name="oSession"></param>
        /// <param name="webSocketMessage"></param>
        /// <param name="ShowError"></param>
        /// <param name="ShowMes"></param>
        public static void ModifiedWebSocketMessage(Session oSession, WebSocketMessage webSocketMessage,
            IFiddlerHttpTamper nowFiddlerChange, bool isRequest, Action<string> ShowError, Action<string> ShowMes)
        {
            if (nowFiddlerChange.ParameterPickList != null)
                PickSessionParameter(oSession, nowFiddlerChange, ShowError, ShowMes, webSocketMessage.IsOutbound,
                    webSocketMessage);
            ParameterContentModific payLoadModified;
            if (isRequest)
            {
                var fiddlerRequestChange = (FiddlerRequestChange)nowFiddlerChange;
                payLoadModified = fiddlerRequestChange.BodyModific;
            }
            else
            {
                var nowFiddlerResponseChange = (FiddlerResponseChange)nowFiddlerChange;
                payLoadModified = nowFiddlerResponseChange.BodyModific;
            }

            //Modific body
            if (payLoadModified != null && payLoadModified.ModifiedMode != ContentModifiedMode.NoChange)
            {
                if (payLoadModified.ModifiedMode == ContentModifiedMode.HexReplace)
                {
                    try
                    {
                        webSocketMessage.SetPayload(payLoadModified.GetFinalContent(webSocketMessage.PayloadAsBytes()));
                    }
                    catch (Exception ex)
                    {
                        ShowError($"error in GetFinalContent in HexReplace with [{ex.Message}]");
                    }
                }
                else
                {
                    if (webSocketMessage.FrameType == WebSocketFrameTypes.Binary)
                    {
                        ShowError("error in GetFinalContent that WebSocketFrameTypes is Binary ,just use <hex> mode");
                    }
                    else if (webSocketMessage.FrameType == WebSocketFrameTypes.Ping ||
                             webSocketMessage.FrameType == WebSocketFrameTypes.Pong ||
                             webSocketMessage.FrameType == WebSocketFrameTypes.Close)
                    {
                        // do nothing
                    }
                    else
                    {
                        var sourcePayload = webSocketMessage.PayloadAsString();
                        if (payLoadModified.ModifiedMode == ContentModifiedMode.ReCode)
                        {
                            try
                            {
                                webSocketMessage.SetPayload(payLoadModified.GetRecodeContent(sourcePayload));
                            }
                            catch (Exception ex)
                            {
                                ShowError($"error in GetRecodeContent in ReCode with [{ex.Message}]");
                            }
                        }
                        else
                        {
                            string errMes;
                            var nameValueCollection = new NameValueCollection();
                            
                            var tempPayload =
                                payLoadModified.GetFinalContent(sourcePayload, nameValueCollection, out errMes);
                            if (errMes != null)
                                ShowError($"error in GetFinalContent in PayLoadModified that [{errMes}]");
                            if (tempPayload != sourcePayload) //非标准协议的实现，或没有实现的压缩会导致PayloadAsString()使数据不可逆
                                webSocketMessage.SetPayload(tempPayload);

                            if (nameValueCollection.Count > 0) ShowMes(
                                $"[ParametrizationContent]:{nameValueCollection.MyToFormatString()}");
                        }
                    }
                }
            }
        }

        public static void PickSessionParameter(Session oSession, IFiddlerHttpTamper nowFiddlerHttpTamper,
            Action<string> ShowError, Action<string> ShowMes, bool isRequest, WebSocketMessage webSocketMessage = null)
        {
            Func<string, ParameterPick, string> PickFunc = (sourceStr, parameterPick) =>
            {
                try
                {
                    return ParameterPickTypeEngine.dictionaryParameterPickFunc[parameterPick.PickType]
                        .ParameterPickFunc(sourceStr, parameterPick.PickTypeExpression,
                            parameterPick.PickTypeAdditional);
                }
                catch (Exception)
                {
                    return null;
                }
            };

            var isWebSocket = webSocketMessage != null;

            if (nowFiddlerHttpTamper.ParameterPickList != null)
                foreach (var parameterPick in nowFiddlerHttpTamper.ParameterPickList)
                {
                    string pickResult = null;
                    string pickSource = null;
                    switch (parameterPick.PickRange)
                    {
                        case ParameterPickRange.Line:
                            if (isRequest)
                            {
                                pickSource = oSession.fullUrl;
                                if (string.IsNullOrEmpty(pickSource))
                                {
                                    pickResult = null;
                                    break;
                                }
                            }
                            else
                            {
                                if (oSession.oResponse.headers == null)
                                {
                                    pickResult = null;
                                    break;
                                }

                                //pickSource = string.Format("{0} {1} {}", oSession.oResponse.headers.HTTPVersion, oSession.oResponse.headers.HTTPResponseCode,oSession.oResponse.headers.StatusDescription);
                                pickSource = string.Format("{0} {1}", oSession.oResponse.headers.HTTPVersion,
                                    oSession.oResponse.headers.HTTPResponseStatus);
                            }

                            pickResult = PickFunc(pickSource, parameterPick);
                            break;

                        case ParameterPickRange.Heads:
                            if (isWebSocket)
                            {
                                ShowError(
                                    "[ParameterizationPick] can not pick parameter in head when the session is websocket");
                                break;
                            }

                            var headerItems = isRequest
                                ? oSession.RequestHeaders
                                : (IEnumerable<HTTPHeaderItem>)oSession.ResponseHeaders;
                            foreach (var tempHead in headerItems)
                            {
                                pickResult = PickFunc(tempHead.ToString(), parameterPick);
                                if (pickResult != null) break;
                            }

                            break;

                        case ParameterPickRange.Entity:
                            if (isWebSocket)
                            {
                                if (webSocketMessage.PayloadLength == 0)
                                {
                                    pickResult = null;
                                    break;
                                }

                                pickSource = webSocketMessage.PayloadAsString();
                                pickResult = PickFunc(pickSource, parameterPick);
                            }
                            else
                            {
                                if ((oSession.requestBodyBytes == null || oSession.requestBodyBytes.Length == 0) &&
                                    isRequest && (oSession.ResponseBody == null || oSession.ResponseBody.Length == 0) &&
                                    isRequest)
                                {
                                    pickResult = null;
                                    break;
                                }

                                pickSource = isRequest
                                    ? oSession.GetRequestBodyAsString()
                                    : oSession.GetResponseBodyAsString();
                                pickResult = PickFunc(pickSource, parameterPick);
                            }

                            break;

                        default:
                            ShowError("[ParameterizationPick] unkonw pick range");
                            break;
                    }

                    if (pickResult == null)
                    {
                        ShowMes(string.Format("[ParameterizationPick] can not find the parameter with [{0}]",
                            parameterPick.ParameterName));
                    }
                    else
                    {
                        ShowMes(string.Format("[ParameterizationPick] pick the parameter [{0} = {1}]",
                            parameterPick.ParameterName, pickResult));
                        if (nowFiddlerHttpTamper.ActuatorStaticDataController.SetActuatorStaticData(
                                parameterPick.ParameterName, pickResult))
                            ShowMes(string.Format(
                                "[ParameterizationPick] add the parameter [{0}] to ActuatorStaticDataCollection",
                                parameterPick.ParameterName));
                        else
                            ShowError(string.Format(
                                "[ParameterizationPick] fail to add the parameter [{0}] to ActuatorStaticDataCollection",
                                parameterPick.ParameterName));
                    }
                }
            else
                ShowError("[ParameterizationPick] not find ParameterPick to pick");
        }

        public static string GetSessionRawData(Session oSession, bool isHaveResponse)
        {
            if (oSession == null) return null;
            var sbRawData = new StringBuilder();
            var ms = new MemoryStream();
            //tempSession.WriteToStream(SmartAssembly, false);
            oSession.WriteRequestToStream(true, true, ms);
            ms.Position = 0;
            var sr = new StreamReader(ms, Encoding.UTF8);
            sbRawData.Append(sr.ReadToEnd());
            sr.Close();
            ms.Close();

            if (oSession.requestBodyBytes != null && oSession.requestBodyBytes.Length > 0)
            {
                sbRawData.AppendLine(oSession.GetRequestBodyAsString());
                sbRawData.Append("\r\n");
            }

            if (isHaveResponse && oSession.bHasResponse)
            {
                sbRawData.AppendLine(oSession.ResponseHeaders.ToString());
                if (oSession.responseBodyBytes != null && oSession.responseBodyBytes.Length > 0)
                    sbRawData.AppendLine(oSession.GetResponseBodyAsString());
            }

            return sbRawData.ToString();
        }

        public static bool GetSessionData(Session oSession, FreeHttpWindow.GetSessionEventArgs sessionEventArgs)
        {
            if (sessionEventArgs == null || oSession == null) return false;
            sessionEventArgs.Uri = oSession.fullUrl;
            if (oSession.oRequest != null)
            {
                sessionEventArgs.RequestHeads =
                    new List<KeyValuePair<string, string>>(oSession.oRequest.headers.Count());
                foreach (var head in oSession.oRequest.headers)
                    sessionEventArgs.RequestHeads.Add(new KeyValuePair<string, string>(head.Name, head.Value));
                if (sessionEventArgs.IsGetEntity) sessionEventArgs.RequestEntity = oSession.GetRequestBodyAsString();
            }

            if (oSession.bHasResponse && oSession.oResponse != null)
            {
                sessionEventArgs.ResponseHeads =
                    new List<KeyValuePair<string, string>>(oSession.oResponse.headers.Count());
                foreach (var head in oSession.oResponse.headers)
                    sessionEventArgs.ResponseHeads.Add(new KeyValuePair<string, string>(head.Name, head.Value));
                if (sessionEventArgs.IsGetEntity) sessionEventArgs.ResponseEntity = oSession.GetResponseBodyAsString();
            }

            return true;
        }
    }
}