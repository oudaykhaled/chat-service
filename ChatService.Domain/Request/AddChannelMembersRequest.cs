namespace ChatService.Domain.Request
{
    public class AddChannelMembersRequest
    {
        public string ChannelID { get; set; }
        public string MemberID { get; set; }
    }
}
