using API.DTOs;
using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;


namespace API.Data;

public class Seed
{
    public static async Task SeedUsers(UserManager<AppUser> userManager)
    {
        // UserManager<T> is like the DbContext, except with a focus on user management.
        // It provides methods for creating, updating, deleting, and querying users.
        if (await userManager.Users.AnyAsync()) return;

        var memberData = await File.ReadAllTextAsync("Data/UserSeedData.json");
        var members = JsonSerializer.Deserialize<List<SeedUserDto>>(memberData);

        if (members == null)
        {
            Console.WriteLine("No members found in the seed data.");
            return;
        }

        foreach (var member in members)
        {
            var user = new AppUser
            {
                Id = member.Id,
                Email = member.Email,
                UserName = member.Email,
                DisplayName = member.DisplayName,
                ImageUrl = member.ImageUrl,
                Member = new Member
                {
                    Id = member.Id,
                    DisplayName = member.DisplayName,
                    Description = member.Description,
                    DateOfBirth = member.DateOfBirth,
                    ImageUrl = member.ImageUrl,
                    Gender = member.Gender,
                    City = member.City,
                    Country = member.Country,
                    Created = member.Created,
                    LastActive = member.LastActive
                }
            };

            user.Member.Photos.Add(new Photo
            {
                Url = member.ImageUrl!,
                MemberId = member.Id,
            });

            var result = await userManager.CreateAsync(user, "Pa$$w0rd");
            if (!result.Succeeded)
            {
                Console.WriteLine($"Error creating user {member.Email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
            await userManager.AddToRoleAsync(user, "Member");
        }

        // Create an admin user outside the loop
        // This ensures that the admin user is created only once
        // and not part of the member seed data.
        var admin = CreateAdminUser();

        var resultAdmin = await userManager.CreateAsync(admin, "Pa$$w0rd");
        if (!resultAdmin.Succeeded)
        {
            Console.WriteLine($"Error creating admin user: {string.Join(", ", resultAdmin.Errors.Select(e => e.Description))}");            
        }

        var resultAdminRoles = await userManager.AddToRolesAsync(admin, ["Admin", "Moderator"]);
        if (!resultAdminRoles.Succeeded)
        {
            Console.WriteLine($"Error adding admin user to roles: {string.Join(", ", resultAdminRoles.Errors.Select(e => e.Description))}");            
        }
    }

    private static AppUser CreateAdminUser() => new()
    {
        UserName = "admin@test.com",
        Email = "admin@test.com",
        DisplayName = "Admin",
        Id = "admin-id"
    };        
}
