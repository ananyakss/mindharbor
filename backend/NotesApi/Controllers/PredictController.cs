using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotesApi.Services;
using static NotesApi.Services.MlPredictionService;

namespace NotesApi.Controllers;

[ApiController]
[Authorize]
[Route("api/predict")]
public class PredictController : ControllerBase
{
    private readonly MlPredictionService _mlPredictionService;
    private readonly BehaviorEntryService _behaviorService;
    private readonly JournalEntryService _journalService;

    public PredictController(
        MlPredictionService mlPredictionService,
        BehaviorEntryService behaviorService,
        JournalEntryService journalService)
    {
        _mlPredictionService = mlPredictionService;
        _behaviorService = behaviorService;
        _journalService = journalService;
    }

    [HttpPost("behavior")]
    public async Task<IActionResult> PredictBehavior(
        [FromBody] BehaviorPredictRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        if (request.Sleep is < 1 or > 10)
        {
            return BadRequest(new { message = "sleep must be between 1 and 10." });
        }

        if (request.Steps is < 0 or > 10)
        {
            return BadRequest(new { message = "steps must be between 0 and 10." });
        }

        if (request.Diet is < 0 or > 5)
        {
            return BadRequest(new { message = "diet must be between 0 and 5." });
        }

        if (request.Social is < 0 or > 10)
        {
            return BadRequest(new { message = "social must be between 0 and 10." });
        }

        if (request.Hobbies is < 0 or > 10)
        {
            return BadRequest(new { message = "hobbies must be between 0 and 10." });
        }

        if (request.Stress is < 1 or > 5)
        {
            return BadRequest(new { message = "stress must be between 1 and 5." });
        }

        try
        {
            var result = await _mlPredictionService.PredictBehaviorAsync(
                new BehaviorPredictionRequest(
                    request.Sleep,
                    request.Steps,
                    request.Diet,
                    request.Social,
                    request.Hobbies,
                    request.Stress),
                cancellationToken);

            if (!string.IsNullOrWhiteSpace(request.BehaviorEntryId))
            {
                var saved = await _behaviorService.UpdateMlAssessmentAsync(
                    userId,
                    request.BehaviorEntryId,
                    result.ModelRiskLevel);
                if (!saved)
                {
                    return NotFound(new { message = "Behavior entry not found." });
                }
            }

            return Ok(result);
        }
        catch (InvalidOperationException error)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { message = error.Message });
        }
    }

    [HttpPost("text")]
    public async Task<IActionResult> PredictText(
        [FromBody] TextPredictRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        if (string.IsNullOrWhiteSpace(request.JournalText))
        {
            return BadRequest(new { message = "journalText is required." });
        }

        try
        {
            var result = await _mlPredictionService.PredictTextAsync(
                request.JournalText.Trim(),
                cancellationToken);

            if (!string.IsNullOrWhiteSpace(request.JournalEntryId))
            {
                var entry = await _journalService.GetByIdAsync(userId, request.JournalEntryId);
                if (entry is null)
                {
                    return NotFound(new { message = "Journal entry not found." });
                }

                if (!entry.AllowAnalysisConsent)
                {
                    return BadRequest(new { message = "Analysis consent is required for this journal entry." });
                }

                var saved = await _journalService.UpdateMlSentimentAsync(
                    userId,
                    request.JournalEntryId,
                    result.Sentiment);
                if (!saved)
                {
                    return NotFound(new { message = "Journal entry not found." });
                }
            }

            return Ok(result);
        }
        catch (InvalidOperationException error)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { message = error.Message });
        }
    }

    public record BehaviorPredictRequest(
        int Sleep,
        int Steps,
        int Diet,
        int Social,
        int Hobbies,
        int Stress,
        string? BehaviorEntryId = null);

    public record TextPredictRequest(string JournalText, string? JournalEntryId = null);

    private string? GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);
}
