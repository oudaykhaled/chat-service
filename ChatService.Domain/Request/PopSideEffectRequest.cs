
namespace ChatService.Domain.Request
{
    public class PopSideEffectRequest
    {
        public string ChannelID { get; set; }
        public ModerationType ModerationType { get; set; }
    }
}
