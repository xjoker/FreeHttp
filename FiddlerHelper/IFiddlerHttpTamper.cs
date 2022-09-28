﻿using System;
using System.Collections.Generic;
using FreeHttp.AutoTest.ParameterizationPick;

namespace FreeHttp.FiddlerHelper
{
    public interface IFiddlerHttpTamper : ICloneable
    {
        string RuleUid { get; set; }
        bool IsEnable { get; set; }
        bool IsHasParameter { get; set; }
        TamperProtocalType TamperProtocol { get; set; }
        FiddlerHttpFilter HttpFilter { get; set; }

        List<ParameterPick> ParameterPickList { get; set; }
        FiddlerActuatorStaticDataCollectionController ActuatorStaticDataController { get; set; }
        bool IsRawReplace { get; }
    }
}