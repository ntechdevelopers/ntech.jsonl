using System.Text.Json.Serialization;

namespace Ntech.JsonL.Models;

/// <summary>
/// Model cho Use Case 2: Large Data Export APIs
/// </summary>
public record CustomerRecord
{
    [JsonPropertyName("customerId")]
    public int CustomerId { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; init; } = string.Empty;

    [JsonPropertyName("registeredAt")]
    public DateTime RegisteredAt { get; init; }

    [JsonPropertyName("balance")]
    public decimal Balance { get; init; }
}
