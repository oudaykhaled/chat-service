namespace ChatService.Domain.Request
{
    public class BindMessageRequest
    {
        public string MessageID { get; set; }
        public string MemberID { get; set; }
        public string ChannelID { get; set; }
        public string SessionID { get; set; }
        public string Text { get; set; }
    }
}
