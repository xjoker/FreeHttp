using System;
using System.Collections.Generic;
using FreeHttp.FiddlerHelper;

namespace FreeHttp.MyHelper
{
    public static class MyExtensionMethods
    {
        public static List<T> MyClone<T>(this List<T> list)
        {
            var returnList = new List<T>();
            //foreach(var tempVaule in list)
            //{
            //    returnList.Add(tempVaule);
            //}
            returnList.AddRange(list);
            return returnList;
        }

        public static T MyDeepClone<T>(this T source) where T : IFiddlerHttpTamper
        {
            if (!typeof(T).IsSerializable) throw new ArgumentException("Your type must be serializable.", "source");
            T cloneObj = default;
            using (var jsonStream = MyJsonHelper.JsonDataContractJsonSerializer.ObjectToJsonStream(source))
            {
                cloneObj = MyJsonHelper.JsonDataContractJsonSerializer.JsonStreamToObject<T>(jsonStream);
            }

            return cloneObj;
        }

        public static bool MyContains<T>(this List<T> list, T item)
        {
            if (item == null)
            {
                for (var j = 0; j < list.Count; j++)
                    if (list[j] == null)
                        return true;
                return false;
            }

            var c = typeof(T);
            if (c == typeof(MyKeyValuePair<string, string>))
            {
                for (var j = 0; j < list.Count; j++)
                    if (list[j].Equals(item))
                        return true;
                return false;
            }

            return list.Contains(item);
        }
    }
}