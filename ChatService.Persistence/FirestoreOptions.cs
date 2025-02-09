namespace ChatService.Persistence
{
    public class FirestoreOptions
    {
        public const string Firestore = "Firestore";
        public string ProjectId { get; set; }
        public string KeyFilePath { get; set; }
    }
}
