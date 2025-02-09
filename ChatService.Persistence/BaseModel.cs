using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatService.Persistence
{
    [FirestoreData]
    public class BaseModel
    {
        [Key]
        public string ID { get; set; }
        [FirestoreProperty]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [FirestoreProperty]
        public DateTime? ModifiedAt { get; set; }
        [FirestoreProperty]
        public DateTime? DeletedAt { get; set; }
        [FirestoreProperty]
        public string Guid { get; set; }
        [FirestoreProperty]
        public bool IsDeleted { get; set; } = false;
        [FirestoreProperty]
        public bool IsActive { get; set; } = true;
        public int StatusID { get; set; } = (int)Status.Active;
    }
}
