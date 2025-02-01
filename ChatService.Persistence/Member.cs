using Google.Cloud.Firestore;

namespace ChatService.Persistence
{
    [FirestoreData]
    public class Member : BaseModel
    {
        [FirestoreProperty]
        public string Name { get; set; }
        [FirestoreProperty]
        public string Nickname { get; set; }
        [FirestoreProperty]
        public string Type { get; set; }
        [FirestoreProperty]
        public string Tag { get; set; }
    }
}
