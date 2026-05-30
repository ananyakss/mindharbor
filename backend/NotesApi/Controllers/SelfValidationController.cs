using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotesApi.Helpers;
using NotesApi.Models;
using NotesApi.Services;

namespace NotesApi.Controllers;

[ApiController]
[Authorize]
[Route("api/self-validation")]
public class SelfValidationController : ControllerBase
{
    private readonly SelfValidationEntryService _service;

    public SelfValidationController(SelfValidationEntryService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        return Ok(await _service.GetAllForUserAsync(userId));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        var entry = await _service.GetByIdAsync(userId, id);
        return entry is null ? NotFound() : Ok(entry);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SelfValidationEntry body)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        body.Id = null;
        body.UserId = userId;
        TherapyWorksheetNormalization.EnsureAndTrim(body);
        var err = TherapyWorksheetRequestValidator.ValidateSelfValidation(body);
        if (err is not null) return BadRequest(new { message = err });

        await _service.CreateAsync(body);
        return CreatedAtAction(nameof(GetById), new { id = body.Id }, body);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] SelfValidationEntry body)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var existing = await _service.GetByIdAsync(userId, id);
        if (existing is null) return NotFound();

        body.Id = existing.Id;
        body.UserId = userId;
        body.CreatedAt = existing.CreatedAt;
        TherapyWorksheetNormalization.EnsureAndTrim(body);
        var err = TherapyWorksheetRequestValidator.ValidateSelfValidation(body);
        if (err is not null) return BadRequest(new { message = err });

        var updated = await _service.UpdateAsync(userId, id, body);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        var deleted = await _service.DeleteAsync(userId, id);
        return deleted ? NoContent() : NotFound();
    }

    private string? GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);
}
