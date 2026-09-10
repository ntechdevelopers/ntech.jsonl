using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

namespace Ntech.JsonL.Helpers;

/// <summary>
/// Helper ho tro thao tac stream NDJSON / JSON Lines trong .NET.
/// Quy cach: Moi doi tuong JSON duoc ghi tren 1 dong, ket thuc bang '\n'.
/// </summary>
public static class NdjsonStreamHelper
{
    private static readonly JsonSerializerOptions DefaultOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Ghi danh sach phan tu tu IAsyncEnumerable sang NDJSON stream.
    /// </summary>
    public static async Task WriteNdjsonStreamAsync<T>(
        Stream destinationStream,
        IAsyncEnumerable<T> items,
        JsonSerializerOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var opts = options ?? DefaultOptions;
        using var writer = new StreamWriter(destinationStream, Encoding.UTF8, leaveOpen: true);

        await foreach (var item in items.WithCancellation(cancellationToken))
        {
            if (item is null) continue;

            string jsonLine = JsonSerializer.Serialize(item, opts);
            await writer.WriteLineAsync(jsonLine.AsMemory(), cancellationToken);
            await writer.FlushAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Doc NDJSON stream va deserialize tung dong thanh doi tuong T.
    /// </summary>
    public static async IAsyncEnumerable<T> ReadNdjsonStreamAsync<T>(
        Stream sourceStream,
        JsonSerializerOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var opts = options ?? DefaultOptions;
        using var reader = new StreamReader(sourceStream, Encoding.UTF8, leaveOpen: true);

        string? line;
        while ((line = await reader.ReadLineAsync(cancellationToken)) != null)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            T? result = default;
            try
            {
                result = JsonSerializer.Deserialize<T>(line, opts);
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"[NdjsonStreamHelper] Deserialization error: {ex.Message}");
            }

            if (result is not null)
            {
                yield return result;
            }
        }
    }

    /// <summary>
    /// Doc tung dong raw string tu NDJSON stream.
    /// </summary>
    public static async IAsyncEnumerable<string> ReadNdjsonLinesAsync(
        Stream sourceStream,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var reader = new StreamReader(sourceStream, Encoding.UTF8, leaveOpen: true);

        string? line;
        while ((line = await reader.ReadLineAsync(cancellationToken)) != null)
        {
            if (!string.IsNullOrWhiteSpace(line))
            {
                yield return line;
            }
        }
    }
}
