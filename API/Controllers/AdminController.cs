using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AdminController(UserManager<AppUser> userManager) 
    : BaseApiController
{
    [Authorize(Policy = "RequireAdminRole")]
    [HttpGet("users-with-roles")]
    public async Task<ActionResult> GetUsersWithRoles()
    {
        var usersWithRoles =
            await userManager.Users.Select(u => new
            {
                u.Id,
                u.Email,
                Roles = userManager.GetRolesAsync(u).Result
            })
            .OrderBy(u => u.Email)
            .ToListAsync();

        return Ok(usersWithRoles);
    }

    [Authorize(Policy = "RequireAdminRole")]
    [HttpPost("edit-roles/{userId}")]
    public async Task<ActionResult<IList<string>>> EditRoles(string userId, [FromQuery] string roles)
    {
        // Validate the roles 
        var selectedRoles = string.IsNullOrEmpty(roles) ? [] : roles.Split(',');
        if (selectedRoles.Length == 0)
        {
            return BadRequest("You must select at least one role");
        }

        // Validate the user exists
        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound("User not found");
        }

        // Adding new roles to user
        var userRoles = await userManager.GetRolesAsync(user);        
        var result = await userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));        

        if (!result.Succeeded)
        {
            return BadRequest("Failed to add to roles");
        }

        // Removing roles from user
        result = await userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));
        if (!result.Succeeded)
        {
            return BadRequest("Failed to remove from roles");
        }

        return Ok(await userManager.GetRolesAsync(user));

    }
    
    [Authorize(Policy = "ModeratePhotoRole")]
    [HttpGet("photos-to-moderate")]
    public ActionResult GetPhotosForModeration()
    {        
        return Ok("Admins or moderators can see this");
    }
}