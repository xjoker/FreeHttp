﻿using FreeHttp.AutoTest.RunTimeStaticData;
using FreeHttp.FiddlerHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace FreeHttp.WebService.DataModel
{
    [DataContract]
    public class RuleDetails
    {
        [DataContract]

        public class RuleCell
        {
            public RuleCell() { }

            [DataMember]
            public string RuleContent { get; set; }
            [DataMember]
            public string RuleVersion { get; set; }
        }

        public RuleDetails()
        {
            RequestRuleCells = new List<RuleCell>();
            ResponseRuleCells = new List<RuleCell>();
        }


        [DataMember]
        public List<RuleCell> RequestRuleCells { get; set; }

        [DataMember]
        public List<RuleCell> ResponseRuleCells { get; set; }

        [DataMember]
        public RuleCell RuleStaticData { get; set; }

        public FiddlerModificHttpRuleCollection ModificHttpRuleCollection { get; set; }

        public ActuatorStaticDataCollection StaticDataCollection { get; set; }

    }
}
