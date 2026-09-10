using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Ntech.JsonL.Models;

namespace Ntech.JsonL.Demos;

/// <summary>
/// Demo ASP.NET Core Endpoint /people-stream va HttpClient Stream Reader
/// </summary>
public static class AspNetCoreEndpointDemo
{
    private const string ServerUrl = "http://localhost:5055";

    public static async Task RunAsync()
    {
        Console.WriteLine("--- DEMO 4: ASP.NET CORE NDJSON ENDPOINT & HTTPCLIENT STREAM ---");
        Console.WriteLine($"Khoi tao server Kestrel tai {ServerUrl}...\n");

        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls(ServerUrl);
        builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Warning);

        var app = builder.Build();

        app.MapGet("/people-stream", async context =>
        {
            context.Response.ContentType = "application/x-ndjson";

            for (int i = 1; i <= 10; i++)
            {
                var person = new PersonDto
                {
                    Id = i,
                    Name = $"User-{i}",
                    Timestamp = DateTime.UtcNow
                };

                await JsonSerializer.SerializeAsync(context.Response.Body, person);
                await context.Response.WriteAsync("\n");
                await context.Response.Body.FlushAsync();

                await Task.Delay(200);
            }
        });

        var serverTask = app.RunAsync();
        await Task.Delay(400);

        Console.WriteLine($"Server endpoint ready: GET {ServerUrl}/people-stream (Content-Type: application/x-ndjson)\n");

        using var httpClient = new HttpClient();
        using var response = await httpClient.GetAsync($"{ServerUrl}/people-stream", HttpCompletionOption.ResponseHeadersRead);

        Console.WriteLine($"HTTP Status Code: {response.StatusCode}");
        Console.WriteLine($"Header Content-Type: {response.Content.Headers.ContentType}\n");

        using var stream = await response.Content.ReadAsStreamAsync();
        using var reader = new StreamReader(stream);

        int lineCount = 0;
        string? line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            lineCount++;
            var person = JsonSerializer.Deserialize<PersonDto>(line);
            Console.WriteLine($"[Line {lineCount:D2}] Received: Id={person?.Id}, Name={person?.Name}, Timestamp={person?.Timestamp:HH:mm:ss.fff}");
        }

        Console.WriteLine($"\nHttpClient nhan va parse thanh cong {lineCount} dong NDJSON.");

        await app.StopAsync();
        await serverTask;
    }
}
