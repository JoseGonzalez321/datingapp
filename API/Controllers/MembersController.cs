using System.Security.Claims;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class MembersController(IMemberRepository memberRepository) : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Member>>> GetMembers()
    {
        return Ok(await memberRepository.GetMembersAsync());
    }

    [Authorize]
    [HttpGet("{id}")] // api/members/bob-id
    public async Task<ActionResult<Member>> GetMember(string id)
    {
        var member = await memberRepository.GetMemberByIdAsync(id);
        if (member == null) return NotFound();
        return member;
    }

    [HttpGet("{id}/photos")]
    public async Task<ActionResult<IReadOnlyList<Photo>>> GetPhotos(string id)
    {
        var photos = await memberRepository.GetPhotosAsync(id);
        if (photos == null) return NotFound();
        return Ok(photos);
    }
    
    [HttpPut]
    public async Task<IActionResult> UpdateMember(MemberUpdateDto memberUpdateDto)
    {
        // var memberId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        // if (memberId == null) return BadRequest("Oops - no member ID found");
        var memberId = User.GetMemberId();

        var member = await memberRepository.GetMemberForUpdateAsync(memberId);
        if (member == null) return NotFound("Member not found");

        member.DisplayName = memberUpdateDto.DisplayName ?? member.DisplayName;
        member.Description = memberUpdateDto.Description ?? member.Description;
        member.City = memberUpdateDto.City ?? member.City;
        member.Country = memberUpdateDto.Country ?? member.Country;

        // Update the member in the repository
        member.User.DisplayName = memberUpdateDto.DisplayName ?? member.User.DisplayName;

        memberRepository.Update(member);

        if (await memberRepository.SaveAllAsync()) return NoContent();

        return BadRequest("Failed to update the member");
    }
}