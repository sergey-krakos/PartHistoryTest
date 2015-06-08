using System;
using System.ServiceModel;

namespace WebServices
{
    [ServiceContract]
    public interface IPartHistoryNew
    {
        [OperationContract]
        Guid StartPartHistory(int seconds, string callback);

        [OperationContract]
        GetPartHistoryOutput GetPartHistoryStatus(Guid jobId);

        [OperationContract]
        AbortPartHistoryOutput AbortPartHistory(Guid jobId);
    }
}