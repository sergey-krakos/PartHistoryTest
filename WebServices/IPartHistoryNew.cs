using System;
using System.ServiceModel;

namespace WebServices
{
    [ServiceContract]
    public interface IPartHistoryNew
    {
        [OperationContract]
        Guid StartPartHistory(int seconds);

        [OperationContract]
        GetPartHistoryOutput GetPartHistoryStatus(Guid jobId);
    }
}
