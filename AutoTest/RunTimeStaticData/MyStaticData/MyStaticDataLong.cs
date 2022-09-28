using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace FreeHttp.AutoTest.RunTimeStaticData.MyStaticData
{
    /// <summary>
    ///     为StaticData提供长数字索引支持【IRunTimeStaticData】
    /// </summary>
    [DataContract]
    public class MyStaticDataLong : IRunTimeStaticData
    {
        [DataMember] private long dataIndex;

        [DataMember] private long defaultEnd;

        [DataMember] private long defaultStart;

        [DataMember] private long defaultStep;

        [DataMember] private bool isNew;

        public MyStaticDataLong(long yourStart, long yourEnd, long yourStep)
        {
            isNew = true;
            dataIndex = defaultStart = yourStart;
            defaultEnd = yourEnd;
            defaultStep = yourStep;
        }

        public MyStaticDataLong(long yourStart, long yourEnd, long yourStep, string originalConnectString)
            : this(yourStart, yourEnd, yourStep)
        {
            OriginalConnectString = originalConnectString;
        }

        [DataMember] public string OriginalConnectString { get; private set; }

        public string RunTimeStaticDataTypeAlias => "staticData_long";

        public CaseStaticDataType RunTimeStaticDataType => CaseStaticDataType.caseStaticData_long;

        public object Clone()
        {
            return new MyStaticDataLong(defaultStart, defaultEnd, defaultStep);
        }


        public string DataCurrent()
        {
            return dataIndex.ToString();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public string DataMoveNext()
        {
            if (isNew)
            {
                isNew = false;
                return dataIndex.ToString();
            }

            if (dataIndex >= defaultEnd)
            {
                DataReset();
                return DataMoveNext();
            }

            //lock(this)
            //{
            dataIndex += defaultStep;
            //}
            return dataIndex.ToString();
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
    }
}