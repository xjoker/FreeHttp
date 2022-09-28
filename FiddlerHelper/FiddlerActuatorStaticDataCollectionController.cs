using FreeHttp.AutoTest.RunTimeStaticData;
using FreeHttp.AutoTest.RunTimeStaticData.MyStaticData;

namespace FreeHttp.FiddlerHelper
{
    public class FiddlerActuatorStaticDataCollectionController
    {
        public ActuatorStaticDataCollection actuatorStaticDataCollection;

        public FiddlerActuatorStaticDataCollectionController(ActuatorStaticDataCollection yourStaticDataCollection)
        {
            actuatorStaticDataCollection = yourStaticDataCollection;
        }

        public void SetActuatorStaticDataCollection(ActuatorStaticDataCollection yourStaticDataCollection)
        {
            actuatorStaticDataCollection = yourStaticDataCollection;
        }

        public bool SetActuatorStaticData(string key, string value)
        {
            if (actuatorStaticDataCollection == null) return false;
            var nowStaticData = actuatorStaticDataCollection[key];
            if (nowStaticData != null)
            {
                nowStaticData.DataMoveNext();
                return actuatorStaticDataCollection.SetStaticDataValue(key, value);
            }

            return actuatorStaticDataCollection.AddStaticDataKey(key, new MyStaticDataValue(value));
        }
    }
}