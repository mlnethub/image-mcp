using System.Net.Http.Headers;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Options;

var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
{
    ContentRootPath = AppContext.BaseDirectory
});

// Disable console logging to prevent interference with MCP stdio protocol
// But keep logging to stderr for debugging
builder.Logging.ClearProviders();
builder.Logging.AddConsole(options =>
{
    options.LogToStandardErrorThreshold = LogLevel.Trace;
});

// Configure ImageApiOptions with validation
builder.Services.AddOptions<ImageApiOptions>()
    .Bind(builder.Configuration.GetSection("ImageApi"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

builder.Services.AddSingleton<HttpClient>(sp =>
{
    var options = sp.GetRequiredService<IOptions<ImageApiOptions>>().Value;
    var client = new HttpClient() { BaseAddress = new Uri(options.BaseUrl) };
    client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("image-search", "1.0"));
    return client;
});

var app = builder.Build();

await app.RunAsync();
