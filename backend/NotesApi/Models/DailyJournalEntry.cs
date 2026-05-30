using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NotesApi.Models;

public class DailyJournalEntry
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    [BsonElement("userId")]
    public string UserId { get; set; } = string.Empty;

    [BsonElement("date")]
    public DateTime Date { get; set; }

    [BsonElement("entryText")]
    public string EntryText { get; set; } = string.Empty;

    [BsonElement("sleepHours")]
    public double SleepHours { get; set; }

    [BsonElement("waterGlasses")]
    public int WaterGlasses { get; set; }

    [BsonElement("foodNotes")]
    public string FoodNotes { get; set; } = string.Empty;

    [BsonElement("mood")]
    public string Mood { get; set; } = string.Empty;

    [BsonElement("moodEmoji")]
    public string? MoodEmoji { get; set; }

    [BsonElement("gratitude")]
    public List<string> Gratitude { get; set; } = [];

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; }

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; }
}

