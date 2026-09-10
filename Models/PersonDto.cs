using System.Text.Json.Serialization;

namespace Ntech.JsonL.Models;

/// <summary>
/// Model cho Demo ASP.NET Core Endpoint /people-stream
/// </summary>
public record PersonDto
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; init; }
}
