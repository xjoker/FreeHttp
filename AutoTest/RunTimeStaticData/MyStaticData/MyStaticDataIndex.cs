using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace FreeHttp.AutoTest.RunTimeStaticData.MyStaticData
{
    /// <summary>
    ///     为StaticData提供类似索引递增的动态数据【IRunTimeStaticData】
    /// </summary>
    [DataContract]
    public class MyStaticDataIndex : IRunTimeStaticData
    {
        [DataMember] private int dataIndex;

        [DataMember] private int defaultEnd;

        [DataMember] private int defaultStart;

        [DataMember] private int defaultStep;

        [DataMember] private bool isNew;

        public MyStaticDataIndex(int yourStart, int yourEnd, int yourStep)
        {
            isNew = true;
            dataIndex = defaultStart = yourStart;
            defaultEnd = yourEnd;
            defaultStep = yourStep;
        }

        public MyStaticDataIndex(int yourStart, int yourEnd, int yourStep, string originalConnectString)
            : this(yourStart, yourEnd, yourStep)
        {
            OriginalConnectString = originalConnectString;
        }

        [DataMember] public string OriginalConnectString { get; private set; }

        public string RunTimeStaticDataTypeAlias => "staticData_index";

        public CaseStaticDataType RunTimeStaticDataType => CaseStaticDataType.caseStaticData_index;

        public object Clone()
        {
            return new MyStaticDataIndex(defaultStart, defaultEnd, defaultStep);
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

            dataIndex += defaultStep;
            return dataIndex.ToString();
        }


        public void DataReset()
        {
            isNew = true;
            dataIndex = defaultStart;
        }


        public bool DataSet(string expectData)
        {
            int tempData;
            if (int.TryParse(expectData, out tempData))
                if (tempData >= defaultStart && tempData <= defaultEnd)
                {
                    dataIndex = tempData;
                    return true;
                }

            return false;
        }
    }
}