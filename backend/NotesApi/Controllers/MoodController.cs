using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotesApi.Models;
using NotesApi.Services;

namespace NotesApi.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class MoodController : ControllerBase
{
    private readonly MoodService _moodService;

    public MoodController(MoodService moodService)
    {
        _moodService = moodService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        return Ok(await _moodService.GetAllForUserAsync(userId));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var entry = await _moodService.GetByIdAsync(userId, id);
        return entry is null ? NotFound() : Ok(entry);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] MoodEntryRequest request)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        if (request.MoodIntensity is < 1 or > 10)
        {
            return BadRequest(new { message = "moodIntensity must be between 1 and 10." });
        }

        var entry = new MoodEntry
        {
            UserId = userId,
            RecordedAt = request.RecordedAt.ToUniversalTime(),
            Mood = request.Mood,
            MoodIntensity = request.MoodIntensity
        };

        await _moodService.CreateAsync(entry);
        return CreatedAtAction(nameof(GetById), new { id = entry.Id }, entry);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] MoodEntryRequest request)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        if (request.MoodIntensity is < 1 or > 10)
        {
            return BadRequest(new { message = "moodIntensity must be between 1 and 10." });
        }

        var existing = await _moodService.GetByIdAsync(userId, id);
        if (existing is null) return NotFound();

        existing.RecordedAt = request.RecordedAt.ToUniversalTime();
        existing.Mood = request.Mood;
        existing.MoodIntensity = request.MoodIntensity;

        var updated = await _moodService.UpdateAsync(userId, id, existing);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var deleted = await _moodService.DeleteAsync(userId, id);
        return deleted ? NoContent() : NotFound();
    }

    private string? GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

    public record MoodEntryRequest(DateTime RecordedAt, MoodType Mood, int MoodIntensity);
}
