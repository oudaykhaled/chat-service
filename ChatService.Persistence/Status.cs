using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatService.Persistence
{
    public enum Status : int
    {
        Active,         
        Inactive,       
        Pending,                
        Closed
    }
}
