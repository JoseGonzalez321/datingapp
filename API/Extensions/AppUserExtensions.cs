using System;
using API.DTOs;
using API.Entities;
using API.Interfaces;

namespace API.Extensions;

public static class AppUserExtensions
{
   public static async Task<UserDto> ToDto(this AppUser user,  ITokenService tokenService)
   {
      if (tokenService == null) throw new ArgumentNullException(nameof(tokenService));

      return new UserDto
      {
         Id = user.Id,
         Email = user.Email!,
         DisplayName = user.DisplayName,
         ImageUrl = user.ImageUrl,
         Token = await tokenService.CreateToken(user)
      };
   }
}
