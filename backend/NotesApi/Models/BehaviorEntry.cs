using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NotesApi.Models;

public class BehaviorEntry
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    [BsonElement("userId")]
    public string UserId { get; set; } = string.Empty;

    /// <summary>Calendar day for the log (UTC date stored as instant at midnight UTC).</summary>
    [BsonElement("day")]
    public DateTime Day { get; set; }

    [BsonElement("sleep")]
    public int Sleep { get; set; }

    [BsonElement("water")]
    public int Water { get; set; }

    [BsonElement("meals")]
    public MealInfo Meals { get; set; } = new();

    [BsonElement("exerciseHours")]
    public int ExerciseHours { get; set; }

    [BsonElement("steps")]
    public int Steps { get; set; }

    [BsonElement("diet")]
    public int Diet { get; set; }

    [BsonElement("social")]
    public int Social { get; set; }

    [BsonElement("hobbies")]
    public int Hobbies { get; set; }

    [BsonElement("stress")]
    public int Stress { get; set; }

    [BsonElement("mlModelRiskLevel")]
    public string? MlModelRiskLevel { get; set; }

    [BsonElement("mlAssessedAt")]
    public DateTime? MlAssessedAt { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; }

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; }
}

public class MealInfo
{
    [BsonElement("breakfast")]
    public bool Breakfast { get; set; }

    [BsonElement("lunch")]
    public bool Lunch { get; set; }

    [BsonElement("dinner")]
    public bool Dinner { get; set; }
}
