namespace WebServices
{
    public class GetPartHistoryOutput
    {
        public bool IsReady { get; set; }

        public ErrorCode Status { get; set; }

        public ErrorCode ErrorCode { get; set; }

        public string ReturnedDocument { get; set; }
    }
}
