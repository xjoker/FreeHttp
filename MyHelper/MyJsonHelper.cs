using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace FreeHttp.MyHelper
{
    public class MyJsonHelper
    {
        public class JsonDataContractJsonSerializer
        {
            /// <summary>
            ///     使用.net内置方法将对象序列号为str 对象需要使用[System.Runtime.Serialization.DataContract()]标记
            /// </summary>
            /// <param name="obj"></param>
            /// <returns></returns>
            public static string ObjectToJsonStr(object obj)
            {
                var serializer = new DataContractJsonSerializer(obj.GetType());
                using (var stream = new MemoryStream())
                {
                    serializer.WriteObject(stream, obj);
                    using (var sr = new StreamReader(stream))
                    {
                        stream.Position = 0;
                        return sr.ReadToEnd();
                    }
                }
            }

            public static Stream ObjectToJsonStream(object obj)
            {
                var serializer = new DataContractJsonSerializer(obj.GetType());
                var stream = new MemoryStream();
                serializer.WriteObject(stream, obj);
                return stream;
            }

            public static T JsonStringToObject<T>(string str)
            {
                var serializeClass = default(T);
                var ser = new DataContractJsonSerializer(typeof(T));
                try
                {
                    using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(str)))
                    {
                        serializeClass = (T)ser.ReadObject(ms);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    serializeClass = default;
                }

                return serializeClass;
            }

            public static T JsonStreamToObject<T>(Stream jsonStream)
            {
                var serializeClass = default(T);
                var ser = new DataContractJsonSerializer(typeof(T));
                try
                {
                    jsonStream.Position = 0;
                    serializeClass = (T)ser.ReadObject(jsonStream);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    serializeClass = default;
                }

                return serializeClass;
            }
        }
    }
}