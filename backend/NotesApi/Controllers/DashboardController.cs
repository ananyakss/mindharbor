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
public class DashboardController : ControllerBase
{
    private readonly MoodService _moodService;
    private readonly BehaviorEntryService _behaviorService;
    private readonly JournalEntryService _journalService;
    private readonly UserService _userService;

    public DashboardController(
        MoodService moodService,
        BehaviorEntryService behaviorService,
        JournalEntryService journalService,
        UserService userService)
    {
        _moodService = moodService;
        _behaviorService = behaviorService;
        _journalService = journalService;
        _userService = userService;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var user = await _userService.GetByIdAsync(userId);
        if (user is null) return Unauthorized();

        var weekStart = DateTime.UtcNow.Date.AddDays(-6);
        var moods = await _moodService.GetForUserSinceAsync(userId, weekStart);
        var behaviors = await _behaviorService.GetForUserSinceAsync(userId, weekStart);
        var journals = await _journalService.GetRecentForUserAsync(userId, 5);
        var weeklyJournals = await _journalService.GetForUserSinceAsync(userId, weekStart);

        var moodByDay = new Dictionary<string, MoodEntry>();
        foreach (var m in moods.OrderByDescending(x => x.RecordedAt))
        {
            var key = m.RecordedAt.ToUniversalTime().Date.ToString("yyyy-MM-dd");
            if (!moodByDay.ContainsKey(key))
            {
                moodByDay[key] = m;
            }
        }

        var trend = new List<MoodTrendPointDto>();
        for (var i = 6; i >= 0; i--)
        {
            var d = DateTime.UtcNow.Date.AddDays(-i);
            var key = d.ToString("yyyy-MM-dd");
            moodByDay.TryGetValue(key, out var entry);
            trend.Add(new MoodTrendPointDto(
                key,
                entry?.Mood.ToString(),
                entry?.MoodIntensity));
        }

        var latestBehavior = behaviors.Count > 0 ? behaviors[0] : null;
        BehaviorSummaryDto? behaviorSummary = null;
        if (latestBehavior is not null)
        {
            behaviorSummary = new BehaviorSummaryDto(
                latestBehavior.Sleep,
                latestBehavior.Water,
                latestBehavior.Meals,
                latestBehavior.ExerciseHours,
                latestBehavior.Steps,
                latestBehavior.Diet,
                latestBehavior.Social,
                latestBehavior.Hobbies,
                latestBehavior.Stress,
                latestBehavior.Day);
        }

        var weeklyRiskSummary = BuildWeeklyRiskSummary(behaviors, weeklyJournals);

        return Ok(new DashboardSummaryDto(
            user.LoginStreak,
            user.GamificationPoints,
            trend,
            behaviorSummary,
            journals,
            weeklyRiskSummary));

    }

    private static WeeklyRiskSummaryDto BuildWeeklyRiskSummary(
        IReadOnlyList<BehaviorEntry> behaviors,
        IReadOnlyList<JournalEntry> journals)
    {
        var behavioral = BuildBehaviorWeeklyRisk(behaviors);
        var textual = BuildTextWeeklyRisk(journals);
        var overallMessage = BuildOverallMessage(behavioral, textual);

        return new WeeklyRiskSummaryDto(behavioral, textual, overallMessage);
    }

    private static WeeklyRiskBreakdownDto? BuildBehaviorWeeklyRisk(IEnumerable<BehaviorEntry> behaviors)
    {
        var assessed = behaviors
            .Where(b => !string.IsNullOrWhiteSpace(b.MlModelRiskLevel))
            .ToList();

        if (assessed.Count == 0)
        {
            return null;
        }

        var highRiskEntries = assessed.Count(b => IsBehaviorHighRisk(b.MlModelRiskLevel!));
        var lowRiskEntries = assessed.Count - highRiskEntries;

        return new WeeklyRiskBreakdownDto(assessed.Count, highRiskEntries, lowRiskEntries);
    }

    private static WeeklyRiskBreakdownDto? BuildTextWeeklyRisk(IEnumerable<JournalEntry> journals)
    {
        var assessed = journals
            .Where(j => j.AllowAnalysisConsent && !string.IsNullOrWhiteSpace(j.MlSentiment))
            .ToList();

        if (assessed.Count == 0)
        {
            return null;
        }

        var highRiskEntries = assessed.Count(j => IsTextHighRisk(j.MlSentiment!));
        var lowRiskEntries = assessed.Count - highRiskEntries;

        return new WeeklyRiskBreakdownDto(assessed.Count, highRiskEntries, lowRiskEntries);
    }

    private static bool IsBehaviorHighRisk(string modelRiskLevel) =>
        modelRiskLevel.Trim().Equals("low", StringComparison.OrdinalIgnoreCase);

    private static bool IsTextHighRisk(string sentiment) =>
        sentiment.Trim().Equals("negative", StringComparison.OrdinalIgnoreCase);

    private static string? BuildOverallMessage(
        WeeklyRiskBreakdownDto? behavioral,
        WeeklyRiskBreakdownDto? textual)
    {
        if (behavioral is null && textual is null)
        {
            return null;
        }

        var behavioralElevated = behavioral is not null && behavioral.HighRiskEntries > behavioral.LowRiskEntries;
        var textualElevated = textual is not null && textual.HighRiskEntries > textual.LowRiskEntries;

        if (behavioralElevated && textualElevated)
        {
            return "Your recent behavioral and journal entries show some elevated-risk patterns.";
        }

        if (behavioralElevated || textualElevated)
        {
            return "Your recent entries show some elevated-risk patterns this week.";
        }

        return "Your recent entries have generally been lower risk this week.";
    }

    private string? GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

    public record MoodTrendPointDto(string Date, string? Mood, int? Intensity);

    public record BehaviorSummaryDto(
        int Sleep,
        int Water,
        MealInfo Meals,
        int ExerciseHours,
        int Steps,
        int Diet,
        int Social,
        int Hobbies,
        int Stress,
        DateTime Day);

    public record DashboardSummaryDto(
        int LoginStreak,
        int GamificationPoints,
        IReadOnlyList<MoodTrendPointDto> WeeklyMoodTrend,
        BehaviorSummaryDto? BehavioralSummary,
        IReadOnlyList<JournalEntry> RecentJournalEntries,
        WeeklyRiskSummaryDto WeeklyRiskSummary);

    public record WeeklyRiskSummaryDto(
        WeeklyRiskBreakdownDto? Behavioral,
        WeeklyRiskBreakdownDto? Textual,
        string? OverallMessage);

    public record WeeklyRiskBreakdownDto(
        int TotalEntries,
        int HighRiskEntries,
        int LowRiskEntries);
}
