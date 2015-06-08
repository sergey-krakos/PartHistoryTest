using Common;

namespace WebServices
{
    public class AbortPartHistoryOutput
    {
        public JobStatus Status { get; set; }

        public ErrorCode ErrorCode { get; set; }
    }
}