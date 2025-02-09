using ChatService.Application.Service;
using ChatService.Domain.Request;
using ChatService.Infrastructure.Broker;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR.Protocol;

namespace ChatService.API.Controllers
{
    [Route("api/channels")]
    [ApiController]
    public class ChannelController : BaseController
    {
        private readonly IChannelService _channelService;
        private readonly IMessageService _messageService;

        public ChannelController(IChannelService channelService, 
            IMessageService messageService)
        {
            _channelService = channelService;
            _messageService = messageService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateChannelRequest request)
        {
            return Ok(await _channelService.CreateAsync(request));
        }

        [HttpPost("{channelId}/members")]
        public async Task<IActionResult> AddMembers([FromRoute] string channelId, [FromBody] AddChannelMembersRequest request)
        {
            request.ChannelID = channelId;
            return Ok(await _channelService.AddMembersAsync(request));
        }

        [HttpPatch("{channelId}/deactivate")]
        public async Task<IActionResult> Deactivate(DeactivateChannelRequest request)
        {
            return Ok(await _channelService.DeactivateAsync(request));
        }

        [HttpPost("{channelId}/messages")]
        public async Task<IActionResult> AddMessage([FromRoute] string channelId, [FromBody] AddMessageRequest request)
        {
            request.ChannelID = channelId;
            return Ok(await _messageService.AddMessageAsync(request));
        }

        [HttpPut("{channelId}/messages")]
        public async Task<IActionResult> EditMessage([FromRoute] string channelId, [FromBody] EditMessageRequest request)
        {
            request.ChannelID = channelId;
            return Ok(await _messageService.EditMessageAsync(request));
        }

        [HttpPost("{channelId}/pop")]
        public async Task<IActionResult> PopMessageAsync([FromRoute] string channelId, [FromBody] PopMessageRequest request)
        {
            request.ChannelID = channelId;
            return Ok(await _messageService.PopMessageAsync(request));
        }

        [HttpPost("{channelId}/messages/{messageId}/moderate")]
        public async Task<IActionResult> ModerateMessage([FromRoute] string channelId, [FromRoute] string messageId, [FromBody] ModerateMessageRequest request)
        {
            request.ChannelID = channelId;
            request.Guid = messageId;
            return Ok(await _messageService.ModerateMessageAsync(request));
        }

        [HttpPost("{channelId}/messages/{messageId}/reply")]
        public async Task<IActionResult> ReplyToMessage([FromRoute] string channelId, [FromRoute] string messageId, [FromBody] ReplyToMessageRequest request)
        {
            request.ChannelID = channelId;
            request.MessageID = messageId;
            return Ok(await _messageService.ReplyToMessageAsync(request));
        }

        [HttpPost("{channelId}/messages/{messageId}/bind")]
        public async Task<IActionResult> BindMessage([FromRoute] string channelId, [FromRoute] string messageId, [FromBody] BindMessageRequest request)
        {
            request.ChannelID = channelId;
            request.MessageID = messageId;
            return Ok(await _messageService.BindMessageAsync(request));
        }

        [HttpDelete("{channelId}/messages/{messageId}")]
        public async Task<IActionResult> DeleteMessage([FromRoute] string channelId, [FromRoute] string messageId, [FromBody] DeleteMessageRequest request)
        {
            request.ChannelID = channelId;
            request.ID = messageId;
            return Ok(await _messageService.DeleteAsync(request));
        }

        [HttpPatch("{channelId}/messages/{messageId}/mask")]
        public async Task<IActionResult> MaskMessage([FromRoute] string channelId, [FromRoute] string messageId, [FromBody] MaskMessageRequest request)
        {
            request.ChannelID = channelId;
            request.MessageID = messageId;
            request.Mask = true;
            return Ok(await _messageService.MaskMessageAsync(request));
        }

        [HttpPatch("{channelId}/messages/{messageId}/unmask")]
        public async Task<IActionResult> UnmaskMessage([FromRoute] string channelId, [FromRoute] string messageId, [FromBody] MaskMessageRequest request)
        {
            request.ChannelID = channelId;
            request.MessageID = messageId;
            request.Mask = false;
            return Ok(await _messageService.MaskMessageAsync(request));
        }

        [HttpPatch("{channelId}/messages/{messageId}/hide")]
        public async Task<IActionResult> HideMessage([FromRoute] string channelId, [FromRoute] string messageId, [FromBody] HideMessageRequest request)
        {
            request.ChannelID = channelId;
            request.MessageID = messageId;
            request.Hide = true;
            return Ok(await _messageService.HideMessageAsync(request));
        }

        [HttpPatch("{channelId}/messages/{messageId}/unhide")]
        public async Task<IActionResult> UnhideMessage([FromRoute] string channelId, [FromRoute] string messageId, [FromBody] HideMessageRequest request)
        {
            request.ChannelID = channelId;
            request.MessageID = messageId;
            request.Hide = false;
            return Ok(await _messageService.HideMessageAsync(request));
        }

        [HttpPost("{channelId}/messages/{messageId}/delivered")]
        public async Task<IActionResult> MarkMessageAsDelivered([FromRoute] string channelId, [FromRoute] string messageId, [FromBody] MarkMessageRequest request)
        {
            request.ChannelID = channelId;
            request.MessageID = messageId;
            request.Status = Domain.MessageStatus.Delivered;
            return Ok(await _messageService.MarkMessageAsync(request));
        }

        [HttpPost("{channelId}/messages/{messageId}/seen")]
        public async Task<IActionResult> MarkMessageAsSeen([FromRoute] string channelId, [FromRoute] string messageId, [FromBody] MarkMessageRequest request)
        {
            request.ChannelID = channelId;
            request.MessageID = messageId;
            request.Status = Domain.MessageStatus.Seen;
            return Ok(await _messageService.MarkMessageAsync(request));
        }
    }
}
