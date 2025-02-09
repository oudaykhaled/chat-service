using Google.Cloud.Firestore;

namespace ChatService.Persistence
{
    [FirestoreData]
    public class Message : BaseModel
    {
        [FirestoreProperty]
        public string Text { get; set; }
        [FirestoreProperty]
        public string Payload { get; set; }
        [FirestoreProperty]
        public string ParentID { get; set; }
        [FirestoreProperty]
        public bool IsHidden { get; set; }
        [FirestoreProperty]
        public bool IsEdited { get; set; }
        [FirestoreProperty]
        public string MessageLogs { get; set; }
        [FirestoreProperty]
        public bool IsMaskedText { get; set; }
        [FirestoreProperty]
        public List<string> DeliveredTo { get; set; }
        [FirestoreProperty]
        public List<string> SeenBy { get; set; }
        [FirestoreProperty]
        public List<string> Context { get; set; }
        [FirestoreProperty]
        public string MemberID { get; set; }
        [FirestoreProperty]
        public string ChannelID { get; set; }
        [FirestoreProperty]
        public string SessionID { get; set; }
    }
}
