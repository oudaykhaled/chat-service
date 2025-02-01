using Google.Cloud.Firestore;
using System.ComponentModel.DataAnnotations;

namespace ChatService.Persistence
{
    [FirestoreData]
    public class Channel : BaseModel
    {
        [FirestoreProperty]
        public string Name { get; set; }
        [DataType(DataType.MultilineText)]
        [FirestoreProperty]
        public string Description { get; set; }
        [FirestoreProperty]
        public Visibility Visibility { get; set; }
        [FirestoreProperty]
        public string CreatedBy { get; set; }
        [FirestoreProperty]
        public string Type { get; set; }
        [FirestoreProperty]
        public string Tag { get; set; }
    }
}
