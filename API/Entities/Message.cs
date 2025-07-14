using System;

namespace API.Entities;

public class Message
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public required string Content { get; set; }
    public DateTime? DateRead { get; set; }
    public DateTime MessageSent { get; set; } = DateTime.Now;
    public bool SenderDeleted { get; set; }
    public bool RecipientDeleted { get; set; }

    // navigation properties
    public required string SenderId { get; set; } = string.Empty;
    public Member Sender { get; set; } = null!;
    public required string RecipientId { get; set; } = string.Empty;
    public Member Recipient { get; set; } = null!;
}
