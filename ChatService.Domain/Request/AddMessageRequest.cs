using Google.Cloud.Firestore;

namespace ChatService.Domain.Request
{
    public class AddMessageRequest
    {
        public string MemberID { get; set; }
        public string ChannelID { get; set; }
        public string SessionID { get; set; }
        public string Text { get; set; }
        public string Payload { get; set; }
        public string ParentID { get; set; }
    }
}
