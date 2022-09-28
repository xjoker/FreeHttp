using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using FreeHttp.AutoTest.ParameterizationContent;
using FreeHttp.HttpHelper;

//using FiddlerRequsetChange = FreeHttp.FiddlerHelper.FiddlerRequestChange;


namespace FreeHttp.FiddlerHelper.VersionControlV1
{
    [Serializable]
    public class FiddlerModificHttpRuleCollection
    {
        private List<FiddlerRequsetChange> requestRuleList;
        private List<FiddlerResponseChange> responseRuleList;


        public FiddlerModificHttpRuleCollection() // Serializable 需要空参数的构造函数
        {
            requestRuleList = null;
            responseRuleList = null;
        }

        public FiddlerModificHttpRuleCollection(List<FiddlerRequsetChange> yourRequestRuleList,
            List<FiddlerResponseChange> yourResponseRuleList)
        {
            requestRuleList = yourRequestRuleList;
            responseRuleList = yourResponseRuleList;
        }

        //因为V1.0-V1.3 版本 FiddlerRequsetChange 这个英文拼错了 这里进行进行升级修复
        public List<FiddlerRequsetChange> RequestRuleList
        {
            get => requestRuleList;
            set => requestRuleList = value;
        }

        public List<FiddlerResponseChange> ResponseRuleList
        {
            get => responseRuleList;
            set => responseRuleList = value;
        }

        public static explicit operator FiddlerHelper.FiddlerModificHttpRuleCollection(
            FiddlerModificHttpRuleCollection fiddlerModificHttpRuleCollectionV1)
        {
            var RequestRuleList = new List<FiddlerRequestChange>();
            if (fiddlerModificHttpRuleCollectionV1.RequestRuleList != null &&
                fiddlerModificHttpRuleCollectionV1.RequestRuleList.Count > 0)
                foreach (var item in fiddlerModificHttpRuleCollectionV1.RequestRuleList)
                    RequestRuleList.Add(item.GetBase());
            var fiddlerModificHttpRuleCollection = new FiddlerHelper.FiddlerModificHttpRuleCollection(RequestRuleList,
                fiddlerModificHttpRuleCollectionV1.ResponseRuleList);

            if (fiddlerModificHttpRuleCollection.RequestRuleList != null &&
                fiddlerModificHttpRuleCollection.RequestRuleList.Count > 0)
                foreach (var item in fiddlerModificHttpRuleCollection.RequestRuleList)
                {
                    if (item.UriModific != null && item.UriModific.ModifiedMode != ContentModifiedMode.NoChange)
                    {
                        item.UriModific.ParameterReplaceContent =
                            new CaseParameterizationContent(item.UriModific.ReplaceContent);
                        item.UriModific.ParameterTargetKey = new CaseParameterizationContent(item.UriModific.TargetKey);
                    }

                    if (item.BodyModific != null && item.BodyModific.ModifiedMode != ContentModifiedMode.NoChange)
                    {
                        item.BodyModific.ParameterReplaceContent =
                            new CaseParameterizationContent(item.BodyModific.ReplaceContent);
                        item.BodyModific.ParameterTargetKey =
                            new CaseParameterizationContent(item.BodyModific.TargetKey);
                    }

                    if (item.IsRawReplace && item.HttpRawRequest.ParameterizationContent.hasParameter)
                        item.IsHasParameter = true;
                    //item.SetHasParameter(true);
                }

            if (fiddlerModificHttpRuleCollection.ResponseRuleList != null &&
                fiddlerModificHttpRuleCollection.ResponseRuleList.Count > 0)
                foreach (var item in fiddlerModificHttpRuleCollection.ResponseRuleList)
                {
                    if (item.BodyModific != null && item.BodyModific.ModifiedMode != ContentModifiedMode.NoChange)
                    {
                        item.BodyModific.ParameterReplaceContent =
                            new CaseParameterizationContent(item.BodyModific.ReplaceContent);
                        item.BodyModific.ParameterTargetKey =
                            new CaseParameterizationContent(item.BodyModific.TargetKey);
                    }

                    if (item.IsRawReplace && item.HttpRawResponse.ParameterizationContent.hasParameter)
                        item.IsHasParameter = true;
                    //item.SetHasParameter(true);
                }

            return fiddlerModificHttpRuleCollection;
        }
    }

    [Serializable]
    [DataContract]
    public class FiddlerRequsetChange : FiddlerRequestChange
    {
        public FiddlerRequestChange GetBase()
        {
            var fiddlerRequestChange = new FiddlerRequestChange();
            fiddlerRequestChange.IsEnable = IsEnable;
            fiddlerRequestChange.TamperProtocol = TamperProtocol;
            fiddlerRequestChange.HttpFilter = HttpFilter;
            fiddlerRequestChange.ParameterPickList = ParameterPickList;
            fiddlerRequestChange.HttpRawRequest = HttpRawRequest;
            fiddlerRequestChange.UriModific = UriModific;
            fiddlerRequestChange.HeadAddList = HeadAddList;
            fiddlerRequestChange.HeadDelList = HeadDelList;
            fiddlerRequestChange.BodyModific = BodyModific;
            fiddlerRequestChange.Tag = Tag;
            fiddlerRequestChange.ActuatorStaticDataController = ActuatorStaticDataController;
            return fiddlerRequestChange;
        }
    }
}