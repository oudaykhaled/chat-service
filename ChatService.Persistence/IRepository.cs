using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatService.Persistence
{
    public interface IRepository<T> where T : BaseModel
    {
        Task<List<T>> GetAllAsync();
        Task<PagedList<T>> GetPagedAsync(int pageSize, string? startAfterDocumentId);
        Task<T> GetByIdAsync(string id);
        Task<string> AddAsync(T entity);
        Task<string> UpdateAsync(T entity);
        Task DeleteAsync(T entity);
        CollectionReference Collection();
        Task<List<T>> QueryAsync(Query query);
        Task<bool> QueryExistsAsync(Query query);
    }
}
