using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotesApi.Models;
using NotesApi.Services;

namespace NotesApi.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class DailyController : ControllerBase
{
    private readonly DailyJournalService _dailyJournalService;

    public DailyController(DailyJournalService dailyJournalService)
    {
        _dailyJournalService = dailyJournalService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var entries = await _dailyJournalService.GetAllForUserAsync(userId);
        return Ok(entries);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var entry = await _dailyJournalService.GetByIdAsync(userId, id);
        if (entry is null) return NotFound();

        return Ok(entry);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] DailyJournalRequest request)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var entry = new DailyJournalEntry
        {
            UserId = userId,
            Date = request.Date.ToUniversalTime(),
            EntryText = request.EntryText,
            SleepHours = request.SleepHours,
            WaterGlasses = request.WaterGlasses,
            FoodNotes = request.FoodNotes,
            Mood = request.Mood,
            MoodEmoji = request.MoodEmoji,
            Gratitude = (request.Gratitude ?? []).Take(3).ToList()
        };

        await _dailyJournalService.CreateAsync(entry);
        return CreatedAtAction(nameof(GetById), new { id = entry.Id }, entry);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] DailyJournalRequest request)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var existing = await _dailyJournalService.GetByIdAsync(userId, id);
        if (existing is null) return NotFound();

        existing.Date = request.Date.ToUniversalTime();
        existing.EntryText = request.EntryText;
        existing.SleepHours = request.SleepHours;
        existing.WaterGlasses = request.WaterGlasses;
        existing.FoodNotes = request.FoodNotes;
        existing.Mood = request.Mood;
        existing.MoodEmoji = request.MoodEmoji;
        existing.Gratitude = (request.Gratitude ?? []).Take(3).ToList();

        var updated = await _dailyJournalService.UpdateAsync(userId, id, existing);
        if (updated is null) return NotFound();

        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var deleted = await _dailyJournalService.DeleteAsync(userId, id);
        return deleted ? NoContent() : NotFound();
    }

    private string? GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

    public record DailyJournalRequest(
        DateTime Date,
        string EntryText,
        double SleepHours,
        int WaterGlasses,
        string FoodNotes,
        string Mood,
        string? MoodEmoji,
        List<string>? Gratitude);
}

