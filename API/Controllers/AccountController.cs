using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AccountController(
    UserManager<AppUser> userManager,
    ITokenService tokenService)
    : BaseApiController
{
    [HttpPost("register")] // api/account/register
    public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
    {
        var user = new AppUser
        {
            DisplayName = registerDto.DisplayName,
            Email = registerDto.Email,
            UserName = registerDto.Email,
            Member = new Member
            {
                DisplayName = registerDto.DisplayName,
                Gender = registerDto.Gender,
                City = registerDto.City,
                Country = registerDto.Country,
                DateOfBirth = registerDto.DateOfBirth
            }
        };

        var result = await userManager.CreateAsync(user, registerDto.Password);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("identity", error.Description);
            }
            return ValidationProblem();
        }

        await userManager.AddToRoleAsync(user, "Member");

        await SetRefreshTokenCookie(user);

        return await user.ToDto(tokenService);
    }

    [HttpPost("login")] // api/account/login
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
    {
        var user = await userManager.FindByEmailAsync(loginDto.Email);

        if (user == null)
        {
            return Unauthorized("Invalid email address");
        }

        var result = await userManager.CheckPasswordAsync(user, loginDto.Password);
        if (!result)
        {
            return Unauthorized("Invalid password");
        }

        await SetRefreshTokenCookie(user);

        return await user.ToDto(tokenService);
    }

    [HttpPost("refresh-token")] // api/account/refresh-token
    public async Task<ActionResult<UserDto>> RefreshToken()
    { 
        var refreshToken = Request.Cookies["refreshToken"];
        if (string.IsNullOrEmpty(refreshToken))
        {
            return NoContent();
        }

        // Validate the refresh token with existing token and not expired
        var user =
            await userManager.Users
                .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken
                    && u.RefreshTokenExpiry > DateTime.UtcNow);

        if (user == null)
        {
            return Unauthorized();
        }
        
        // Set new refresh token cookie
        await SetRefreshTokenCookie(user);

        return await user.ToDto(tokenService);
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<ActionResult> Logout()
    {
        await userManager.Users
            .Where(x => x.Id == User.GetMemberId())
            .ExecuteUpdateAsync(setters =>
                setters.SetProperty(x => x.RefreshToken, _ => null)
                       .SetProperty(x => x.RefreshTokenExpiry, _ => DateTime.MinValue));

        Response.Cookies.Delete("refreshToken");
        return Ok();
    }

    private async Task SetRefreshTokenCookie(AppUser user)
    {
        var refreshToken = tokenService.GenerateRefreshToken();
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        await userManager.UpdateAsync(user);

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Expires = user.RefreshTokenExpiry,
            Secure = true, // Set to true if using HTTPS
            SameSite = SameSiteMode.Strict // Adjust as needed
        };

        Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
    }
}
