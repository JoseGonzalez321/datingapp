using System;
using API.Data;
using API.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace API.Helpers;

public class LogUserActivity : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        var resultContext = await next();
        if (context.HttpContext.User.Identity?.IsAuthenticated != true) return;

        var memberId = context.HttpContext.User.GetMemberId();
        var dbContext =
            context.HttpContext.RequestServices
                   .GetRequiredService<AppDbContext>();

        await dbContext.Members
            .Where(m => m.Id == memberId)
            .ExecuteUpdateAsync(m => m.SetProperty(x => x.LastActive, DateTime.UtcNow));
        
        
    }
}
