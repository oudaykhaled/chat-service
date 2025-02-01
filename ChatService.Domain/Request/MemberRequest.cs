using Microsoft.AspNetCore.Mvc;

namespace ChatService.Domain.Request
{
    public class MemberRequest
    {
        [FromRoute(Name = "memberId")]
        public string ID { get; set; }
    }
}
