using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Net.Http.Json;
using System.Text.Json;

namespace QuickstartWeatherServer.Tools;

[McpServerToolType]
public static class WeatherTools
{
    [McpServerTool, Description("Get weather alerts for a US state.")]
    public static async Task<string> GetAlerts(
        HttpClient client,
        [Description("The US state to get alerts for.")] string state)
    {
        var jsonElement = await client.GetFromJsonAsync<JsonElement>($"/alerts/active/area/{state}");
        var alerts = jsonElement.GetProperty("features").EnumerateArray();

        if (!alerts.Any())
        {
            return "No active alerts for this state.";
        }

        return string.Join("\n--\n", alerts.Select(alert =>
        {
            JsonElement properties = alert.GetProperty("properties");
            return $"""
                    Event: {properties.GetProperty("event").GetString()}
                    Area: {properties.GetProperty("areaDesc").GetString()}
                    Severity: {properties.GetProperty("severity").GetString()}
                    Description: {properties.GetProperty("description").GetString()}
                    Instruction: {properties.GetProperty("instruction").GetString()}
                    """;
        }));
    }

    [McpServerTool, Description("Get weather forecast for a location.")]
    public static async Task<string> GetForecast(
        HttpClient client,
        [Description("Latitude of the location.")] double latitude,
        [Description("Longitude of the location.")] double longitude)
    {       
        var locationJson = await client.GetFromJsonAsync<JsonElement>($"/points/{latitude},{longitude}");
        var forecastUrl = GetForecastUrl(locationJson);
        
        var forecastJson = await client.GetFromJsonAsync<JsonElement>(forecastUrl);
        var periods = GetPeriods(forecastJson).EnumerateArray();
        
        return string.Join("\n---\n", periods.Select(period => $"""
                {period.GetProperty("name").GetString()}
                Temperature: {period.GetProperty("temperature").GetInt32()}°F
                Wind: {period.GetProperty("windSpeed").GetString()} {period.GetProperty("windDirection").GetString()}
                Forecast: {period.GetProperty("detailedForecast").GetString()}
                """));
    }

    private static string GetForecastUrl(JsonElement root) 
    {
        if (!root.GetProperty("properties").TryGetProperty("forecast", out JsonElement property))
        {
            throw new ApplicationException($"Couldn't read forecast url from response {root}");
        }

        return property.GetString()!;
    }

    private static JsonElement GetPeriods(JsonElement root)
    {
        if (!root.GetProperty("properties").TryGetProperty("periods", out JsonElement property))
        {
            throw new ApplicationException($"Couldn't read periods from response {root}");
        }

        return property;
    }
}