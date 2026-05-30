using MongoDB.Driver;
using NotesApi.Models;

namespace NotesApi.Services;

public class MoodService
{
    private const string CollectionName = "moodEntries";
    private readonly IMongoCollection<MoodEntry> _moods;

    public MoodService(MongoDbService mongoDb)
    {
        _moods = mongoDb.GetCollection<MoodEntry>(CollectionName);
    }

    public async Task<List<MoodEntry>> GetAllForUserAsync(string userId) =>
        await _moods.Find(m => m.UserId == userId)
            .SortByDescending(m => m.RecordedAt)
            .ToListAsync();

    public async Task<List<MoodEntry>> GetForUserSinceAsync(string userId, DateTime sinceUtc) =>
        await _moods.Find(m => m.UserId == userId && m.RecordedAt >= sinceUtc)
            .SortBy(m => m.RecordedAt)
            .ToListAsync();

    public async Task<MoodEntry?> GetByIdAsync(string userId, string id) =>
        await _moods.Find(m => m.Id == id && m.UserId == userId).FirstOrDefaultAsync();

    public async Task<MoodEntry> CreateAsync(MoodEntry entry)
    {
        entry.CreatedAt = DateTime.UtcNow;
        entry.UpdatedAt = DateTime.UtcNow;
        await _moods.InsertOneAsync(entry);
        return entry;
    }

    public async Task<MoodEntry?> UpdateAsync(string userId, string id, MoodEntry updated)
    {
        updated.UpdatedAt = DateTime.UtcNow;
        var result = await _moods.FindOneAndReplaceAsync(
            m => m.Id == id && m.UserId == userId,
            updated,
            new FindOneAndReplaceOptions<MoodEntry> { ReturnDocument = ReturnDocument.After });
        return result;
    }

    public async Task<bool> DeleteAsync(string userId, string id)
    {
        var result = await _moods.DeleteOneAsync(m => m.Id == id && m.UserId == userId);
        return result.DeletedCount > 0;
    }
}
