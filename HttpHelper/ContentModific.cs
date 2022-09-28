using System;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using FreeHttp.AutoTest;

namespace FreeHttp.HttpHelper
{
    public enum ContentModifiedMode
    {
        NoChange,
        KeyValueReplace,
        EntireReplace,
        RegexReplace,
        HexReplace,
        ReCode
    }

    [Serializable]
    [DataContract]
    public class ContentModific
    {
        public ContentModific()
        {
            ModifiedMode = ContentModifiedMode.NoChange;
            TargetKey = null;
            ReplaceContent = null;
        }

        public ContentModific(string targetKey, string replaceContent)
        {
            if (string.IsNullOrEmpty(targetKey))
            {
                ModifiedMode = ContentModifiedMode.EntireReplace;
                TargetKey = null;
            }
            else
            {
                if (targetKey.StartsWith("<regex>"))
                {
                    ModifiedMode = ContentModifiedMode.RegexReplace;
                    TargetKey = targetKey;
                }
                else if (targetKey.StartsWith("<hex>"))
                {
                    //check data
                    try
                    {
                        replaceContent = replaceContent.TrimEnd(' ');
                        targetKey = targetKey.TrimEnd(' ');
                        replaceContent =
                            BitConverter.ToString(MyBytes.HexStringToByte(replaceContent, HexDecimal.hex16));
                        TargetKey = string.Format("<hex>{0}",
                            BitConverter.ToString(MyBytes.HexStringToByte(targetKey.Remove(0, "<hex>".Length),
                                HexDecimal.hex16)));
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(
                            string.Format(
                                "your input is illegal that your should use prescribed hex16 format like 0x00 0x01 0xff and the space or - will be ok for byte spit. \r\ninner Exception is [{0}]",
                                ex.Message), ex);
                    }

                    ModifiedMode = ContentModifiedMode.HexReplace;
                }
                else if (targetKey.StartsWith("<recode>"))
                {
                    try
                    {
                        targetKey = targetKey.TrimEnd(' ');
                        Encoding.GetEncoding(targetKey.Remove(0, 8)
                            .Trim(' ')); //https://docs.microsoft.com/zh-cn/dotnet/api/system.text.encoding?view=netcore-2.2
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(
                            string.Format(
                                "your input is illegal that your should use legal EncodingInfo.Name like utf-8;hz-gb-2312 ......\r\ninner Exception is [{0}]",
                                ex.Message), ex);
                    }

                    ModifiedMode = ContentModifiedMode.ReCode;
                    TargetKey = targetKey;
                }
                else
                {
                    ModifiedMode = ContentModifiedMode.KeyValueReplace;
                    TargetKey = targetKey;
                }
            }

            //set the ReplaceContent
            if (ModifiedMode == ContentModifiedMode.EntireReplace && string.IsNullOrEmpty(replaceContent))
            {
                ModifiedMode = ContentModifiedMode.NoChange;
                ReplaceContent = null;
            }
            else if (ModifiedMode == ContentModifiedMode.ReCode)
            {
                ReplaceContent = null;
            }
            else
            {
                ReplaceContent = replaceContent == null ? "" : replaceContent;
            }
        }

        [DataMember] public ContentModifiedMode ModifiedMode { get; set; }

        [DataMember] public string TargetKey { get; set; }

        [DataMember] public string ReplaceContent { get; set; }

        public string GetFinalContent(string sourceContent)
        {
            string finalContent;
            switch (ModifiedMode)
            {
                case ContentModifiedMode.NoChange:
                    finalContent = sourceContent;
                    break;
                case ContentModifiedMode.EntireReplace:
                    finalContent = ReplaceContent;
                    break;
                case ContentModifiedMode.KeyValueReplace:
                    finalContent = sourceContent.Replace(TargetKey, ReplaceContent);
                    break;
                case ContentModifiedMode.RegexReplace:
                    try
                    {
                        finalContent = Regex.Replace(sourceContent, TargetKey.Remove(0, 8), ReplaceContent);
                    }
                    catch (Exception ex)
                    {
                        finalContent = $"RegexReplace [{TargetKey.Remove(0, 7)}] GetFinalContent fail :{ex.Message}";
                    }

                    break;
                case ContentModifiedMode.HexReplace:
                    throw new Exception("your should implement HexReplace with anther GetFinalContent overload");
                case ContentModifiedMode.ReCode:
                    throw new Exception("your should implement Recode with GetRecodeContent");
                default:
                    throw new Exception("not support ContentModifiedMode");
            }

            return finalContent;
        }

        public byte[] GetFinalContent(byte[] sourceContent)
        {
            switch (ModifiedMode)
            {
                case ContentModifiedMode.NoChange:
                case ContentModifiedMode.EntireReplace:
                case ContentModifiedMode.KeyValueReplace:
                case ContentModifiedMode.RegexReplace:
                case ContentModifiedMode.ReCode:
                    throw new Exception("this implement of GetFinalContent is only for HexReplace");
                case ContentModifiedMode.HexReplace:
                    var replaceContentBytes = MyBytes.HexStringToByte(ReplaceContent, HexDecimal.hex16);
                    var searchKey = TargetKey.Remove(0, 5); //<hex>
                    if (string.IsNullOrEmpty(searchKey)) return replaceContentBytes;
                    var searchKeyBytes = MyBytes.HexStringToByte(searchKey, HexDecimal.hex16);
                    return MyBytes.ReplaceBytes(sourceContent, searchKeyBytes, replaceContentBytes);
                default:
                    throw new Exception("not support ContentModifiedMode");
            }
        }

        public byte[] GetRecodeContent(string sourceContent)
        {
            switch (ModifiedMode)
            {
                case ContentModifiedMode.NoChange:
                case ContentModifiedMode.EntireReplace:
                case ContentModifiedMode.KeyValueReplace:
                case ContentModifiedMode.RegexReplace:
                case ContentModifiedMode.HexReplace:
                    throw new Exception("this implement of GetRecodeContent is only for ReCode ");
                case ContentModifiedMode.ReCode:
                    var searchKey = TargetKey.Remove(0, 8).Trim(' ');
                    var nowEncoding =
                        Encoding.GetEncoding(searchKey); //shoud check the searchKey when we creat ContentModific
                    return nowEncoding.GetBytes(sourceContent);
                default:
                    throw new Exception("not support ContentModifiedMode");
            }
        }

        public override string ToString()
        {
            var resultStringBuilder = new StringBuilder();
            switch (ModifiedMode)
            {
                case ContentModifiedMode.NoChange:
                    break;
                case ContentModifiedMode.EntireReplace:
                    resultStringBuilder.Append("[EntireReplace] ");
                    resultStringBuilder.Append(ReplaceContent);
                    break;
                case ContentModifiedMode.KeyValueReplace:
                    resultStringBuilder.Append("[Replace] ");
                    resultStringBuilder.Append(TargetKey);
                    resultStringBuilder.Append(" [To] ");
                    resultStringBuilder.Append(ReplaceContent);
                    break;
                case ContentModifiedMode.RegexReplace:
                    resultStringBuilder.Append("[RegexReplace] ");
                    resultStringBuilder.Append(TargetKey);
                    resultStringBuilder.Append(" [To] ");
                    resultStringBuilder.Append(ReplaceContent);
                    break;
                case ContentModifiedMode.HexReplace:
                    resultStringBuilder.Append("[HexReplace] ");
                    resultStringBuilder.Append(TargetKey);
                    resultStringBuilder.Append(" [To] ");
                    resultStringBuilder.Append(ReplaceContent);
                    break;
                case ContentModifiedMode.ReCode:
                    resultStringBuilder.Append("[ReCode] ");
                    resultStringBuilder.Append(TargetKey);
                    break;
                default:
                    resultStringBuilder.Append("not support ContentModifiedMode");
                    break;
            }

            return resultStringBuilder.ToString();
        }
    }
}