using Google.Cloud.Firestore;
using Microsoft.Extensions.Options;

namespace ChatService.Persistence
{
    public class ChatDbContext
    {
        private readonly FirestoreDb _fireStoreDb;
        private readonly FirestoreOptions _firestoreOptions;

        public ChatDbContext(IOptions<FirestoreOptions> options)
        {
            _firestoreOptions = options.Value;

            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", _firestoreOptions.KeyFilePath);

            _fireStoreDb = FirestoreDb.Create(_firestoreOptions.ProjectId);
        }

        public CollectionReference GetCollection(string collectionName)
        {
            return _fireStoreDb.Collection(collectionName);
        }
    }
}
