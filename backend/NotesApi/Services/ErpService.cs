using MongoDB.Driver;
using NotesApi.Models;

namespace NotesApi.Services;

public class ErpService
{
    private const string CollectionName = "erpEntries";
    private readonly IMongoCollection<ErpEntry> _erpEntries;

    public ErpService(MongoDbService mongoDb)
    {
        _erpEntries = mongoDb.GetCollection<ErpEntry>(CollectionName);
    }

    public async Task<List<ErpEntry>> GetAllForUserAsync(string userId) =>
        await _erpEntries.Find(entry => entry.UserId == userId)
            .SortByDescending(entry => entry.Date)
            .ToListAsync();

    public async Task<ErpEntry?> GetByIdAsync(string userId, string id) =>
        await _erpEntries.Find(entry => entry.Id == id && entry.UserId == userId).FirstOrDefaultAsync();

    public async Task<ErpEntry> CreateAsync(ErpEntry entry)
    {
        entry.CreatedAt = DateTime.UtcNow;
        entry.UpdatedAt = DateTime.UtcNow;
        await _erpEntries.InsertOneAsync(entry);
        return entry;
    }

    public async Task<ErpEntry?> UpdateAsync(string userId, string id, ErpEntry updated)
    {
        updated.UpdatedAt = DateTime.UtcNow;
        var result = await _erpEntries.FindOneAndReplaceAsync(
            entry => entry.Id == id && entry.UserId == userId,
            updated,
            new FindOneAndReplaceOptions<ErpEntry> { ReturnDocument = ReturnDocument.After });
        return result;
    }

    public async Task<bool> DeleteAsync(string userId, string id)
    {
        var result = await _erpEntries.DeleteOneAsync(entry => entry.Id == id && entry.UserId == userId);
        return result.DeletedCount > 0;
    }
}

