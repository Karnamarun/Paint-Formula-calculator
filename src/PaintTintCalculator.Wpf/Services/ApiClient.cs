using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using PaintTintCalculator.Wpf.Models;

namespace PaintTintCalculator.Wpf.Services;

public class ApiClient : IApiClient
{
    private readonly HttpClient _httpClient;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public string BaseAddress => _httpClient.BaseAddress?.ToString() ?? "http://localhost:5000";

    public ApiClient(HttpClient? httpClient = null, string baseUrl = "http://localhost:5000")
    {
        _httpClient = httpClient ?? new HttpClient { BaseAddress = new Uri(baseUrl) };
        _httpClient.Timeout = TimeSpan.FromSeconds(10);
    }

    public async Task<bool> CheckConnectionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/shades/bases", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<IReadOnlyList<ShadeModel>> GetShadesAsync(string? search = null, CancellationToken cancellationToken = default)
    {
        var url = string.IsNullOrWhiteSpace(search)
            ? "/api/shades"
            : $"/api/shades?search={Uri.EscapeDataString(search.Trim())}";

        var response = await _httpClient.GetAsync(url, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return Array.Empty<ShadeModel>();
        }

        var result = await response.Content.ReadFromJsonAsync<List<ShadeModel>>(JsonOptions, cancellationToken);
        return result ?? new List<ShadeModel>();
    }

    public async Task<IReadOnlyList<BaseModel>> GetBasesAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync("/api/shades/bases", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return Array.Empty<BaseModel>();
        }

        var result = await response.Content.ReadFromJsonAsync<List<BaseModel>>(JsonOptions, cancellationToken);
        return result ?? new List<BaseModel>();
    }

    public async Task<CalculationResponseModel> CalculateTintAsync(int shadeId, int baseId, decimal canSizeLitres, CancellationToken cancellationToken = default)
    {
        var payload = new { shadeId, baseId, canSizeLitres };
        var response = await _httpClient.PostAsJsonAsync("/api/tint/calculate", payload, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var data = await response.Content.ReadFromJsonAsync<CalculationResponseModel>(JsonOptions, cancellationToken);
            if (data != null)
            {
                data.IsSuccess = true;
                return data;
            }
        }

        // Parse friendly error response
        var error = await ParseErrorAsync(response, cancellationToken);
        return new CalculationResponseModel
        {
            IsSuccess = false,
            ErrorMessage = error.Message,
            ErrorCode = error.Code
        };
    }

    public async Task<DispenseResponseModel> CreateDispenseJobAsync(int shadeId, int baseId, decimal canSizeLitres, CancellationToken cancellationToken = default)
    {
        var payload = new { shadeId, baseId, canSizeLitres };
        var response = await _httpClient.PostAsJsonAsync("/api/dispense-jobs", payload, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), default, cancellationToken);
            var root = doc.RootElement;
            var jobId = root.TryGetProperty("id", out var idProp) ? idProp.GetInt32() : 0;
            var totalPrice = root.TryGetProperty("totalPrice", out var priceProp) ? priceProp.GetDecimal() : 0m;
            var createdAt = root.TryGetProperty("createdAt", out var createdProp) ? createdProp.GetDateTimeOffset() : DateTimeOffset.UtcNow;

            return new DispenseResponseModel
            {
                IsSuccess = true,
                JobId = jobId,
                TotalPrice = totalPrice,
                CreatedAt = createdAt
            };
        }

        var error = await ParseErrorAsync(response, cancellationToken);
        return new DispenseResponseModel
        {
            IsSuccess = false,
            ErrorMessage = error.Message,
            ErrorCode = error.Code
        };
    }

    private static async Task<ApiErrorResponse> ParseErrorAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(JsonOptions, cancellationToken);
            if (error != null && !string.IsNullOrWhiteSpace(error.Message))
            {
                return error;
            }
        }
        catch
        {
            // fallback
        }

        return new ApiErrorResponse
        {
            Code = $"HTTP_{(int)response.StatusCode}",
            Message = $"Server returned {(int)response.StatusCode} ({response.ReasonPhrase})."
        };
    }
}
