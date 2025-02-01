using ChatService.Application.Service;
using ChatService.Domain;
using ChatService.Domain.Request;
using ChatService.Domain.Response;
using ChatService.Persistence;
using Microsoft.AspNetCore.Mvc;

namespace ChatService.API.Controllers
{
    [Route("api/members")]
    [ApiController]
    public class MemberController : BaseController
    {
        private readonly IMemberService _memberService;

        public MemberController(IMemberService memberService)
        {
            _memberService = memberService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateMemberRequest request)
        {
            return Ok(await _memberService.CreateAsync(request));
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateMemberRequest request)
        {
            return Ok(await _memberService.UpdateAsync(request));
        }

        [HttpPatch("{memberId}/deactivate")]
        public async Task<IActionResult> Deactivate(DeactivateMemberRequest request)
        {
            return Ok(await _memberService.DeactivateAsync(request));
        }

        [HttpDelete("{memberId}")]
        public async Task<IActionResult> Delete(DeleteMemberRequest request)
        {
            await _memberService.DeleteAsync(request);
            return Ok();
        }

        [HttpGet("{memberId}")]
        public async Task<ActionResult<MemberResponse>> GetById(MemberRequest request)
        {
            return Ok(await _memberService.GetByIdAsync(request));
        }

        [HttpGet]
        public async Task<ActionResult<PagedList<MemberResponse>>> GetAll([FromQuery]GetPagedRequest request)
        {
            return Ok(await _memberService.GetPagedAsync(request));
        }
    }
}
