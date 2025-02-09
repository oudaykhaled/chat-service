using Microsoft.AspNetCore.Mvc;

namespace ChatService.Domain.Request
{
    public class DeleteMemberRequest
    {
        [FromRoute(Name = "memberId")]
        public string ID { get; set; }
    }
}
