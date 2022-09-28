using System.Runtime.Serialization;

namespace FreeHttp.WebService.DataModel
{
    [DataContract]
    public enum ReturnStatus
    {
        Success = 0,
        Fail = 1,
        Error = 2
    }

    [DataContract]
    public class BaseResultModel<T>
    {
        public BaseResultModel(int? code = 0, string message = null, T result = default,
            ReturnStatus returnStatus = ReturnStatus.Success)
        {
            Code = code ?? 0;
            Message = message;
            Status = returnStatus;
            Result = result;
        }

        [DataMember] public int Code { get; set; }

        [DataMember] public string Message { get; set; }

        [DataMember] public T Result { get; set; }

        [DataMember] public ReturnStatus Status { get; set; }
    }
}