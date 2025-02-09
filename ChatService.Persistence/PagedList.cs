using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatService.Persistence
{
    public class PagedList<T>
    {
        public string? NextPageToken {  get; set; }
        public int PageCount { get; set; }
        public int TotalCount { get; set; }
        public IList<T> Items { get; set; }
        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= Items.Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
                }
                return Items[index];
            }
        }
    }
}
