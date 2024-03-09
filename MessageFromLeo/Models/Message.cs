using MongoDB.Bson;

namespace MessageFromLeo.Models;
public class Message
{
    public ObjectId Id { get; set; }
    public string To { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}