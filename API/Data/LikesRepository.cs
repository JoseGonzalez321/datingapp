using API.Entities;
using API.Helpers;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class LikesRepository(AppDbContext context) : ILikesRepository
{
    public void AddLike(MemberLike like)
    {
        context.Likes.Add(like);
    }

    public void DeleteLike(MemberLike like)
    {
        context.Likes.Remove(like);
    }

    public async  Task<IReadOnlyList<string>> GetCurrentMemberLikesIds(string memberId)
    {
        // Get the MemberIds of members that the current member has liked
       return await context.Likes
               .Where(l => l.SourceMemberId == memberId)
               .Select(l => l.TargetMemberId)
               .ToListAsync();
    }

    public async Task<MemberLike?> GetMemberLike(string sourceMemberId, string likedMemberId)
    {
        return await context.Likes.FindAsync(sourceMemberId, likedMemberId);
    }

    public async Task<PaginatedResult<Member>> GetMemberLikes(LikesParams likesParams)
    {
        var memberId = likesParams.MemberId;
        var query = context.Likes.AsQueryable();
        IQueryable<Member> result;

        switch (likesParams.Predicate)
        {
            case "liked":
                result = query
                    .Where(l => l.SourceMemberId == memberId)
                    .Select(l => l.TargetMember);
                break;
                        
            case "likedBy":
                result = query
                    .Where(l => l.TargetMemberId == memberId)
                    .Select(l => l.SourceMember);
                break;
            default:
                var likeIds = await GetCurrentMemberLikesIds(memberId);

                result = query 
                        .Where(x => x.TargetMemberId == memberId
                            && likeIds.Contains(x.SourceMemberId))
                        .Select(x => x.SourceMember);
                break;
        }

        return await PaginationHelper.CreateAsync(
            result, likesParams.PageNumber, likesParams.PageSize);
    }

    public async Task<bool> SaveAllChanges()
    {
        return await context.SaveChangesAsync() > 0;
    }
}
