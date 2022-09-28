using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using FreeHttp.FiddlerHelper;
using FreeHttp.WebService;

namespace FreeHttp.MyHelper
{
    internal class SerializableHelper
    {
        public static void SerializeRuleList(ListView requestRuleListView, ListView reponseRuleListView)
        {
            var rulePath = "FreeHttp\\RuleData.xml";
            if (requestRuleListView != null && reponseRuleListView != null)
            {
                //dynamic
                var requestList = new List<FiddlerRequestChange>();
                var responseList = new List<FiddlerResponseChange>();
                foreach (ListViewItem tempItem in requestRuleListView.Items)
                    requestList.Add((FiddlerRequestChange)tempItem.Tag);
                foreach (ListViewItem tempItem in reponseRuleListView.Items)
                    responseList.Add((FiddlerResponseChange)tempItem.Tag);
                //Stream stream = File.Open("data.xml", FileMode.Create);
                TextWriter writer = new StreamWriter(rulePath, false);
                var serializer = new XmlSerializer(typeof(FiddlerModificHttpRuleCollection));
                //serializer = new XmlSerializer(typeof(List<IFiddlerHttpTamper>));
                serializer.Serialize(writer, new FiddlerModificHttpRuleCollection(requestList, responseList));
                writer.Close();

                //写入版本
                var xmlDocument = new XmlDocument();
                xmlDocument.Load(rulePath);
                var dbAtt = xmlDocument.CreateAttribute("ruleVersion");
                dbAtt.Value = UserComputerInfo.GetRuleVersion();
                xmlDocument.SelectSingleNode("/FiddlerModificHttpRuleCollection")?.Attributes.Append(dbAtt);
                xmlDocument.Save(rulePath);
            }
        }

        public static FiddlerModificHttpRuleCollection DeserializeRuleList()
        {
            var rulePath = "FreeHttp\\RuleData.xml";
            FiddlerModificHttpRuleCollection fiddlerModificHttpRuleCollection = null;
            if (File.Exists(rulePath))
            {
                var myFileStream = new FileStream(rulePath, FileMode.Open);
                try
                {
                    using (var reader = new XmlTextReader(myFileStream))
                    {
                        reader.Normalization = false;
                        //版本控制
                        var ruleVersion = string.Empty;
                        //System.Version version = new Version("2.0.0");
                        while (reader.Read())
                            if (reader.NodeType == XmlNodeType.Element)
                                if (reader.Name == "FiddlerModificHttpRuleCollection")
                                {
                                    ruleVersion = reader.GetAttribute("ruleVersion");
                                    break;
                                }

                        if (string.IsNullOrEmpty(ruleVersion) || ruleVersion[0] == '1')
                        {
                            File.Copy(rulePath, rulePath + ".oldVersion", true);
                            var mySerializer =
                                new XmlSerializer(
                                    typeof(FiddlerHelper.VersionControlV1.FiddlerModificHttpRuleCollection));
                            fiddlerModificHttpRuleCollection =
                                (FiddlerModificHttpRuleCollection)
                                (FiddlerHelper.VersionControlV1.FiddlerModificHttpRuleCollection)mySerializer
                                    .Deserialize(reader);
                        }
                        else if (ruleVersion[0] == '2')
                        {
                            var mySerializer = new XmlSerializer(typeof(FiddlerModificHttpRuleCollection));
                            fiddlerModificHttpRuleCollection =
                                (FiddlerModificHttpRuleCollection)mySerializer.Deserialize(reader);
                        }
                        else
                        {
                            throw new Exception("unkonw ruleVersion",
                                new Exception(
                                    "this freehttp can not analysis the rule file , you should updata your freehttp"));
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        string.Format("{0}\r\n{1}\r\nyour error rule file will back up in {2}", ex.Message,
                            ex.InnerException == null ? "" : ex.InnerException.Message,
                            Directory.GetCurrentDirectory() + rulePath + ".lastErrorFile"), "load user rule fail");
                    _ = RemoteLogService.ReportLogAsync($"load user rule fail [{ex}]",
                        RemoteLogService.RemoteLogOperation.WindowLoad, RemoteLogService.RemoteLogType.Error);
                    File.Copy(rulePath, rulePath + ".lastErrorFile", true);
                }
                finally
                {
                    myFileStream.Close();
                }
            }

            return fiddlerModificHttpRuleCollection;
        }


        /// <summary>
        ///     [Serializable] 标记类 （公共成员默认被序列化）
        ///     [NonSerialized] 标记不需要序列化的成员 (只对终端field有效 ， 属性可以使用[System.Xml.Serialization.XmlIgnore])
        ///     Serializable 需要空参数的构造函数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="modificSettingInfo"></param>
        /// <param name="filePath"></param>
        public static void SerializeData<T>(T modificSettingInfo, string filePath)
        {
            if (modificSettingInfo != null)
            {
                TextWriter writer = new StreamWriter(filePath, false);
                var serializer = new XmlSerializer(typeof(T));
                //serializer = new XmlSerializer(typeof(List<IFiddlerHttpTamper>));
                serializer.Serialize(writer, modificSettingInfo);
                writer.Close();
            }
        }

        public static T DeserializeData<T>(string filePath)
        {
            var modificSettingInfo =
                default(T); //对于数值类型会返回零。 对于结构，此关键字将返回初始化为零或 null 的每个结构成员，具体取决于这些结构是值类型还是引用类型,对于数值类型会返回零。 对于结构，此关键字将返回初始化为零或 null 的每个结构成员，具体取决于这些结构是值类型还是引用类型
            if (File.Exists(filePath))
            {
                var mySerializer = new XmlSerializer(typeof(T));
                var myFileStream = new FileStream(filePath, FileMode.Open);
                try
                {
                    //modificSettingInfo = (T)mySerializer.Deserialize(myFileStream);    //默认使用XmlReader （ It doesn't have a property for controlling normalization, as the XmlTextReader does.） 导致\r\n被反序列化为\n
                    using (var reader = new XmlTextReader(myFileStream))
                    {
                        reader.Normalization = false;
                        modificSettingInfo = (T)mySerializer.Deserialize(reader);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        string.Format("{0}\r\n{1}", ex.Message,
                            ex.InnerException == null ? "" : ex.InnerException.Message), "DeserializeData fail");
                    File.Copy(filePath, string.Format("{0}.lastErrorFile", filePath), true);
                    modificSettingInfo = default;
                }
                finally
                {
                    myFileStream.Close();
                }
            }

            return modificSettingInfo;
        }


        /// <summary>
        ///     『DataMemberAttribute Class』
        ///     使用 [DataContract()] 标记class
        ///     【如果要使用[Serializable] 默认序列化公开字段及属性，且要求其有公开的Set,用[DataContract]指没有这个限制，使用 [DataMember(Name = "ID")] / [DataMember]
        ///     标记成员】
        ///     使用 [DataMember(Name = "ID")] / [DataMember]  标记成员
        ///     并且不要求成员访问修饰符为public
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serializeClass"></param>
        /// <param name="filePath"></param>
        public static void SerializeContractData<T>(T serializeClass, string filePath)
        {
            if (serializeClass != null)
            {
                var fs = new FileStream(filePath, FileMode.Create);
                var writer = XmlDictionaryWriter.CreateTextWriter(fs);
                var ser = new DataContractSerializer(typeof(T));
                ser.WriteObject(writer, serializeClass);
                writer.Close();
                fs.Close();
            }
        }

        public static T DeserializeContractData<T>(string filePath)
        {
            var serializeClass =
                default(T); //对于数值类型会返回零。 对于结构，此关键字将返回初始化为零或 null 的每个结构成员，具体取决于这些结构是值类型还是引用类型,对于数值类型会返回零。 对于结构，此关键字将返回初始化为零或 null 的每个结构成员，具体取决于这些结构是值类型还是引用类型
            if (File.Exists(filePath))
            {
                var fs = new FileStream(filePath, FileMode.OpenOrCreate);
                var ser = new DataContractSerializer(typeof(T));
                try
                {
                    serializeClass = (T)ser.ReadObject(fs);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        string.Format("{0}\r\n{1}", ex.Message, ex.Message,
                            ex.InnerException == null ? "" : ex.InnerException.Message),
                        "DeserializeContractData Fail");
                    File.Copy(filePath, string.Format("{0}.lastErrorFile", filePath), true);
                    serializeClass = default;
                }
                finally
                {
                    fs.Close();
                }
            }

            return serializeClass;
        }
    }
}