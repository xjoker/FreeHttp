using System;
using System.Collections.Generic;

namespace FreeHttp.FiddlerHelper
{
    [Serializable]
    public class FiddlerModificHttpRuleCollection
    {
        private List<FiddlerRequestChange> requestRuleList;
        private List<FiddlerResponseChange> responseRuleList;


        public FiddlerModificHttpRuleCollection() // Serializable 需要空参数的构造函数
        {
            requestRuleList = null;
            responseRuleList = null;
        }

        public FiddlerModificHttpRuleCollection(List<FiddlerRequestChange> yourRequestRuleList,
            List<FiddlerResponseChange> yourResponseRuleList)
        {
            requestRuleList = yourRequestRuleList;
            responseRuleList = yourResponseRuleList;
        }

        public List<FiddlerRequestChange> RequestRuleList
        {
            get => requestRuleList;
            set => requestRuleList = value;
        }

        public List<FiddlerResponseChange> ResponseRuleList
        {
            get => responseRuleList;
            set => responseRuleList = value;
        }
    }
}