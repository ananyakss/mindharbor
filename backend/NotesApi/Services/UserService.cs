using MongoDB.Driver;
using NotesApi.Models;

namespace NotesApi.Services;

public class UserService
{
    private const string UsersCollectionName = "users";
    private const string RefreshTokensCollectionName = "refreshTokens";

    private readonly IMongoCollection<User> _users;
    private readonly IMongoCollection<RefreshToken> _refreshTokens;

    public UserService(MongoDbService mongoDb)
    {
        _users = mongoDb.GetCollection<User>(UsersCollectionName);
        _refreshTokens = mongoDb.GetCollection<RefreshToken>(RefreshTokensCollectionName);
    }

    public async Task<User?> GetByEmailAsync(string email) =>
        await _users.Find(u => u.Email.ToLower() == email.ToLower()).FirstOrDefaultAsync();

    public async Task<User?> GetByUsernameAsync(string username) =>
        await _users.Find(u => u.Username.ToLower() == username.ToLower()).FirstOrDefaultAsync();

    public async Task<User?> GetByIdAsync(string userId) =>
        await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();

    public async Task<User> CreateAsync(User user)
    {
        user.CreatedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        await _users.InsertOneAsync(user);
        return user;
    }

    public async Task UpdateAsync(User user)
    {
        user.UpdatedAt = DateTime.UtcNow;
        await _users.ReplaceOneAsync(u => u.Id == user.Id, user);
    }

    public async Task RemoveRefreshTokensForUserAsync(string userId) =>
        await _refreshTokens.DeleteManyAsync(rt => rt.UserId == userId);

    public async Task<RefreshToken> SaveRefreshTokenAsync(RefreshToken refreshToken)
    {
        await _refreshTokens.InsertOneAsync(refreshToken);
        return refreshToken;
    }

    public async Task<RefreshToken?> GetRefreshTokenAsync(string token) =>
        await _refreshTokens.Find(rt => rt.Token == token).FirstOrDefaultAsync();

    public async Task UpdateRefreshTokenAsync(RefreshToken refreshToken) =>
        await _refreshTokens.ReplaceOneAsync(rt => rt.Id == refreshToken.Id, refreshToken);

    public async Task DeleteRefreshTokenAsync(string token) =>
        await _refreshTokens.DeleteOneAsync(rt => rt.Token == token);

    /// <summary>Updates streak and points on a successful login (once per calendar UTC day).</summary>
    public async Task<User?> RecordLoginAsync(string userId)
    {
        var user = await GetByIdAsync(userId);
        if (user is null)
        {
            return null;
        }

        var today = DateTime.UtcNow.Date;
        var lastDate = user.LastLoginUtc?.Date;

        if (lastDate != today)
        {
            if (lastDate is null)
            {
                user.LoginStreak = 1;
                user.GamificationPoints += 10;
            }
            else if (lastDate == today.AddDays(-1))
            {
                user.LoginStreak += 1;
                user.GamificationPoints += 10 + Math.Min(user.LoginStreak, 30);
            }
            else
            {
                user.LoginStreak = 1;
                user.GamificationPoints += 10;
            }

            user.LastLoginUtc = DateTime.UtcNow;
            await UpdateAsync(user);
        }

        return user;
    }
}

