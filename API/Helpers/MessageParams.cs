using System;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace API.Helpers;

public class MessageParams : PagingParams
{
    public string? MemberId { get; set; }
    public string Container { get; set; } = "Inbox";
}
