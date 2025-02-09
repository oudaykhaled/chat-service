using Google.Cloud.Firestore;

namespace ChatService.Domain.Request
{
    public class MaskMessageRequest
    {
        public string MessageID { get; set; }
        public string MemberID { get; set; }
        public string ChannelID { get; set; }
        public string SessionID { get; set; }
        public bool Mask { get; set; }
    }
}
