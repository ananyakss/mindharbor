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
        var riskLevel = root.GetProperty("risk_level").GetString()
            ?? throw new InvalidOperationException("ML service returned no risk_level.");

        string? message = null;
        if (root.TryGetProperty("message", out var messageElement))
        {
            message = messageElement.GetString();
        }

        return new BehaviorPredictionResult(riskLevel, message);
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

    public record BehaviorPredictionRequest(
        int Sleep,
        int Steps,
        int Diet,
        int Social,
        int Hobbies,
        int Stress);

    public record BehaviorPredictionResult(string RiskLevel, string? Message);

    public record TextPredictionResult(string Sentiment);
}
