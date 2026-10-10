using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NotesApi.Models;

public class JournalEntry
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    [BsonElement("userId")]
    public string UserId { get; set; } = string.Empty;

    [BsonElement("entryText")]
    public string EntryText { get; set; } = string.Empty;

    [BsonElement("allowAnalysisConsent")]
    public bool AllowAnalysisConsent { get; set; }

    [BsonElement("mlSentiment")]
    public string? MlSentiment { get; set; }

    [BsonElement("mlAssessedAt")]
    public DateTime? MlAssessedAt { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; }

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; }
}
