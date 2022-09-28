using System;
using System.Runtime.Serialization;

namespace FreeHttp.AutoTest.RunTimeStaticData.MyStaticData
{
    /// <summary>
    ///     为StaticData提供当前时间的动态数据【IRunTimeStaticData】
    /// </summary>
    [DataContract]
    public class MyStaticDataNowTime : IRunTimeStaticData
    {
        [DataMember] private string myDataFormatInfo;

        [DataMember] private string myNowStr;

        [DataMember] private int timestampFormatdividend;

        public MyStaticDataNowTime(string yourRormatInfo)
        {
            myNowStr = "";
            if (int.TryParse(yourRormatInfo, out timestampFormatdividend))
            {
                if (timestampFormatdividend <= 0)
                {
                    timestampFormatdividend = 0;
                    myDataFormatInfo = "";
                }
            }
            else
            {
                myDataFormatInfo = yourRormatInfo;
            }
        }

        public MyStaticDataNowTime(string yourRormatInfo, string originalConnectString)
            : this(yourRormatInfo)
        {
            OriginalConnectString = originalConnectString;
        }

        [DataMember] public string OriginalConnectString { get; private set; }

        public string RunTimeStaticDataTypeAlias => "staticData_time";

        public CaseStaticDataType RunTimeStaticDataType => CaseStaticDataType.caseStaticData_time;

        public object Clone()
        {
            return new MyStaticDataNowTime(myDataFormatInfo);
        }

        public string DataCurrent()
        {
            return myNowStr;
        }

        public string DataMoveNext()
        {
            if (timestampFormatdividend == 0)
                myNowStr = DateTime.Now.ToString(myDataFormatInfo);
            else
                myNowStr = ((DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / timestampFormatdividend)
                    .ToString();
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