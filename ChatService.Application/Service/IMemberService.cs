using ChatService.Domain.Request;
using ChatService.Domain.Response;
using ChatService.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatService.Application.Service
{
    public interface IMemberService
    {
        Task<IdentityResponse> CreateAsync(CreateMemberRequest request);
        Task<IdentityResponse> UpdateAsync(UpdateMemberRequest request);
        Task<IdentityResponse> DeactivateAsync(DeactivateMemberRequest request);
        Task DeleteAsync(DeleteMemberRequest request);
        Task<MemberResponse> GetByIdAsync(MemberRequest request);
        Task<PagedList<MemberResponse>> GetPagedAsync(GetPagedRequest request);
    }
}
