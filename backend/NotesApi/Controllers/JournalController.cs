using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotesApi.Models;
using NotesApi.Services;

namespace NotesApi.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class JournalController : ControllerBase
{
    private const int MaxEntryLength = 20_000;

    private readonly JournalEntryService _journalService;

    public JournalController(JournalEntryService journalService)
    {
        _journalService = journalService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        return Ok(await _journalService.GetAllForUserAsync(userId));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var entry = await _journalService.GetByIdAsync(userId, id);
        return entry is null ? NotFound() : Ok(entry);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] JournalEntryRequest request)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        if (string.IsNullOrWhiteSpace(request.EntryText) || request.EntryText.Length > MaxEntryLength)
        {
            return BadRequest(new { message = $"entryText is required and must be at most {MaxEntryLength} characters." });
        }

        var entry = new JournalEntry
        {
            UserId = userId,
            EntryText = request.EntryText.Trim(),
            AllowAnalysisConsent = request.AllowAnalysisConsent
        };

        await _journalService.CreateAsync(entry);
        return CreatedAtAction(nameof(GetById), new { id = entry.Id }, entry);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] JournalEntryRequest request)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        if (string.IsNullOrWhiteSpace(request.EntryText) || request.EntryText.Length > MaxEntryLength)
        {
            return BadRequest(new { message = $"entryText is required and must be at most {MaxEntryLength} characters." });
        }

        var existing = await _journalService.GetByIdAsync(userId, id);
        if (existing is null) return NotFound();

        existing.EntryText = request.EntryText.Trim();
        existing.AllowAnalysisConsent = request.AllowAnalysisConsent;

        var updated = await _journalService.UpdateAsync(userId, id, existing);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var deleted = await _journalService.DeleteAsync(userId, id);
        return deleted ? NoContent() : NotFound();
    }

    private string? GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

    public record JournalEntryRequest(string EntryText, bool AllowAnalysisConsent);
}
