using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using PaintTintCalculator.Application.DTOs.DispenseJobs;
using PaintTintCalculator.Application.DTOs.Shades;
using PaintTintCalculator.Application.DTOs.Tint;
using Xunit;

namespace PaintTintCalculator.Api.Tests;

public class ApiEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public ApiEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task SearchShades_ReturnsOk_WithSeededShades()
    {
        var response = await _client.GetAsync("/api/shades?search=Ocean");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var shades = await response.Content.ReadFromJsonAsync<List<ShadeSummaryDto>>(JsonOptions);
        Assert.NotNull(shades);
        Assert.Contains(shades, s => s.Code == "OM-201");
    }

    [Fact]
    public async Task GetShadeById_ReturnsOk_ForExistingShade()
    {
        var response = await _client.GetAsync("/api/shades/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var shade = await response.Content.ReadFromJsonAsync<ShadeDetailsDto>(JsonOptions);
        Assert.NotNull(shade);
        Assert.Equal("OM-201", shade.Code);
        Assert.NotEmpty(shade.Formulas);
    }

    [Fact]
    public async Task GetShadeById_ReturnsNotFound_ForNonExistentShade()
    {
        var response = await _client.GetAsync("/api/shades/99999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetBases_ReturnsAllThreeBases()
    {
        var response = await _client.GetAsync("/api/shades/bases");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var bases = await response.Content.ReadFromJsonAsync<List<BaseDto>>(JsonOptions);
        Assert.NotNull(bases);
        Assert.Equal(3, bases.Count);
        Assert.Contains(bases, b => b.Name == "Pastel");
        Assert.Contains(bases, b => b.Name == "Medium");
        Assert.Contains(bases, b => b.Name == "Deep");
    }

    [Fact]
    public async Task CalculateTint_ReturnsOk_ForValidRequest()
    {
        // Ocean Mist (1), Pastel Base (1), 4L Can
        var request = new CalculateTintRequest(1, 1, 4.0m);
        var response = await _client.PostAsJsonAsync("/api/tint/calculate", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<TintCalculationResultDto>(JsonOptions);
        Assert.NotNull(result);
        Assert.Equal("OM-201", result.ShadeCode);
        Assert.Equal("Pastel", result.BaseName);
        Assert.Equal(4.0m, result.CanSizeLitres);
        Assert.Equal(30.00m, result.TotalColorantMl);
        Assert.Equal(1032.00m, result.TotalPrice);
    }

    [Fact]
    public async Task CalculateTint_ReturnsBadRequest_ForUnsupportedCanSize()
    {
        // 3L is not supported
        var request = new CalculateTintRequest(1, 1, 3.0m);
        var response = await _client.PostAsJsonAsync("/api/tint/calculate", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("INVALID_CAN_SIZE", content);
    }

    [Fact]
    public async Task CreateDispenseJob_PersistsJobAndReturnsCreated()
    {
        // Ocean Mist (1), Pastel Base (1), 4L Can
        var request = new CreateDispenseJobRequest(1, 1, 4.0m);
        var response = await _client.PostAsJsonAsync("/api/dispense-jobs", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var job = await response.Content.ReadFromJsonAsync<DispenseJobDto>(JsonOptions);
        Assert.NotNull(job);
        Assert.True(job.Id > 0);
        Assert.Equal(4.0m, job.CanSizeLitres);
        Assert.Equal(1032.00m, job.TotalPrice);
        Assert.NotEmpty(job.Items);
    }

    [Fact]
    public async Task GetRecentDispenseJobs_ReturnsRecentList()
    {
        // Ensure at least one dispense job exists
        var createRequest = new CreateDispenseJobRequest(1, 1, 1.0m);
        await _client.PostAsJsonAsync("/api/dispense-jobs", createRequest);

        var response = await _client.GetAsync("/api/dispense-jobs/recent");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var jobs = await response.Content.ReadFromJsonAsync<List<DispenseJobDto>>(JsonOptions);
        Assert.NotNull(jobs);
        Assert.NotEmpty(jobs);
    }
}
