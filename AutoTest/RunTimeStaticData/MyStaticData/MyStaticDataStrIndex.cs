using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace FreeHttp.AutoTest.RunTimeStaticData.MyStaticData
{
    /// <summary>
    ///     为StaticData提定长字符串型数字索引支持【IRunTimeStaticData】
    /// </summary>
    [DataContract]
    public class MyStaticDataStrIndex : IRunTimeStaticData
    {
        [DataMember] private long dataIndex;

        [DataMember] private long defaultEnd;

        [DataMember] private long defaultStart;

        [DataMember] private long defaultStep;

        [DataMember] private bool isNew;

        [DataMember] private int strLen;

        public MyStaticDataStrIndex(long yourStart, long yourEnd, long yourStep, int yourStrLen)
        {
            isNew = true;
            dataIndex = defaultStart = yourStart;
            defaultEnd = yourEnd;
            defaultStep = yourStep;
            strLen = yourStrLen;
        }

        public MyStaticDataStrIndex(long yourStart, long yourEnd, long yourStep, int yourStrLen,
            string originalConnectString)
            : this(yourStart, yourEnd, yourStep, yourStrLen)
        {
            OriginalConnectString = originalConnectString;
        }

        [DataMember] public string OriginalConnectString { get; private set; }

        public string RunTimeStaticDataTypeAlias => "staticData_strIndex";

        public CaseStaticDataType RunTimeStaticDataType => CaseStaticDataType.caseStaticData_strIndex;

        public object Clone()
        {
            return new MyStaticDataStrIndex(defaultStart, defaultEnd, defaultStep, strLen);
        }

        public string DataCurrent()
        {
            return GetLenStr(dataIndex);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public string DataMoveNext()
        {
            if (isNew)
            {
                isNew = false;
                return GetLenStr(dataIndex);
            }

            if (dataIndex >= defaultEnd)
            {
                DataReset();
                return DataMoveNext();
            }

            dataIndex += defaultStep;
            return GetLenStr(dataIndex);
        }


        public void DataReset()
        {
            isNew = true;
            dataIndex = defaultStart;
        }


        public bool DataSet(string expectData)
        {
            long tempData;
            if (long.TryParse(expectData, out tempData))
                if (tempData >= defaultStart && tempData <= defaultEnd)
                {
                    dataIndex = tempData;
                    return true;
                }

            return false;
        }

        private string GetLenStr(long yourLeng)
        {
            var outStr = yourLeng.ToString();
            var distinction = strLen - outStr.Length;
            if (distinction > 0)
                for (var i = 0; i < distinction; i++)
                    outStr = "0" + outStr;
            return outStr;
        }
    }
}