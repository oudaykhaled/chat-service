namespace ChatService.Domain.Request
{
    public class HideMessageRequest
    {
        public string MessageID { get; set; }
        public string MemberID { get; set; }
        public string ChannelID { get; set; }
        public string SessionID { get; set; }
        public bool Hide {  get; set; }
    }
}
