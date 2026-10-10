using MongoDB.Driver;
using NotesApi.Models;

namespace NotesApi.Services;

public class JournalEntryService
{
    private const string CollectionName = "journalEntries";
    private readonly IMongoCollection<JournalEntry> _entries;

    public JournalEntryService(MongoDbService mongoDb)
    {
        _entries = mongoDb.GetCollection<JournalEntry>(CollectionName);
    }

    public async Task<List<JournalEntry>> GetAllForUserAsync(string userId) =>
        await _entries.Find(e => e.UserId == userId)
            .SortByDescending(e => e.CreatedAt)
            .ToListAsync();

    public async Task<List<JournalEntry>> GetRecentForUserAsync(string userId, int take) =>
        await _entries.Find(e => e.UserId == userId)
            .SortByDescending(e => e.CreatedAt)
            .Limit(take)
            .ToListAsync();

    public async Task<List<JournalEntry>> GetForUserSinceAsync(string userId, DateTime sinceUtc) =>
        await _entries.Find(e => e.UserId == userId && e.CreatedAt >= sinceUtc)
            .SortByDescending(e => e.CreatedAt)
            .ToListAsync();

    public async Task<JournalEntry?> GetByIdAsync(string userId, string id) =>
        await _entries.Find(e => e.Id == id && e.UserId == userId).FirstOrDefaultAsync();

    public async Task<JournalEntry> CreateAsync(JournalEntry entry)
    {
        entry.CreatedAt = DateTime.UtcNow;
        entry.UpdatedAt = DateTime.UtcNow;
        await _entries.InsertOneAsync(entry);
        return entry;
    }

    public async Task<JournalEntry?> UpdateAsync(string userId, string id, JournalEntry updated)
    {
        updated.UpdatedAt = DateTime.UtcNow;
        var result = await _entries.FindOneAndReplaceAsync(
            e => e.Id == id && e.UserId == userId,
            updated,
            new FindOneAndReplaceOptions<JournalEntry> { ReturnDocument = ReturnDocument.After });
        return result;
    }

    public async Task<bool> DeleteAsync(string userId, string id)
    {
        var result = await _entries.DeleteOneAsync(e => e.Id == id && e.UserId == userId);
        return result.DeletedCount > 0;
    }

    public async Task<bool> UpdateMlSentimentAsync(string userId, string id, string sentiment)
    {
        var update = Builders<JournalEntry>.Update
            .Set(e => e.MlSentiment, sentiment)
            .Set(e => e.MlAssessedAt, DateTime.UtcNow)
            .Set(e => e.UpdatedAt, DateTime.UtcNow);

        var result = await _entries.UpdateOneAsync(e => e.Id == id && e.UserId == userId, update);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> ClearMlSentimentAsync(string userId, string id)
    {
        var update = Builders<JournalEntry>.Update
            .Unset(e => e.MlSentiment)
            .Unset(e => e.MlAssessedAt)
            .Set(e => e.UpdatedAt, DateTime.UtcNow);

        var result = await _entries.UpdateOneAsync(e => e.Id == id && e.UserId == userId, update);
        return result.ModifiedCount > 0;
    }
}
