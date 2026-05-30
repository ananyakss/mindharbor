using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NotesApi.Models;

public class MoodEntry
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    [BsonElement("userId")]
    public string UserId { get; set; } = string.Empty;

    [BsonElement("recordedAt")]
    public DateTime RecordedAt { get; set; }

    [BsonRepresentation(BsonType.String)]
    [BsonElement("mood")]
    public MoodType Mood { get; set; }

    [BsonElement("moodIntensity")]
    public int MoodIntensity { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; }

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; }
}
