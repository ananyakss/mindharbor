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
public class BehaviorController : ControllerBase
{
    private readonly BehaviorEntryService _behaviorService;

    public BehaviorController(BehaviorEntryService behaviorService)
    {
        _behaviorService = behaviorService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        return Ok(await _behaviorService.GetAllForUserAsync(userId));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var entry = await _behaviorService.GetByIdAsync(userId, id);
        return entry is null ? NotFound() : Ok(entry);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] BehaviorEntryRequest request)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var validation = Validate(request);
        if (!string.IsNullOrEmpty(validation))
        {
            return BadRequest(new { message = validation });
        }

        var entry = new BehaviorEntry
        {
            UserId = userId,
            Day = request.Day.ToUniversalTime().Date,
            Sleep = request.Sleep,
            Water = request.Water,
            Meals = new MealInfo
            {
                Breakfast = request.Meals.Breakfast,
                Lunch = request.Meals.Lunch,
                Dinner = request.Meals.Dinner
            },
            ExerciseHours = request.ExerciseHours,
            Steps = request.Steps,
            Diet = request.Diet,
            Social = request.Social,
            Hobbies = request.Hobbies,
            Stress = request.Stress
        };
        Console.WriteLine($"Saving data: {JsonSerializer.Serialize(entry)}");

        await _behaviorService.CreateAsync(entry);
        return CreatedAtAction(nameof(GetById), new { id = entry.Id }, entry);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] BehaviorEntryRequest request)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var validation = Validate(request);
        if (!string.IsNullOrEmpty(validation))
        {
            return BadRequest(new { message = validation });
        }

        var existing = await _behaviorService.GetByIdAsync(userId, id);
        if (existing is null) return NotFound();

        existing.Day = request.Day.ToUniversalTime().Date;
        existing.Sleep = request.Sleep;
        existing.Water = request.Water;
        existing.Meals = new MealInfo
        {
            Breakfast = request.Meals.Breakfast,
            Lunch = request.Meals.Lunch,
            Dinner = request.Meals.Dinner
        };
        existing.ExerciseHours = request.ExerciseHours;
        existing.Steps = request.Steps;
        existing.Diet = request.Diet;
        existing.Social = request.Social;
        existing.Hobbies = request.Hobbies;
        existing.Stress = request.Stress;
        Console.WriteLine($"Saving data: {JsonSerializer.Serialize(existing)}");

        var updated = await _behaviorService.UpdateAsync(userId, id, existing);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var deleted = await _behaviorService.DeleteAsync(userId, id);
        return deleted ? NoContent() : NotFound();
    }

    private static string? Validate(BehaviorEntryRequest request)
    {
        if (request.Meals is null)
        {
            return "meals is required.";
        }

        if (request.Sleep is < 1 or > 10)
        {
            return "sleep must be between 1 and 10.";
        }

        if (request.Water is < 0 or > 30)
        {
            return "water must be between 0 and 30.";
        }

        if (request.ExerciseHours is < 0 or > 24)
        {
            return "exerciseHours must be between 0 and 24.";
        }

        if (request.Steps is < 0 or > 10)
        {
            return "steps must be between 0 and 10.";
        }

        if (request.Diet is < 0 or > 5)
        {
            return "diet must be between 0 and 5.";
        }

        if (request.Social is < 0 or > 10)
        {
            return "social must be between 0 and 10.";
        }

        if (request.Hobbies is < 0 or > 10)
        {
            return "hobbies must be between 0 and 10.";
        }

        if (request.Stress is < 1 or > 5)
        {
            return "stress must be between 1 and 5.";
        }

        return null;
    }

    private string? GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

    public record BehaviorEntryRequest(
        DateTime Day,
        int Sleep,
        int Water,
        MealInfoRequest Meals,
        int Diet,
        int ExerciseHours,
        int Steps,
        int Social,
        int Hobbies,
        int Stress);

    public record MealInfoRequest(
        bool Breakfast,
        bool Lunch,
        bool Dinner);
}
