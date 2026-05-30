using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotesApi.Models;
using NotesApi.Services;

namespace NotesApi.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ErpController : ControllerBase
{
    private readonly ErpService _erpService;

    public ErpController(ErpService erpService)
    {
        _erpService = erpService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var entries = await _erpService.GetAllForUserAsync(userId);
        return Ok(entries);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var entry = await _erpService.GetByIdAsync(userId, id);
        if (entry is null) return NotFound();

        return Ok(entry);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ErpEntryRequest request)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        if (request.AnxietyLevel is < 1 or > 10)
        {
            return BadRequest(new { message = "anxietyLevel must be between 1 and 10." });
        }

        var entry = new ErpEntry
        {
            UserId = userId,
            Date = request.Date.ToUniversalTime(),
            Trigger = request.Trigger.Trim(),
            AnxietyLevel = request.AnxietyLevel,
            ExposureActivity = request.ExposureActivity.Trim(),
            ResistedCompulsion = request.ResistedCompulsion,
            Reflection = request.Reflection.Trim()
        };

        await _erpService.CreateAsync(entry);
        return CreatedAtAction(nameof(GetById), new { id = entry.Id }, entry);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] ErpEntryRequest request)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        if (request.AnxietyLevel is < 1 or > 10)
        {
            return BadRequest(new { message = "anxietyLevel must be between 1 and 10." });
        }

        var existing = await _erpService.GetByIdAsync(userId, id);
        if (existing is null) return NotFound();

        existing.Date = request.Date.ToUniversalTime();
        existing.Trigger = request.Trigger.Trim();
        existing.AnxietyLevel = request.AnxietyLevel;
        existing.ExposureActivity = request.ExposureActivity.Trim();
        existing.ResistedCompulsion = request.ResistedCompulsion;
        existing.Reflection = request.Reflection.Trim();

        var updated = await _erpService.UpdateAsync(userId, id, existing);
        if (updated is null) return NotFound();

        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var deleted = await _erpService.DeleteAsync(userId, id);
        return deleted ? NoContent() : NotFound();
    }

    private string? GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

    public record ErpEntryRequest(
        DateTime Date,
        string Trigger,
        int AnxietyLevel,
        string ExposureActivity,
        bool ResistedCompulsion,
        string Reflection);
}
