using System;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

// UnitOfWork is responsible for coordinating the repostories and single transactions
// It ensures that all changes are committed to the database in a single transaction
// This is useful for maintaining data integrity and consistency across multiple repositories
// All repositories are accessed through the UnitOfWork
// and all you need to inject is the UnitOfWork into your controllers or services 
// instead of individual repositories

public class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    private IMemberRepository? _memberRepository;
    private IMessageRepository? _messageRepository;
    private ILikesRepository? _likesRepository;

    public IMemberRepository MemberRepository =>
        _memberRepository ??= new MemberRepository(context);

    public IMessageRepository MessageRepository =>
        _messageRepository ??= new MessageRepository(context);

    public ILikesRepository LikesRepository =>
        _likesRepository ??= new LikesRepository(context);

    public async Task<bool> Complete()
    {
        // This method ensures atomicity of the operations
        // All changes are committed or none are
        try
        {
            return await context.SaveChangesAsync() > 0;
        }
        catch (DbUpdateException ex)
        {
            throw new Exception("An error occurred while saving changes to the database.", ex);
        }
    }

    public bool HasChanges()
    {
        return context.ChangeTracker.HasChanges();
    }
}
