using MongoDB.Driver;
using NotesApi.Models;

namespace NotesApi.Services;

public class CbtService
{
    private const string CollectionName = "cbtEntries";
    private readonly IMongoCollection<CbtEntry> _cbtEntries;

    public CbtService(MongoDbService mongoDb)
    {
        _cbtEntries = mongoDb.GetCollection<CbtEntry>(CollectionName);
    }

    public async Task<List<CbtEntry>> GetAllForUserAsync(string userId) =>
        await _cbtEntries.Find(entry => entry.UserId == userId)
            .SortByDescending(entry => entry.Date)
            .ToListAsync();

    public async Task<CbtEntry?> GetByIdAsync(string userId, string id) =>
        await _cbtEntries.Find(entry => entry.Id == id && entry.UserId == userId).FirstOrDefaultAsync();

    public async Task<CbtEntry> CreateAsync(CbtEntry entry)
    {
        entry.CreatedAt = DateTime.UtcNow;
        entry.UpdatedAt = DateTime.UtcNow;
        await _cbtEntries.InsertOneAsync(entry);
        return entry;
    }

    public async Task<CbtEntry?> UpdateAsync(string userId, string id, CbtEntry updated)
    {
        updated.UpdatedAt = DateTime.UtcNow;
        var result = await _cbtEntries.FindOneAndReplaceAsync(
            entry => entry.Id == id && entry.UserId == userId,
            updated,
            new FindOneAndReplaceOptions<CbtEntry> { ReturnDocument = ReturnDocument.After });
        return result;
    }

    public async Task<bool> DeleteAsync(string userId, string id)
    {
        var result = await _cbtEntries.DeleteOneAsync(entry => entry.Id == id && entry.UserId == userId);
        return result.DeletedCount > 0;
    }
}

