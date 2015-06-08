using System;
using System.Threading;
using System.Xml.Serialization;

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

        public Guid DocumentId { get; set; }

        public string CallBackUrl { get; set; }

        public int Seconds { get; set; }

        public JobStatus Status { get; set; }

        [XmlIgnore]
        public CancellationTokenSource TokenSource { get; set; }
    }
}