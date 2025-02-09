using Microsoft.AspNetCore.Mvc;

namespace ChatService.Domain.Request
{
    public class DeactivateChannelRequest
    {
        [FromRoute(Name = "channelId")]
        public string ID { get; set; }
    }
}
