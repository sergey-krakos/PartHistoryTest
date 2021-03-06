﻿using Common;

namespace WebServices
{
    public class GetPartHistoryOutput
    {
        public bool IsReady { get; set; }

        public JobStatus Status { get; set; }

        public ErrorCode ErrorCode { get; set; }

        public string ReturnedDocument { get; set; }
    }
}
