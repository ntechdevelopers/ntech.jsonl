using System.Text.Json.Serialization;

namespace Ntech.JsonL.Models;

/// <summary>
/// Model cho Use Case 3: Event Streaming Systems (Kafka / RabbitMQ / IoT style)
/// </summary>
public record DomainEvent
{
    [JsonPropertyName("eventId")]
    public string EventId { get; init; } = Guid.NewGuid().ToString("N");

    [JsonPropertyName("event")]
    public string Event { get; init; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    [JsonPropertyName("payload")]
    public object? Payload { get; init; }
}
