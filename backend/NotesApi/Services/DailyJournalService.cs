using MongoDB.Driver;
using NotesApi.Models;

namespace NotesApi.Services;

public class DailyJournalService
{
    private const string CollectionName = "dailyJournal";
    private readonly IMongoCollection<DailyJournalEntry> _dailyJournalEntries;

    public DailyJournalService(MongoDbService mongoDb)
    {
        _dailyJournalEntries = mongoDb.GetCollection<DailyJournalEntry>(CollectionName);
    }

    public async Task<List<DailyJournalEntry>> GetAllForUserAsync(string userId) =>
        await _dailyJournalEntries.Find(entry => entry.UserId == userId)
            .SortByDescending(entry => entry.Date)
            .ToListAsync();

    public async Task<DailyJournalEntry?> GetByIdAsync(string userId, string id) =>
        await _dailyJournalEntries.Find(entry => entry.Id == id && entry.UserId == userId).FirstOrDefaultAsync();

    public async Task<DailyJournalEntry> CreateAsync(DailyJournalEntry entry)
    {
        entry.CreatedAt = DateTime.UtcNow;
        entry.UpdatedAt = DateTime.UtcNow;
        await _dailyJournalEntries.InsertOneAsync(entry);
        return entry;
    }

    public async Task<DailyJournalEntry?> UpdateAsync(string userId, string id, DailyJournalEntry updated)
    {
        updated.UpdatedAt = DateTime.UtcNow;
        var result = await _dailyJournalEntries.FindOneAndReplaceAsync(
            entry => entry.Id == id && entry.UserId == userId,
            updated,
            new FindOneAndReplaceOptions<DailyJournalEntry> { ReturnDocument = ReturnDocument.After });
        return result;
    }

    public async Task<bool> DeleteAsync(string userId, string id)
    {
        var result = await _dailyJournalEntries.DeleteOneAsync(entry => entry.Id == id && entry.UserId == userId);
        return result.DeletedCount > 0;
    }
}

