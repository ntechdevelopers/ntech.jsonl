using System.Diagnostics;
using System.Text.Json;
using Ntech.JsonL.Helpers;
using Ntech.JsonL.Models;

namespace Ntech.JsonL.Demos;

/// <summary>
/// Demo Use Case 2: Large Data Export APIs
/// So sanh dung luong RAM va thoi gian phan hoi ban ghi dau tien.
/// </summary>
public static class LargeDataExportDemo
{
    public static async Task RunAsync()
    {
        Console.WriteLine("--- DEMO 2: LARGE DATA EXPORT APIs (BENCHMARK) ---");

        int recordCount = 50_000;
        Console.WriteLine($"Kiem thu xuat {recordCount:N0} ban ghi Customer...\n");

        // ----------------------------------------------------
        // Phuong phap 1: Standard JSON Array [...]
        // ----------------------------------------------------
        Console.WriteLine("1. Phuong phap Standard JSON Array [...]");

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        long memBefore1 = GC.GetTotalMemory(true);

        var sw1 = Stopwatch.StartNew();

        var customerList = new List<CustomerRecord>(recordCount);
        for (int i = 1; i <= recordCount; i++)
        {
            customerList.Add(new CustomerRecord
            {
                CustomerId = i,
                Name = $"Customer-{i}",
                Email = $"customer{i}@ntechdevelopers.com",
                RegisteredAt = DateTime.UtcNow.AddMinutes(-i),
                Balance = 1000.50m + i
            });
        }

        using var jsonStream1 = new MemoryStream();
        await JsonSerializer.SerializeAsync(jsonStream1, customerList);

        sw1.Stop();
        long memAfter1 = GC.GetTotalMemory(false);
        long memDiff1 = (memAfter1 - memBefore1) / (1024 * 1024);

        Console.WriteLine($"   - Thoi gian khoi tao & serialize JSON Array: {sw1.ElapsedMilliseconds} ms");
        Console.WriteLine($"   - Dung luong RAM cap phat: ~{memDiff1} MB");
        Console.WriteLine($"   - Dung luong payload: {jsonStream1.Length / (1024 * 1024)} MB\n");

        customerList.Clear();
        customerList = null;

        // ----------------------------------------------------
        // Phuong phap 2: NDJSON Streaming
        // ----------------------------------------------------
        Console.WriteLine("2. Phuong phap NDJSON Stream {...}\\n{...}\\n");

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        long memBefore2 = GC.GetTotalMemory(true);

        var sw2 = Stopwatch.StartNew();
        long timeToFirstRecordMs = 0;
        int readCount = 0;

        using var jsonLStream = new MemoryStream();

        async IAsyncEnumerable<CustomerRecord> GenerateRecordsAsync()
        {
            for (int i = 1; i <= recordCount; i++)
            {
                yield return new CustomerRecord
                {
                    CustomerId = i,
                    Name = $"Customer-{i}",
                    Email = $"customer{i}@ntechdevelopers.com",
                    RegisteredAt = DateTime.UtcNow.AddMinutes(-i),
                    Balance = 1000.50m + i
                };
            }
        }

        var writeTask = NdjsonStreamHelper.WriteNdjsonStreamAsync(jsonLStream, GenerateRecordsAsync());

        jsonLStream.Position = 0;
        await foreach (var record in NdjsonStreamHelper.ReadNdjsonStreamAsync<CustomerRecord>(jsonLStream))
        {
            readCount++;
            if (readCount == 1)
            {
                timeToFirstRecordMs = sw2.ElapsedMilliseconds;
            }
        }

        await writeTask;
        sw2.Stop();

        long memAfter2 = GC.GetTotalMemory(false);
        long memDiff2 = Math.Max(0, (memAfter2 - memBefore2) / (1024 * 1024));

        Console.WriteLine($"   - Thoi gian nhan ban ghi dau tien (TTFB): {timeToFirstRecordMs} ms");
        Console.WriteLine($"   - Tong thoi gian xu ly stream: {sw2.ElapsedMilliseconds} ms");
        Console.WriteLine($"   - Dung luong RAM cap phat: ~{memDiff2} MB");
        Console.WriteLine($"   - So luong ban ghi da xu ly: {readCount:N0}\n");

        Console.WriteLine("[Ket luan]");
        Console.WriteLine($"   - Chenh lech RAM cap phat: tiet kiem ~{memDiff1 - memDiff2} MB");
        Console.WriteLine($"   - Client nhan du lieu ban ghi #1 sau {timeToFirstRecordMs} ms so voi {sw1.ElapsedMilliseconds} ms cua mảng JSON.");
    }
}
