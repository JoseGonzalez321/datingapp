using System.Security.Claims;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class MembersController(
    IMemberRepository memberRepository,
    IPhotoService photoService)
    : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Member>>> GetMembers(
        [FromQuery] MemberParams memberParams
    )
    {
        memberParams.CurrentMemberId = User.GetMemberId();
        return Ok(await memberRepository.GetMembersAsync(memberParams));
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

    [HttpPost("add-photo")]
    public async Task<ActionResult<Photo>> AddPhoto([FromForm] IFormFile file)
    {
        var memberId = User.GetMemberId();
        var member = await memberRepository.GetMemberForUpdateAsync(memberId);
        if (member == null) return NotFound("Member not found");

        if (file == null || file.Length == 0)
        {
            return BadRequest("No photo file provided");
        }

        var result = await photoService.UploadPhotoAsync(file);
        if (result.Error != null)
        {
            return BadRequest(result.Error.Message);
        }

        var photo = new Photo
        {
            Url = result.SecureUrl.AbsoluteUri,
            PublicId = result.PublicId,
            MemberId = memberId,
        };

        if (member.ImageUrl == null)
        {
            member.ImageUrl = photo.Url;
            member.User.ImageUrl = photo.Url;
        }

        member.Photos.Add(photo);

        if (await memberRepository.SaveAllAsync())
            return photo;

        return BadRequest("Problem adding photo");
    }

    [HttpPut("set-main-photo/{photoId}")]
    public async Task<IActionResult> SetMainPhoto(int photoId)
    {
        var member = await memberRepository.GetMemberForUpdateAsync(User.GetMemberId());
        if (member == null) return NotFound("Cannot get member from token");

        var photo = member.Photos.SingleOrDefault(p => p.Id == photoId);
        if (photo == null) return NotFound("Photo not found");

        if (member.ImageUrl == photo.Url)
        {
            return BadRequest("This is already the main photo");
        }

        member.ImageUrl = photo.Url;
        member.User.ImageUrl = photo.Url;

        if (await memberRepository.SaveAllAsync())
        {
            return NoContent();
        }

        return BadRequest("Problem setting main photo");
    }

    [HttpDelete("delete-photo/{photoId}")]
    public async Task<IActionResult> DeletePhoto(int photoId)
    {
        var member = await memberRepository.GetMemberForUpdateAsync(User.GetMemberId());
        if (member == null) return NotFound("Cannot get member from token");

        var photo = member.Photos.SingleOrDefault(p => p.Id == photoId);
        if (photo == null || photo.Url == member.ImageUrl)
        {
            return BadRequest("This main photo cannot be deleted");
        }

        if (photo.PublicId != null)
        {
            var result = await photoService.DeletePhotoAsync(photo.PublicId);
            if (result.Error != null)
            {
                return BadRequest(result.Error.Message);
            }
        }

        member.Photos.Remove(photo);

        if (await memberRepository.SaveAllAsync())
        {
            return NoContent();
        }

        return BadRequest("Problem deleting photo");
    }
}

