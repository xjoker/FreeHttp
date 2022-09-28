using System;
using System.Collections.Specialized;
using System.Runtime.Serialization;
using FreeHttp.AutoTest.ParameterizationContent;
using FreeHttp.AutoTest.RunTimeStaticData;

namespace FreeHttp.HttpHelper
{
    [Serializable]
    [DataContract]
    public class ParameterHttpResponse : HttpResponse
    {
        [NonSerialized] private ActuatorStaticDataCollection actuatorStaticDataCollection;

        public ParameterHttpResponse(string yourResponse, bool isParameter)
        {
            ParameterizationContent = new CaseParameterizationContent(yourResponse, isParameter);
            ParameterizationContent.encodetype = ParameterizationContentEncodingType.encode_default;
            OriginSting = yourResponse;
        }

        public ParameterHttpResponse()
        {
            actuatorStaticDataCollection = null;
        }

        private ParameterHttpResponse(HttpResponse httpResponse)
        {
            ResponseLine = httpResponse.ResponseLine;
            ResponseHeads = httpResponse.ResponseHeads;
            ResponseEntity = httpResponse.ResponseEntity;
            OriginSting = httpResponse.OriginSting;
        }

        [DataMember] public CaseParameterizationContent ParameterizationContent { get; set; }

        public void SetUseParameterInfo(bool isUseParameter, ActuatorStaticDataCollection yourStaticDataCollection)
        {
            ParameterizationContent.hasParameter = isUseParameter;
            actuatorStaticDataCollection = yourStaticDataCollection;
        }

        public new byte[] GetRawHttpResponse()
        {
            return base.GetRawHttpResponse();
        }

        public byte[] GetRawHttpResponse(out string errorMes, out NameValueCollection nameValueCollection)
        {
            nameValueCollection = null;
            errorMes = null;
            if (ParameterizationContent.hasParameter)
            {
                nameValueCollection = new NameValueCollection();
                var newOriginSting = ParameterizationContent.GetTargetContentData(actuatorStaticDataCollection,
                    nameValueCollection, out errorMes);
                var tempHttpResponse = GetHttpResponse(newOriginSting);
                //tempHttpResponse.SetAutoContentLength();
                return tempHttpResponse.GetRawHttpResponse();
            }

            return base.GetRawHttpResponse();
        }

        public HttpResponse UpdateHttpResponse(out string errorMes, out NameValueCollection nameValueCollection)
        {
            nameValueCollection = null;
            errorMes = null;
            if (ParameterizationContent.hasParameter)
            {
                nameValueCollection = new NameValueCollection();
                var newOriginSting = ParameterizationContent.GetTargetContentData(actuatorStaticDataCollection,
                    nameValueCollection, out errorMes);
                var tempHttpResponse = GetHttpResponse(newOriginSting);
                tempHttpResponse.SetAutoContentLength(); // if hasParameter SetAutoContentLength
                return tempHttpResponse;
            }

            return this;
        }

        public static ParameterHttpResponse GetHttpResponse(string yourResponse, bool isParameter)
        {
            ParameterHttpResponse returnPrameterHttpResponse;
            returnPrameterHttpResponse = new ParameterHttpResponse(GetHttpResponse(yourResponse));
            returnPrameterHttpResponse.ParameterizationContent =
                new CaseParameterizationContent(yourResponse, isParameter);
            return returnPrameterHttpResponse;
        }

        public static ParameterHttpResponse GetHttpResponse(string yourResponse, bool isParameter,
            ActuatorStaticDataCollection yourActuatorStaticDataCollection)
        {
            var returnPrameterHttpResponse = GetHttpResponse(yourResponse, isParameter);
            //returnPrameterHttpResponse.actuatorStaticDataCollection = yourActuatorStaticDataCollection;
            returnPrameterHttpResponse.SetUseParameterInfo(isParameter, yourActuatorStaticDataCollection);
            return returnPrameterHttpResponse;
        }
    }
}