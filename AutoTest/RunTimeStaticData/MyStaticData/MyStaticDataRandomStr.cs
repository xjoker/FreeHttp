using System.Runtime.Serialization;

namespace FreeHttp.AutoTest.RunTimeStaticData.MyStaticData
{
    /// <summary>
    ///     为StaticData提供随机字符串动态数据【IRunTimeStaticData】
    /// </summary>
    [DataContract]
    public class MyStaticDataRandomStr : IRunTimeStaticData
    {
        [DataMember] private string myNowStr;

        [DataMember] private int myStrNum;

        [DataMember] private int myStrType;

        public MyStaticDataRandomStr(int yourStrNum, int yourStrType)
        {
            myNowStr = "";
            myStrNum = yourStrNum;
            myStrType = yourStrType;
        }

        public MyStaticDataRandomStr(int yourStrNum, int yourStrType, string originalConnectString)
            : this(yourStrNum, yourStrType)
        {
            OriginalConnectString = originalConnectString;
        }

        [DataMember] public string OriginalConnectString { get; private set; }

        public string RunTimeStaticDataTypeAlias => "staticData_random";

        public CaseStaticDataType RunTimeStaticDataType => CaseStaticDataType.caseStaticData_random;

        public object Clone()
        {
            return new MyStaticDataRandomStr(myStrNum, myStrType);
        }

        public string DataCurrent()
        {
            return myNowStr;
        }

        public string DataMoveNext()
        {
            myNowStr = MyCommonTool.GenerateRandomStr(myStrNum, myStrType);
            return myNowStr;
        }

        public void DataReset()
        {
            myNowStr = "";
        }


        public bool DataSet(string expectData)
        {
            if (expectData != null)
            {
                myNowStr = expectData;
                return true;
            }

            return false;
        }
    }
}