using System.Runtime.Serialization;

namespace FreeHttp.AutoTest.RunTimeStaticData.MyStaticData
{
    [DataContract]
    public class MyStaticDataValue : IRunTimeStaticData
    {
        [DataMember] private string defaultValue;


        public MyStaticDataValue(string yourVaule)
        {
            defaultValue = OriginalConnectString = yourVaule;
        }

        [DataMember] public string OriginalConnectString { get; private set; }


        public string RunTimeStaticDataTypeAlias => "staticData_value";

        public CaseStaticDataType RunTimeStaticDataType => CaseStaticDataType.caseStaticData_vaule;


        public object Clone()
        {
            return new MyStaticDataValue(defaultValue);
        }


        public string DataCurrent()
        {
            return defaultValue;
        }

        public string DataMoveNext()
        {
            return defaultValue;
        }


        public void DataReset()
        {
        }


        public bool DataSet(string expectData)
        {
            if (expectData != null)
            {
                defaultValue = expectData;
                return true;
            }

            return false;
        }
    }
}