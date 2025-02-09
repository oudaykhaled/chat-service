namespace ChatService.Domain.Response
{
    public class MessageResponse
    {
        public string Guid { get; set; }
        public DateTime CreatedAt { get; set; }
        public string MemberID { get; set; }
        public string ChannelID { get; set; }
        public string SessionID { get; set; }
        public string Text { get; set; }
        public string Payload { get; set; }
        public string? ParentID { get; set; }
        public string? MessageID { get; set; }
    }
}
