using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace FreeHttp.AutoTest.ParameterizationPick
{
    [Serializable]
    [DataContract]
    public class ParameterPick
    {
        [DataMember] public string ParameterName { get; set; }

        [DataMember] public ParameterPickType PickType { get; set; }

        [DataMember] public ParameterPickRange PickRange { get; set; }

        [DataMember] public string PickTypeAdditional { get; set; }

        [DataMember] public string PickTypeExpression { get; set; }

        public override string ToString()
        {
            return string.Format("get [{0}] from [{1}] by [{2} grep]({3}) with [{4}]", ParameterName,
                PickRange.ToString(), PickType.ToString(), PickTypeAdditional, PickTypeExpression);
        }
    }

    public class ParameterPickInfo
    {
        public ParameterPickInfo(ParameterPickType pickType, List<KeyValuePair<string, string>> pickTypeAdditionalList,
            bool editable, Func<string, string, string, string> parameterPickFunc)
        {
            PickType = pickType;
            PickTypeAdditionalList = pickTypeAdditionalList;
            Editable = editable;
            ParameterPickFunc = parameterPickFunc;
        }

        public ParameterPickType PickType { get; }
        public List<KeyValuePair<string, string>> PickTypeAdditionalList { get; }
        public bool Editable { get; }
        public Func<string, string, string, string> ParameterPickFunc { get; }
    }

    public class ParameterPickTypeEngine
    {
        public static Dictionary<ParameterPickType, ParameterPickInfo> dictionaryParameterPickFunc =
            new Dictionary<ParameterPickType, ParameterPickInfo>
            {
                {
                    ParameterPickType.Str,
                    new ParameterPickInfo(ParameterPickType.Str,
                        new List<KeyValuePair<string, string>>
                        {
                            new KeyValuePair<string, string>("str-str", "StartString-EndString"),
                            new KeyValuePair<string, string>("str-len", "StartString-StringLength"),
                            new KeyValuePair<string, string>("index-len", "StartIndex-StringLength")
                        }, false, ParameterPickHelper.ParameterPickStr)
                },
                {
                    ParameterPickType.Regex,
                    new ParameterPickInfo(ParameterPickType.Regex,
                        new List<KeyValuePair<string, string>>
                        {
                            new KeyValuePair<string, string>("1", "RegexExpression"),
                            new KeyValuePair<string, string>("0", "RegexExpression")
                        }, true, ParameterPickHelper.ParameterPickRegex)
                },
                {
                    ParameterPickType.Xml,
                    new ParameterPickInfo(ParameterPickType.Xml,
                        new List<KeyValuePair<string, string>>
                        {
                            new KeyValuePair<string, string>("1", "XpathExpression"),
                            new KeyValuePair<string, string>("0", "XpathExpression")
                        }, true, ParameterPickHelper.ParameterPickXml)
                }
            };
    }
}