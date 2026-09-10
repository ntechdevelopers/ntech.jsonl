using System.Text.Json;
using System.Threading.Channels;
using Ntech.JsonL.Models;

namespace Ntech.JsonL.Demos;

/// <summary>
/// Demo Use Case 3: Event Streaming Systems
/// </summary>
public static class EventStreamingDemo
{
    public static async Task RunAsync()
    {
        Console.WriteLine("--- DEMO 3: REAL-TIME EVENT STREAMING SYSTEM (NDJSON) ---");
        Console.WriteLine("Mo phong luong event tu Event Pipeline...\n");

        var channel = Channel.CreateUnbounded<DomainEvent>();

        var producerTask = Task.Run(async () =>
        {
            string[] eventTypes = ["UserCreated", "OrderPlaced", "PaymentReceived", "InventoryUpdated"];

            for (int i = 1; i <= 8; i++)
            {
                var evtType = eventTypes[Random.Shared.Next(eventTypes.Length)];
                var domainEvent = new DomainEvent
                {
                    Event = evtType,
                    Timestamp = DateTime.UtcNow,
                    Payload = evtType switch
                    {
                        "UserCreated" => new { UserId = 1000 + i, Username = $"user_{i}" },
                        "OrderPlaced" => new { OrderId = 5000 + i, Amount = 99.99 * i },
                        "PaymentReceived" => new { PaymentId = 9000 + i, Status = "SUCCESS" },
                        "InventoryUpdated" => new { SKU = $"SKU-ITEM-{i}", Quantity = 50 * i },
                        _ => new { Data = "Default" }
                    }
                };

                await channel.Writer.WriteAsync(domainEvent);
                await Task.Delay(Random.Shared.Next(150, 350));
            }

            channel.Writer.Complete();
        });

        using var stream = new MemoryStream();
        using var writer = new StreamWriter(stream, leaveOpen: true);

        int processedEvents = 0;

        await foreach (var domainEvent in channel.Reader.ReadAllAsync())
        {
            processedEvents++;

            string jsonLine = JsonSerializer.Serialize(domainEvent, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            });

            await writer.WriteLineAsync(jsonLine);
            await writer.FlushAsync();

            var deserializedEvt = JsonSerializer.Deserialize<DomainEvent>(jsonLine);
            Console.WriteLine($"[Event #{processedEvents:D2}] Type={deserializedEvt?.Event} | Raw NDJSON: {jsonLine}");
        }

        await producerTask;
        Console.WriteLine($"\nDa xu ly {processedEvents} event lines.");
    }
}
