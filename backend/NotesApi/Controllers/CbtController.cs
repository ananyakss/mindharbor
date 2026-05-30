using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotesApi.Models;
using NotesApi.Services;

namespace NotesApi.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class CbtController : ControllerBase
{
    private static readonly HashSet<string> AllowedDistortions = new(StringComparer.OrdinalIgnoreCase)
    {
        "All-or-nothing thinking",
        "Overgeneralization",
        "Catastrophizing",
        "Mind reading",
        "Emotional reasoning"
    };

    private readonly CbtService _cbtService;

    public CbtController(CbtService cbtService)
    {
        _cbtService = cbtService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var entries = await _cbtService.GetAllForUserAsync(userId);
        return Ok(entries);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var entry = await _cbtService.GetByIdAsync(userId, id);
        if (entry is null) return NotFound();

        return Ok(entry);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CbtEntryRequest request)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var validation = ValidateRequest(request, out var distortions, out var challenge, out var reframe, out var action);
        if (validation is not null)
        {
            return BadRequest(new { message = validation });
        }

        var entry = new CbtEntry
        {
            UserId = userId,
            Date = request.Date.ToUniversalTime(),
            Situation = request.Situation.Trim(),
            AutomaticThought = request.AutomaticThought.Trim(),
            Emotions = request.Emotions?.Select(e => new Emotion { Name = e.Name.Trim(), Intensity = e.Intensity }).ToList() ?? [],
            CognitiveDistortions = distortions,
            ChallengeThoughts = challenge,
            ReframeThoughts = reframe,
            ActionPlan = action,
            EvidenceFor = request.EvidenceFor?.Trim() ?? string.Empty,
            EvidenceAgainst = request.EvidenceAgainst?.Trim() ?? string.Empty,
            RationalResponse = request.RationalResponse?.Trim() ?? string.Empty
        };

        await _cbtService.CreateAsync(entry);
        return CreatedAtAction(nameof(GetById), new { id = entry.Id }, entry);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] CbtEntryRequest request)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var validation = ValidateRequest(request, out var distortions, out var challenge, out var reframe, out var action);
        if (validation is not null)
        {
            return BadRequest(new { message = validation });
        }

        var existing = await _cbtService.GetByIdAsync(userId, id);
        if (existing is null) return NotFound();

        existing.Date = request.Date.ToUniversalTime();
        existing.Situation = request.Situation.Trim();
        existing.AutomaticThought = request.AutomaticThought.Trim();
        existing.Emotions = request.Emotions?.Select(e => new Emotion { Name = e.Name.Trim(), Intensity = e.Intensity }).ToList() ?? [];
        existing.CognitiveDistortions = distortions;
        existing.ChallengeThoughts = challenge;
        existing.ReframeThoughts = reframe;
        existing.ActionPlan = action;
        existing.EvidenceFor = request.EvidenceFor?.Trim() ?? existing.EvidenceFor;
        existing.EvidenceAgainst = request.EvidenceAgainst?.Trim() ?? existing.EvidenceAgainst;
        existing.RationalResponse = request.RationalResponse?.Trim() ?? existing.RationalResponse;

        var updated = await _cbtService.UpdateAsync(userId, id, existing);
        if (updated is null) return NotFound();

        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var deleted = await _cbtService.DeleteAsync(userId, id);
        return deleted ? NoContent() : NotFound();
    }

    private static string? ValidateRequest(
        CbtEntryRequest request,
        out List<string> distortions,
        out string challenge,
        out string reframe,
        out string action)
    {
        distortions = [];
        challenge = string.Empty;
        reframe = string.Empty;
        action = string.Empty;

        if (string.IsNullOrWhiteSpace(request.Situation) || string.IsNullOrWhiteSpace(request.AutomaticThought))
        {
            return "Situation and automatic thoughts are required.";
        }

        if (request.Emotions is null || request.Emotions.Count == 0)
        {
            return "At least one emotion is required.";
        }

        foreach (var e in request.Emotions)
        {
            if (string.IsNullOrWhiteSpace(e.Name) || e.Intensity is < 1 or > 10)
            {
                return "Each emotion needs a name and intensity between 1 and 10.";
            }
        }

        var raw = request.CognitiveDistortions ?? [];
        foreach (var d in raw)
        {
            if (string.IsNullOrWhiteSpace(d))
            {
                continue;
            }

            if (!AllowedDistortions.Contains(d.Trim()))
            {
                return $"Cognitive distortion not allowed: {d}";
            }

            var canonical = AllowedDistortions.First(x => x.Equals(d.Trim(), StringComparison.OrdinalIgnoreCase));
            if (!distortions.Contains(canonical, StringComparer.OrdinalIgnoreCase))
            {
                distortions.Add(canonical);
            }
        }

        challenge = FirstNonEmpty(request.ChallengeThoughts, request.EvidenceFor);
        reframe = FirstNonEmpty(request.ReframeThoughts, request.RationalResponse);
        action = (request.ActionPlan ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(challenge))
        {
            return "Challenge thoughts (or legacy evidence for) is required.";
        }

        if (string.IsNullOrWhiteSpace(reframe))
        {
            return "Reframe thoughts (or legacy rational response) is required.";
        }

        if (string.IsNullOrWhiteSpace(action))
        {
            return "Action plan is required.";
        }

        return null;
    }

    private static string FirstNonEmpty(string? primary, string? fallback) =>
        !string.IsNullOrWhiteSpace(primary) ? primary!.Trim() : (fallback ?? string.Empty).Trim();

    private string? GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

    public record EmotionRequest(string Name, int Intensity);

    public record CbtEntryRequest(
        DateTime Date,
        string Situation,
        string AutomaticThought,
        List<EmotionRequest>? Emotions,
        List<string>? CognitiveDistortions,
        string? ChallengeThoughts,
        string? ReframeThoughts,
        string? ActionPlan,
        string? EvidenceFor,
        string? EvidenceAgainst,
        string? RationalResponse);
}
