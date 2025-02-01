using ChatService.Domain.Request;
using ChatService.Domain.Response;
using ChatService.Persistence;
using ChatService.Persistence.Exception;

namespace ChatService.Application.Service
{
    public class ChannelService : IChannelService
    {
        private readonly IRepository<Channel> _channelRepository;
        private readonly IRepository<Member> _memberRepository;
        private readonly IRepository<SessionMember> _sessionMemberRepository;

        public ChannelService(IRepository<Channel> channelRepository, 
            IRepository<Member> memberRepository,
            IRepository<SessionMember> sessionMemberRepository)
        {
            _channelRepository = channelRepository;
            _memberRepository = memberRepository;
            _sessionMemberRepository = sessionMemberRepository;
        }

        public async Task<IdentityResponse> CreateAsync(CreateChannelRequest request)
        {
            IdentityResponse response = new IdentityResponse();

            var query = _channelRepository.Collection().WhereEqualTo(nameof(Channel.Name), request.Name);
            bool exists = await _channelRepository.QueryExistsAsync(query);
            if (!exists)
            {
                var member = await _memberRepository.GetByIdAsync(request.MemberID);
                if (member != null)
                {
                    Channel channel = new Channel();

                    channel.Guid = Guid.NewGuid().ToString();
                    channel.CreatedAt = DateTime.UtcNow;
                    channel.Name = request.Name;
                    channel.Description = request.Description;
                    channel.Type = request.Type;
                    channel.Tag = request.Tag;
                    channel.CreatedBy = request.MemberID;

                    response.ID = await _channelRepository.AddAsync(channel);
                    return response;
                }
                throw new BadRequestException($"Member '{request.MemberID}' does not exists.");
            }
            throw new BadRequestException($"Channel '{request.Name}' already exists.");
        }

        public async Task<IdentityResponse> DeactivateAsync(DeactivateChannelRequest request)
        {
            IdentityResponse response = new IdentityResponse();

            var existingModel = await _channelRepository.GetByIdAsync(request.ID);
            if (existingModel != null)
            {
                response.ID = request.ID;

                existingModel.IsActive = false;
                existingModel.ModifiedAt = DateTime.UtcNow;

                await _channelRepository.UpdateAsync(existingModel);
            }

            return response;
        }

        public async Task<IdentityResponse> AddMembersAsync(AddChannelMembersRequest request)
        {
            IdentityResponse response = new IdentityResponse();

            var query = _sessionMemberRepository.Collection()
                                    .WhereEqualTo(nameof(Channel.ID), request.ChannelID)
                                    .WhereEqualTo(nameof(Member.ID), request.MemberID);
            bool exists = await _sessionMemberRepository.QueryExistsAsync(query);
            if (!exists)
            {
                var member = await _memberRepository.GetByIdAsync(request.MemberID);
                if (member != null)
                {
                    var channel = await _channelRepository.GetByIdAsync(request.ChannelID);
                    if (channel != null)
                    {
                        SessionMember sessionMember = new SessionMember();

                        sessionMember.Guid = Guid.NewGuid().ToString();
                        sessionMember.CreatedAt = DateTime.UtcNow;
                        sessionMember.MemberID = request.MemberID;
                        sessionMember.ChannelID = request.ChannelID;

                        response.ID = await _sessionMemberRepository.AddAsync(sessionMember);
                        return response;
                    }
                    throw new BadRequestException($"Channel '{request.ChannelID}' does not exists.");
                }
                throw new BadRequestException($"Member '{request.MemberID}' does not exists.");
            }
            throw new BadRequestException($"The selected member already exists in the channel.");
        }

        public async Task<PagedList<ChannelResponse>> GetPagedAsync(GetPagedRequest request)
        {
            PagedList<ChannelResponse> response = new PagedList<ChannelResponse>();
            var pagedCollection = await _channelRepository.GetPagedAsync(request.PageSize, request.StartAfterDocumentId);
            if (pagedCollection != null)
            {
                if (pagedCollection.Items != null && pagedCollection.Items.Any())
                {
                    response.Items = pagedCollection.Items.Select(x => new ChannelResponse()
                    {
                        ID = x.ID,
                        Name = x.Name,
                        Description = x.Description,
                        Type = x.Type,
                        Tag = x.Tag,
                        CreatedAt = x.CreatedAt,
                    }).ToList();
                }
                response.TotalCount = pagedCollection.TotalCount;
                response.PageCount = pagedCollection.PageCount;
                response.NextPageToken = pagedCollection.NextPageToken;
            }
            return response;
        }
    }
}
