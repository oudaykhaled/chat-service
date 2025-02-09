using Google.Cloud.Firestore;

namespace ChatService.Persistence
{
    [FirestoreData]
    public class SessionMember : BaseModel
    {
        [FirestoreProperty]
        public string MemberID { get; set; }
        [FirestoreProperty]
        public string ChannelID { get; set; }
        [FirestoreProperty]
        public string? Type { get; set; }
        [FirestoreProperty]
        public string? Tag { get; set; }
        [FirestoreProperty]
        public bool IsAdmin { get; set; }
        [FirestoreProperty]
        public bool IsModerator { get; set; }
        [FirestoreProperty]
        public string? AccessKey { get; set; }
        [FirestoreProperty]
        public string? Role { get; set; }
        [FirestoreProperty]
        public int Ordinal { get; set; }
        [FirestoreProperty]
        public byte[] Position { get; set; } = Array.Empty<byte>();
    }
}
