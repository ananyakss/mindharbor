using MongoDB.Driver;
using NotesApi.Models;

namespace NotesApi.Services;

public class BehaviorEntryService
{
    private const string CollectionName = "behavior_logs";
    private readonly IMongoCollection<BehaviorEntry> _entries;

    public BehaviorEntryService(MongoDbService mongoDb)
    {
        _entries = mongoDb.GetCollection<BehaviorEntry>(CollectionName);
    }

    public async Task<List<BehaviorEntry>> GetAllForUserAsync(string userId) =>
        await _entries.Find(e => e.UserId == userId)
            .SortByDescending(e => e.Day)
            .ToListAsync();

    public async Task<List<BehaviorEntry>> GetForUserSinceAsync(string userId, DateTime sinceUtc) =>
        await _entries.Find(e => e.UserId == userId && e.Day >= sinceUtc)
            .SortByDescending(e => e.Day)
            .ToListAsync();

    public async Task<BehaviorEntry?> GetByIdAsync(string userId, string id) =>
        await _entries.Find(e => e.Id == id && e.UserId == userId).FirstOrDefaultAsync();

    public async Task<BehaviorEntry> CreateAsync(BehaviorEntry entry)
    {
        entry.CreatedAt = DateTime.UtcNow;
        entry.UpdatedAt = DateTime.UtcNow;
        await _entries.InsertOneAsync(entry);
        return entry;
    }

    public async Task<BehaviorEntry?> UpdateAsync(string userId, string id, BehaviorEntry updated)
    {
        updated.UpdatedAt = DateTime.UtcNow;
        var result = await _entries.FindOneAndReplaceAsync(
            e => e.Id == id && e.UserId == userId,
            updated,
            new FindOneAndReplaceOptions<BehaviorEntry> { ReturnDocument = ReturnDocument.After });
        return result;
    }

    public async Task<bool> DeleteAsync(string userId, string id)
    {
        var result = await _entries.DeleteOneAsync(e => e.Id == id && e.UserId == userId);
        return result.DeletedCount > 0;
    }
}
