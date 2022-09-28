using System;

namespace FreeHttp.AutoTest.ParameterizationPick
{
    [Serializable]
    public enum ParameterPickType
    {
        Str,
        Xml,
        Regex
    }


    [Serializable]
    public enum ParameterPickRange
    {
        Line,
        Heads,
        Entity
    }
}