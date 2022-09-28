using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using FreeHttp.AutoTest.ParameterizationPick;
using FreeHttp.AutoTest.RunTimeStaticData;
using FreeHttp.HttpHelper;
using FreeHttp.MyHelper;

namespace FreeHttp.FiddlerHelper
{
    [Serializable]
    [DataContract]
    public class FiddlerResponseChange : IFiddlerHttpTamper
    {
        private string _uid;

        [DataMember] public ParameterHttpResponse HttpRawResponse { get; set; }

        [DataMember] public bool IsIsDirectRespons { get; set; } //only for HttpRawResponse

        [DataMember] public int ResponseLatency { get; set; }

        [DataMember] public List<string> HeadAddList { get; set; }

        [DataMember] public List<string> HeadDelList { get; set; }

        [DataMember] public ParameterContentModific BodyModific { get; set; }

        //[NonSerialized]
        [XmlIgnore] public object Tag { get; set; }

        /// <summary>
        ///     get rule uid (not set this vaule in your business code)
        /// </summary>
        [DataMember]
        public string RuleUid
        {
            get
            {
                if (_uid == null) _uid = Guid.NewGuid().ToString("D");
                return _uid;
            }
            set => _uid = value;
        }

        [DataMember] public bool IsEnable { get; set; }

        [DataMember] public bool IsHasParameter { get; set; }

        [DataMember] public TamperProtocalType TamperProtocol { get; set; }

        [DataMember] public FiddlerHttpFilter HttpFilter { get; set; }

        [DataMember] public List<ParameterPick> ParameterPickList { get; set; }

        [XmlIgnore] public FiddlerActuatorStaticDataCollectionController ActuatorStaticDataController { get; set; }

        public bool IsRawReplace => HttpRawResponse != null;

        public object Clone()
        {
            var cloneFiddlerResponseChange = this.MyDeepClone();
            cloneFiddlerResponseChange?.SetHasParameter(IsHasParameter,
                ActuatorStaticDataController?.actuatorStaticDataCollection);
            return cloneFiddlerResponseChange;
        }

        public void SetHasParameter(bool hasParameter, ActuatorStaticDataCollection staticDataController = null)
        {
            if (staticDataController != null)
                ActuatorStaticDataController = new FiddlerActuatorStaticDataCollectionController(staticDataController);
            IsHasParameter = hasParameter;

            if (IsRawReplace)
            {
                if (HttpRawResponse != null)
                    HttpRawResponse.SetUseParameterInfo(IsHasParameter,
                        ActuatorStaticDataController.actuatorStaticDataCollection);
            }
            else
            {
                if (BodyModific != null && BodyModific.ModifiedMode != ContentModifiedMode.NoChange)
                    BodyModific.SetUseParameterInfo(IsHasParameter,
                        ActuatorStaticDataController.actuatorStaticDataCollection);
            }
        }
    }
}