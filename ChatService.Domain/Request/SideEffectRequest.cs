
namespace ChatService.Domain.Request
{
    public class SideEffectRequest : AddMessageRequest
    {
        public string Guid { get; set; }
        public DateTime CreatedAt { get; set; }
        public ModerationType ModerationType { get; set; }
        public string MessageID { get; set; }
    }
}
