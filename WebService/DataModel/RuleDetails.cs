using System.Collections.Generic;
using System.Runtime.Serialization;
using FreeHttp.AutoTest.RunTimeStaticData;
using FreeHttp.FiddlerHelper;

namespace FreeHttp.WebService.DataModel
{
    [DataContract]
    public class RuleDetails
    {
        public RuleDetails()
        {
            RequestRuleCells = new List<RuleCell>();
            ResponseRuleCells = new List<RuleCell>();
        }


        [DataMember] public List<RuleCell> RequestRuleCells { get; set; }

        [DataMember] public List<RuleCell> ResponseRuleCells { get; set; }

        [DataMember] public RuleCell RuleStaticDataCell { get; set; }

        [DataMember] public RuleCell RuleGroupCell { get; set; }

        /// <summary>
        ///     备注 只要Share RuleDetails 才会有
        /// </summary>
        [DataMember]
        public string Remark { get; set; }

        public FiddlerModificHttpRuleCollection ModificHttpRuleCollection { get; set; }

        public ActuatorStaticDataCollection StaticDataCollection { get; set; }

        public FiddlerRuleGroup RuleGroup { get; set; }

        [DataContract]
        public class RuleCell
        {
            [DataMember] public string RuleContent { get; set; }

            [DataMember] public string RuleVersion { get; set; }
        }
    }
}