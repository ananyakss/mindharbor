using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NotesApi.Models;

public class Emotion
{
    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("intensity")]
    public int Intensity { get; set; }
}

public class CbtEntry
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    [BsonElement("userId")]
    public string UserId { get; set; } = string.Empty;

    [BsonElement("date")]
    public DateTime Date { get; set; }

    [BsonElement("situation")]
    public string Situation { get; set; } = string.Empty;

    [BsonElement("automaticThought")]
    public string AutomaticThought { get; set; } = string.Empty;

    [BsonElement("emotions")]
    public List<Emotion> Emotions { get; set; } = [];

    [BsonElement("cognitiveDistortions")]
    public List<string> CognitiveDistortions { get; set; } = [];

    /// <summary>Step 5 — challenge thoughts (legacy evidenceFor maps here when migrating).</summary>
    [BsonElement("challengeThoughts")]
    public string ChallengeThoughts { get; set; } = string.Empty;

    /// <summary>Step 6 — reframe (legacy rationalResponse).</summary>
    [BsonElement("reframeThoughts")]
    public string ReframeThoughts { get; set; } = string.Empty;

    [BsonElement("actionPlan")]
    public string ActionPlan { get; set; } = string.Empty;

    [BsonElement("evidenceFor")]
    public string EvidenceFor { get; set; } = string.Empty;

    [BsonElement("evidenceAgainst")]
    public string EvidenceAgainst { get; set; } = string.Empty;

    [BsonElement("rationalResponse")]
    public string RationalResponse { get; set; } = string.Empty;

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; }

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; }
}

