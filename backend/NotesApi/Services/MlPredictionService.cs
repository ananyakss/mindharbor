using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using NotesApi.Options;

namespace NotesApi.Services;

public class MlPredictionService
{
    private readonly HttpClient _httpClient;
    private readonly MlOptions _options;

    public MlPredictionService(HttpClient httpClient, IOptions<MlOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<BehaviorPredictionResult> PredictBehaviorAsync(
        BehaviorPredictionRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync(
            $"{_options.BaseUrl.TrimEnd('/')}/predict",
            new
            {
                sleep = request.Sleep,
                steps = request.Steps,
                diet = request.Diet,
                social = request.Social,
                hobbies = request.Hobbies,
                stress = request.Stress
            },
            cancellationToken);

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(ReadMlError(body, response.StatusCode));
        }

        using var document = JsonDocument.Parse(body);
        var root = document.RootElement;
        var modelRiskLevel = root.GetProperty("risk_level").GetString()
            ?? throw new InvalidOperationException("ML service returned no risk_level.");

        var applicationRiskLevel = MapModelRiskToApplicationRisk(modelRiskLevel);
        var message = BuildApplicationRiskMessage(applicationRiskLevel);

        return new BehaviorPredictionResult(modelRiskLevel, applicationRiskLevel, message);
    }

    public async Task<TextPredictionResult> PredictTextAsync(
        string journalText,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync(
            $"{_options.BaseUrl.TrimEnd('/')}/predict-text",
            new { journalText },
            cancellationToken);

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(ReadMlError(body, response.StatusCode));
        }

        using var document = JsonDocument.Parse(body);
        var root = document.RootElement;
        var sentiment = root.GetProperty("sentiment").GetString()
            ?? throw new InvalidOperationException("ML service returned no sentiment.");

        return new TextPredictionResult(sentiment);
    }

    private static string ReadMlError(string body, System.Net.HttpStatusCode statusCode)
    {
        try
        {
            using var document = JsonDocument.Parse(body);
            if (document.RootElement.TryGetProperty("error", out var errorElement))
            {
                var message = errorElement.GetString();
                if (!string.IsNullOrWhiteSpace(message))
                {
                    return message;
                }
            }
        }
        catch (JsonException)
        {
        }

        return $"ML service request failed with status {(int)statusCode}.";
    }

    // Model labels reflect work-life balance score (high = better balance, low = worse balance).
    // The application presents inverted risk meaning to users (low risk = good, high risk = bad).
    private static string MapModelRiskToApplicationRisk(string modelRiskLevel) =>
        modelRiskLevel.Trim().ToLowerInvariant() switch
        {
            "high" => "low",
            "low" => "high",
            _ => modelRiskLevel
        };

    private static string BuildApplicationRiskMessage(string applicationRiskLevel) =>
        applicationRiskLevel.Trim().ToLowerInvariant() switch
        {
            "low" => "You seem to be doing well today.",
            "moderate" => "Some imbalance detected.",
            _ => "Your behavior indicates higher risk."
        };

    public record BehaviorPredictionRequest(
        int Sleep,
        int Steps,
        int Diet,
        int Social,
        int Hobbies,
        int Stress);

    public record BehaviorPredictionResult(string ModelRiskLevel, string RiskLevel, string? Message);

    public record TextPredictionResult(string Sentiment);
}
