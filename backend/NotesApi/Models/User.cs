using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NotesApi.Models;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("username")]
    public string Username { get; set; } = string.Empty;

    [BsonElement("email")]
    public string Email { get; set; } = string.Empty;

    [BsonElement("hashedPassword")]
    public string HashedPassword { get; set; } = string.Empty;

    [BsonElement("initialPasswordEncrypted")]
    public string? InitialPasswordEncrypted { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; }

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; }

    [BsonElement("loginStreak")]
    public int LoginStreak { get; set; }

    [BsonElement("lastLoginUtc")]
    public DateTime? LastLoginUtc { get; set; }

    [BsonElement("gamificationPoints")]
    public int GamificationPoints { get; set; }
}

