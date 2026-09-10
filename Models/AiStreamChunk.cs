using System.Text.Json.Serialization;

namespace Ntech.JsonL.Models;

/// <summary>
/// Model cho Use Case 1: AI / LLM Streaming Responses
/// </summary>
public record AiStreamChunk
{
    [JsonPropertyName("type")]
    public string Type { get; init; } = "content"; // "content" | "done" | "error"

    [JsonPropertyName("text")]
    public string? Text { get; init; }

    [JsonPropertyName("tokenId")]
    public int? TokenId { get; init; }

    [JsonPropertyName("totalTokens")]
    public int? TotalTokens { get; init; }

    [JsonPropertyName("executionTimeMs")]
    public long? ExecutionTimeMs { get; init; }
}
