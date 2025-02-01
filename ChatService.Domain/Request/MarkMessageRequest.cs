namespace ChatService.Domain.Request
{
    public class MarkMessageRequest
    {
        public string MessageID { get; set; }
        public string MemberID { get; set; }
        public string ChannelID { get; set; }
        public string SessionID { get; set; }
        public MessageStatus Status { get; set; }
    }
}
