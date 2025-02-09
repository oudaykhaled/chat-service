using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatService.Domain.Response
{
    public class MemberResponse : IdentityResponse
    {
        public string Name { get; set; }
        public string Nickname { get; set; }
        public string Type { get; set; }
        public string Tag { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
