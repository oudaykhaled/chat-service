using ChatService.Domain.Request;
using ChatService.Domain.Response;
using ChatService.Persistence;
using ChatService.Persistence.Exception;

namespace ChatService.Application.Service
{
    public class MemberService : IMemberService
    {
        private readonly IRepository<Member> _memberRepository;

        public MemberService(IRepository<Member> memberRepository)
        {
            _memberRepository = memberRepository;
        }

        public async Task<IdentityResponse> CreateAsync(CreateMemberRequest request)
        {
            IdentityResponse response = new IdentityResponse();

            var query = _memberRepository.Collection().WhereEqualTo(nameof(Member.Nickname), request.Nickname);
            bool exists = await _memberRepository.QueryExistsAsync(query);
            if (!exists)
            {
                Member member = new Member();

                member.Guid = Guid.NewGuid().ToString();
                member.CreatedAt = DateTime.UtcNow;
                member.Name = request.Name;
                member.Nickname = request.Nickname;
                member.Type = request.Type;
                member.Tag = request.Tag;

                response.ID = await _memberRepository.AddAsync(member);
                return response;
            }
            throw new BadRequestException($"Member '{request.Nickname}' already exists.");
        }

        public async Task DeleteAsync(DeleteMemberRequest request)
        {
            var member = await _memberRepository.GetByIdAsync(request.ID);
            if (member != null)
            {
                await _memberRepository.DeleteAsync(member);
            }
        }

        public async Task<PagedList<MemberResponse>> GetPagedAsync(GetPagedRequest request)
        {
            PagedList<MemberResponse> response = new PagedList<MemberResponse>();
            var pagedCollection = await _memberRepository.GetPagedAsync(request.PageSize, request.StartAfterDocumentId);
            if (pagedCollection != null)
            {
                if (pagedCollection.Items != null && pagedCollection.Items.Any())
                {
                    response.Items = pagedCollection.Items.Select(x => new MemberResponse()
                    {
                        ID = x.ID,
                        Name = x.Name,
                        Nickname = x.Nickname,
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

        public async Task<MemberResponse> GetByIdAsync(MemberRequest request)
        {
            MemberResponse response = new MemberResponse();
            var model = await _memberRepository.GetByIdAsync(request.ID);
            if (model != null)
            {
                response.ID = model.ID;
                response.Name = model.Name;
                response.Nickname = model.Nickname;
                response.Type = model.Type;
                response.Tag = model.Tag;
                response.CreatedAt = model.CreatedAt;
            }
            return response;
        }

        public async Task<IdentityResponse> DeactivateAsync(DeactivateMemberRequest request)
        {
            IdentityResponse response = new IdentityResponse();

            var existingModel = await _memberRepository.GetByIdAsync(request.ID);
            if (existingModel != null)
            {
                response.ID = request.ID;

                existingModel.IsActive = false;
                existingModel.ModifiedAt = DateTime.UtcNow;

                await _memberRepository.UpdateAsync(existingModel);
            }

            return response;
        }

        public async Task<IdentityResponse> UpdateAsync(UpdateMemberRequest request)
        {
            IdentityResponse response = new IdentityResponse();

            var existingModel = await _memberRepository.GetByIdAsync(request.ID);
            if (existingModel != null)
            {
                bool exists = false;
                if (!existingModel.Nickname.Equals(request.Nickname))
                {
                    var query = _memberRepository.Collection().WhereEqualTo(nameof(Member.Nickname), request.Nickname);
                    exists = await _memberRepository.QueryExistsAsync(query);
                }

                if (!exists)
                {
                    response.ID = request.ID;

                    existingModel.Name = request.Name;
                    existingModel.Nickname = request.Nickname;
                    existingModel.Type = request.Type;
                    existingModel.Tag = request.Tag;
                    existingModel.ModifiedAt = DateTime.UtcNow;

                    await _memberRepository.UpdateAsync(existingModel);
                }
                else
                {
                    throw new BadRequestException($"Member '{request.Nickname}' already exists.");
                }
            }

            return response;
        }
    }
}
