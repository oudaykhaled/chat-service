namespace ChatService.Domain.Request
{
    public class CreateMemberRequest
    {
        public string Name { get; set; }
        public string Nickname { get; set; }
        public string Type { get; set; }
        public string Tag { get; set; }
    }
}
