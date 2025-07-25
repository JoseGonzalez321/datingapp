
using API.Entities;
using API.Helpers;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class MemberRepository(AppDbContext context) : IMemberRepository
{
    public async Task<Member?> GetMemberByIdAsync(string id)
    {
        return await context.Members.FindAsync(id);
    }

    public async Task<Member?> GetMemberForUpdateAsync(string id)
    {
        return await context.Members
            .Include(m => m.User)
            .Include(m => m.Photos)
            .SingleOrDefaultAsync(m => m.Id == id);
    }

    public async Task<PaginatedResult<Member>> GetMembersAsync(
        MemberParams memberParams)
    {
        var query = context.Members.AsQueryable();
        query = query.Where(m => m.Id != memberParams.CurrentMemberId);

        if (memberParams.Gender != null)
        {
            query = query.Where(m => m.Gender == memberParams.Gender);
        }

        var minDob = GetMinDob(memberParams);
        var maxDob = GetMaxDob(memberParams);

        query = query.Where(m => m.DateOfBirth >= minDob && m.DateOfBirth <= maxDob);

        query = memberParams.OrderBy switch
        {
            "created" => query.OrderByDescending(m => m.Created),
            "lastActive" => query.OrderByDescending(m => m.LastActive),
            _ => query.OrderByDescending(m => m.LastActive)
        };

        return await PaginationHelper.CreateAsync(
            query, memberParams.PageNumber, memberParams.PageSize);
    }
    
    public async Task<IReadOnlyList<Photo>> GetPhotosAsync(string memberId)
    {
        return await context.Members
            .Where(m => m.Id == memberId)
            .SelectMany(m => m.Photos)
            .ToListAsync();
    }

    public void Update(Member member)
    {
        context.Entry(member).State = EntityState.Modified;
    }
    
    private DateOnly GetMaxDob(MemberParams memberParams)
    {
        return DateOnly.FromDateTime(DateTime.Today.AddYears(-memberParams.MinAge));    
    }

    private DateOnly GetMinDob(MemberParams memberParams)
    {
        return DateOnly.FromDateTime(DateTime.Today.AddYears(-memberParams.MaxAge - 1));
    }
}
