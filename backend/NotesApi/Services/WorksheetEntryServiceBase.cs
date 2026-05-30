using MongoDB.Driver;
using NotesApi.Models;

namespace NotesApi.Services;

public abstract class WorksheetEntryServiceBase<TDocument> where TDocument : WorksheetEntryBase
{
    protected readonly IMongoCollection<TDocument> Collection;

    protected WorksheetEntryServiceBase(MongoDbService mongoDb, string collectionName)
    {
        Collection = mongoDb.GetCollection<TDocument>(collectionName);
    }

    public async Task<List<TDocument>> GetAllForUserAsync(string userId) =>
        await Collection.Find(x => x.UserId == userId)
            .SortByDescending(x => x.CreatedAt)
            .ToListAsync();

    public async Task<TDocument?> GetByIdAsync(string userId, string id) =>
        await Collection.Find(x => x.Id == id && x.UserId == userId).FirstOrDefaultAsync();

    public async Task<TDocument> CreateAsync(TDocument entry)
    {
        entry.CreatedAt = DateTime.UtcNow;
        entry.UpdatedAt = DateTime.UtcNow;
        await Collection.InsertOneAsync(entry);
        return entry;
    }

    public async Task<TDocument?> UpdateAsync(string userId, string id, TDocument updated)
    {
        updated.UpdatedAt = DateTime.UtcNow;
        return await Collection.FindOneAndReplaceAsync(
            x => x.Id == id && x.UserId == userId,
            updated,
            new FindOneAndReplaceOptions<TDocument> { ReturnDocument = ReturnDocument.After });
    }

    public async Task<bool> DeleteAsync(string userId, string id)
    {
        var result = await Collection.DeleteOneAsync(x => x.Id == id && x.UserId == userId);
        return result.DeletedCount > 0;
    }
}
