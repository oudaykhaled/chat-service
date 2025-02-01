using ChatService.Domain.Request;
using ChatService.Domain.Response;
using ChatService.Persistence;

namespace ChatService.Application.Service
{
    public interface IChannelService
    {
        Task<IdentityResponse> CreateAsync(CreateChannelRequest request);
        Task<IdentityResponse> DeactivateAsync(DeactivateChannelRequest request);
        Task<IdentityResponse> AddMembersAsync(AddChannelMembersRequest request);
        Task<PagedList<ChannelResponse>> GetPagedAsync(GetPagedRequest request);
    }
}
