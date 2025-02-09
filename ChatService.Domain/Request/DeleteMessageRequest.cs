namespace ChatService.Domain.Request
{
    public class DeleteMessageRequest
    {
        public string ID { get; set; }
        public string ChannelID { get; set; }
        public string MemberID { get; set; }
        public string SessionID { get; set; }
    }
}
