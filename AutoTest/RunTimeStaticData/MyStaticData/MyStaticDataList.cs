﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace FreeHttp.AutoTest.RunTimeStaticData.MyStaticData
{
    /// <summary>
    ///     为StaticData提供当基于List的列表数据支持据【IRunTimeStaticData】
    /// </summary>
    [DataContract]
    public class MyStaticDataList : IRunTimeStaticData
    {
        [DataMember] private bool isNew;

        [DataMember] private bool isRandom;

        [DataMember] private int nowIndex;

        private readonly Random ran;

        [DataMember] private string souseData;

        [DataMember] private List<string> souseListData;

        public MyStaticDataList(string yourSourceData, bool isRandomNext)
        {
            isNew = true;
            souseData = yourSourceData;
            souseListData = yourSourceData.Split(',').ToList();
            nowIndex = 0;
            isRandom = isRandomNext;
            if (isRandom)
                ran = new Random();
            else
                ran = null;
        }

        public MyStaticDataList(string yourSourceData, bool isRandomNext, string originalConnectString)
            : this(yourSourceData, isRandomNext)
        {
            OriginalConnectString = originalConnectString;
        }

        [DataMember] public string OriginalConnectString { get; private set; }

        public string RunTimeStaticDataTypeAlias => "staticData_list";

        public CaseStaticDataType RunTimeStaticDataType => CaseStaticDataType.caseStaticData_list;

        public object Clone()
        {
            return new MyStaticDataList(souseData, isRandom);
        }

        public string DataCurrent()
        {
            return souseListData[nowIndex];
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public string DataMoveNext()
        {
            if (isRandom)
            {
                nowIndex = ran.Next(0, souseListData.Count - 1);
                return souseListData[nowIndex];
            }

            if (isNew)
            {
                isNew = false;
            }
            else
            {
                nowIndex++;
                if (nowIndex > souseListData.Count - 1) nowIndex = 0;
            }

            return souseListData[nowIndex];
        }

        public void DataReset()
        {
            isNew = true;
            nowIndex = 0;
        }

        public bool DataSet(string expectData)
        {
            if (souseListData.Contains(expectData)) nowIndex = souseListData.IndexOf(expectData);
            return false;
        }
    }
}