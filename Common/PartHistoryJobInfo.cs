using System;

namespace Common
{
    [Serializable]
    public class PartHistoryJobInfo
    {
        public PartHistoryJobInfo()
        {
        }

        public PartHistoryJobInfo(int seconds)
        {
            Seconds = seconds;
            JobId = Guid.NewGuid();
            Status = JobStatus.InProgress;
        }

        public Guid JobId { get; set; }

        public int Seconds { get; set; }

        public JobStatus Status { get; set; }
    }
}
