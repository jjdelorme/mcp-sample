using Anthropic.SDK;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol.Client;

var clientTransport = new StdioClientTransport(new StdioClientTransportOptions
{
    Name = "WeatherService",
    Command = "/home/jasondel/dev/mcp/.venv/bin/python",
    Arguments = ["/home/jasondel/dev/mcp/server/python/weather.py"],
});

var client = await McpClientFactory.CreateAsync(clientTransport);

// Print the list of tools available from the server.
foreach (var tool in await client.ListToolsAsync())
{
    Console.WriteLine($"{tool.Name} ({tool.Description})");
}

// Execute a tool (this would normally be driven by LLM tool invocations).
var result = await client.CallToolAsync(
    "get_forecast",
    new Dictionary<string, object?>() { ["latitude"] = 47.6062, ["longitude"] = -122.3321 },
    cancellationToken:CancellationToken.None);

// The weather tool returns a single text content object with the forecast.
Console.WriteLine(result.Content.First(c => c.Type == "text").Text);