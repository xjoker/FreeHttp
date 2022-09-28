using System.Collections.Generic;

namespace FreeHttp.WebService.DataModel
{
    public class ShareRuleSummary
    {
        public List<RuleToken> ShareRuleList { get; set; }
        public List<RuleToken> PrivateRuleList { get; set; }

        public class RuleToken
        {
            public string Token { get; set; }
            public string Remark { get; set; }

            public string ShowTag => $"...{Token.Substring(Token.Length > 22 ? 22 : 0)} [{Remark ?? "-"}]";

            public string ShowWholeTag => $"{Token} [{Remark ?? "-"}]";
        }
    }
}