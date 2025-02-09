using ChatService.Persistence.Exception;
using Google.Cloud.Firestore;

namespace ChatService.Persistence
{
    public class Repository<T> : IRepository<T> where T : BaseModel
    {
        private readonly ChatDbContext _context;
        private readonly string _collectionName;

        public Repository(ChatDbContext context)
        {
            _context = context;
            _collectionName = typeof(T).Name.ToLower();
        }

        public async Task<string> AddAsync(T entity)
        {
            CollectionReference collection = _context.GetCollection(_collectionName);
            var documentReference = await collection.AddAsync(entity);
            return documentReference.Id;
        }

        public async Task DeleteAsync(T entity)
        {
            DocumentReference docRef = _context.GetCollection(_collectionName).Document(entity.ID);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();
            if (snapshot.Exists)
            {
                await docRef.DeleteAsync();
            }
            else
            {
                throw new NoDataFoundException(entity.ID);
            }
        }

        public async Task<List<T>> GetAllAsync()
        {
            CollectionReference collection = _context.GetCollection(_collectionName);
            QuerySnapshot snapshot = await collection.GetSnapshotAsync();
            return snapshot.Documents.Select(doc =>
            {
                T record = doc.ConvertTo<T>();
                record.ID = doc.Id;
                return record;
            }).ToList();
        }

        public async Task<PagedList<T>> GetPagedAsync(int pageSize, string? startAfterDocumentId)
        {
            PagedList<T> result = new PagedList<T>();
            CollectionReference collection = _context.GetCollection(_collectionName);
            Query query = collection.Limit(pageSize).OrderBy(nameof(Member.CreatedAt));

            QuerySnapshot snapshot = await collection.GetSnapshotAsync();
            result.TotalCount = snapshot.Count;

            if (result.TotalCount > 0)
            {
                result.PageCount = (int)Math.Ceiling((double)result.TotalCount / pageSize);
            }

            if (!string.IsNullOrEmpty(startAfterDocumentId))
            {
                DocumentSnapshot startAfterSnapshot = await collection.Document(startAfterDocumentId).GetSnapshotAsync();
                if (startAfterSnapshot.Exists)
                {
                    query = query.StartAfter(startAfterSnapshot);
                }
            }

            snapshot = await query.GetSnapshotAsync();

            result.NextPageToken = snapshot.Documents.Count < pageSize ? null : snapshot.Documents[^1].Id;
            result.Items = snapshot.Documents.Select(doc =>
            {
                T record = doc.ConvertTo<T>();
                record.ID = doc.Id;
                return record;
            }).ToList();

            return result;
        }

        public async Task<T> GetByIdAsync(string id)
        {
            T? result = default;
            DocumentReference docRef = _context.GetCollection(_collectionName).Document(id);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

            if (snapshot.Exists)
            {
                result = snapshot.ConvertTo<T>();
                result.ID = snapshot.Id;
                return result;
            }

            throw new NoDataFoundException(id);
        }

        public async Task<string> UpdateAsync(T entity)
        {
            string result = null;

            DocumentReference docRef = _context.GetCollection(_collectionName).Document(entity.ID);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();
            if (snapshot.Exists)
            {
                await docRef.SetAsync(entity, SetOptions.Overwrite);
                result = docRef.Id;
            }
            else
            {
                throw new NoDataFoundException(entity.ID);
            }
            return result;
        }

        public CollectionReference Collection()
        {
            return _context.GetCollection(_collectionName);
        }

        public async Task<List<T>> QueryAsync(Query query)
        {
            QuerySnapshot snapshot = await query.GetSnapshotAsync();
            return snapshot.Documents.Select(doc => doc.ConvertTo<T>()).ToList();
        }

        public async Task<bool> QueryExistsAsync(Query query)
        {
            QuerySnapshot snapshot = await query.GetSnapshotAsync();
            return snapshot.Count > 0;
        }
    }
}
