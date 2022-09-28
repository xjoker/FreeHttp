using System;
using System.Collections.Specialized;
using System.Runtime.Serialization;
using FreeHttp.AutoTest.ParameterizationContent;
using FreeHttp.AutoTest.RunTimeStaticData;

namespace FreeHttp.HttpHelper
{
    [Serializable]
    [DataContract]
    public class ParameterHttpRequest : HttpRequest
    {
        [NonSerialized] private ActuatorStaticDataCollection actuatorStaticDataCollection;

        public ParameterHttpRequest()
        {
            actuatorStaticDataCollection = null;
        }

        public ParameterHttpRequest(string yourRequest, bool isParameter)
        {
            ParameterizationContent = new CaseParameterizationContent(yourRequest, isParameter);
            ParameterizationContent.encodetype = ParameterizationContentEncodingType.encode_default;
            OriginSting = yourRequest;
        }

        private ParameterHttpRequest(HttpRequest httpRequest)
        {
            RequestLine = httpRequest.RequestLine;
            RequestHeads = httpRequest.RequestHeads;
            RequestEntity = httpRequest.RequestEntity;
            OriginSting = httpRequest.OriginSting;
        }

        [DataMember] public CaseParameterizationContent ParameterizationContent { get; set; }

        public void SetUseParameterInfo(bool isUseParameter, ActuatorStaticDataCollection yourStaticDataCollection)
        {
            ParameterizationContent.hasParameter = isUseParameter;
            actuatorStaticDataCollection = yourStaticDataCollection;
        }

        public new byte[] GetRawHttpRequest()
        {
            return base.GetRawHttpRequest();
        }

        public byte[] GetRawHttpRequest(out string errorMes, out NameValueCollection nameValueCollection)
        {
            nameValueCollection = null;
            errorMes = null;
            if (ParameterizationContent.hasParameter)
            {
                nameValueCollection = new NameValueCollection();
                var newOriginSting = ParameterizationContent.GetTargetContentData(actuatorStaticDataCollection,
                    nameValueCollection, out errorMes);
                var tempHttpRequest = GetHttpRequest(newOriginSting);
                //tempHttpRequest.SetAutoContentLength();
                return tempHttpRequest.GetRawHttpRequest();
            }

            return base.GetRawHttpRequest();
        }

        public HttpRequest UpdateHttpRequest(out string errorMes, out NameValueCollection nameValueCollection)
        {
            nameValueCollection = null;
            errorMes = null;
            if (ParameterizationContent.hasParameter)
            {
                nameValueCollection = new NameValueCollection();
                var newOriginSting = ParameterizationContent.GetTargetContentData(actuatorStaticDataCollection,
                    nameValueCollection, out errorMes);
                var tempHttpRequest = GetHttpRequest(newOriginSting); // it may throw exception
                tempHttpRequest.SetAutoContentLength(); // if hasParameter SetAutoContentLength
                return tempHttpRequest;
            }

            return this;
        }

        public static ParameterHttpRequest GetHttpRequest(string yourRequest, bool isParameter)
        {
            ParameterHttpRequest returnPrameterHttpRequest;
            returnPrameterHttpRequest = new ParameterHttpRequest(GetHttpRequest(yourRequest));
            returnPrameterHttpRequest.ParameterizationContent =
                new CaseParameterizationContent(yourRequest, isParameter);
            return returnPrameterHttpRequest;
        }

        public static ParameterHttpRequest GetHttpRequest(string yourRequest, bool isParameter,
            ActuatorStaticDataCollection yourActuatorStaticDataCollection)
        {
            var returnPrameterHttpRequest = GetHttpRequest(yourRequest, isParameter);
            //returnPrameterHttpRequest.actuatorStaticDataCollection = yourActuatorStaticDataCollection;
            returnPrameterHttpRequest.SetUseParameterInfo(isParameter, yourActuatorStaticDataCollection);
            return returnPrameterHttpRequest;
        }
    }
}