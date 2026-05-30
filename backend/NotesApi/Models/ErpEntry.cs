using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NotesApi.Models;

public class ErpEntry
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    [BsonElement("userId")]
    public string UserId { get; set; } = string.Empty;

    [BsonElement("date")]
    public DateTime Date { get; set; }

    [BsonElement("trigger")]
    public string Trigger { get; set; } = string.Empty;

    [BsonElement("anxietyLevel")]
    public int AnxietyLevel { get; set; }

    [BsonElement("exposureActivity")]
    public string ExposureActivity { get; set; } = string.Empty;

    [BsonElement("resistedCompulsion")]
    public bool ResistedCompulsion { get; set; }

    [BsonElement("reflection")]
    public string Reflection { get; set; } = string.Empty;

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; }

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; }
}
