using ChatService.Domain.Request;
using ChatService.Domain.Response;

namespace ChatService.Application.Service
{
    public interface IMessageService
    {
        Task<IdentityResponse> AddMessageAsync(AddMessageRequest request);
        Task<IdentityResponse> EditMessageAsync(EditMessageRequest request);
        Task<IdentityResponse> ModerateMessageAsync(ModerateMessageRequest request);
        Task<IdentityResponse> SideEffectAsync(SideEffectRequest request);
        Task<MessageResponse> PopSideEffectAsync(PopSideEffectRequest request);
        Task<MessageResponse> PopMessageAsync(PopMessageRequest request);
        Task<IdentityResponse> ReplyToMessageAsync(ReplyToMessageRequest request);
        Task<IdentityResponse> BindMessageAsync(BindMessageRequest request);
        Task<IdentityResponse> DeleteAsync(DeleteMessageRequest request);
        Task<IdentityResponse> MaskMessageAsync(MaskMessageRequest request);
        Task<IdentityResponse> HideMessageAsync(HideMessageRequest request);
        Task<IdentityResponse> MarkMessageAsync(MarkMessageRequest request);
    }
}
