using Google.Cloud.Firestore;

namespace ChatService.Domain.Request
{
    public class ReplyToMessageRequest : AddMessageRequest
    {
        public string MessageID { get; set; }
    }
}
