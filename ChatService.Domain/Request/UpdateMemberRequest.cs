namespace ChatService.Domain.Request
{
    public class UpdateMemberRequest : CreateMemberRequest
    {
        public string ID { get; set; }
    }
}
