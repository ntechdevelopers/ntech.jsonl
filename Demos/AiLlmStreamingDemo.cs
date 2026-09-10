using System.Diagnostics;
using System.Text.Json;
using Ntech.JsonL.Helpers;
using Ntech.JsonL.Models;

namespace Ntech.JsonL.Demos;

/// <summary>
/// Demo Use Case 1: AI and LLM Streaming Responses
/// </summary>
public static class AiLlmStreamingDemo
{
    public static async Task RunAsync()
    {
        Console.WriteLine("--- DEMO 1: AI & LLM STREAMING RESPONSES (NDJSON) ---");
        Console.WriteLine("Mo phong phan hoi token tu model AI...\n");

        var prompt = "Truyen tai du lieu JSON Lines (NDJSON) trong .NET co loi ich gi?";
        Console.WriteLine($"[Prompt]: {prompt}\n");

        using var stream = new MemoryStream();
        var sw = Stopwatch.StartNew();

        var producerTask = Task.Run(async () =>
        {
            string[] tokens = [
                "JSON", " Lines", " (NDJSON)", " cho", " phép", " truyen", " tai", " du", " lieu",
                " theo", " tung", " dong", " ngay", " khi", " đuoc", " tao", " ra.",
                " Phuong", " phap", " nay", " giam", " thoi", " gian", " phan", " hoi",
                " ban", " ghi", " dau", " tien,", " toi", " uu", " dung", " luong", " RAM",
                " va", " thich", " hop", " cho", " cac", " he", " thong", " stream", " real-time."
            ];

            using var writer = new StreamWriter(stream, leaveOpen: true);

            for (int i = 0; i < tokens.Length; i++)
            {
                var chunk = new AiStreamChunk
                {
                    Type = "content",
                    Text = tokens[i],
                    TokenId = i + 1
                };

                string jsonLine = JsonSerializer.Serialize(chunk);
                await writer.WriteLineAsync(jsonLine);
                await writer.FlushAsync();

                await Task.Delay(Random.Shared.Next(25, 50));
            }

            var doneChunk = new AiStreamChunk
            {
                Type = "done",
                TotalTokens = tokens.Length,
                ExecutionTimeMs = sw.ElapsedMilliseconds
            };
            await writer.WriteLineAsync(JsonSerializer.Serialize(doneChunk));
            await writer.FlushAsync();
        });

        Console.Write("[AI Stream Output]: ");

        long firstTokenMs = 0;
        int tokenCount = 0;
        using var readerStream = new StreamView(stream);

        await foreach (var chunk in NdjsonStreamHelper.ReadNdjsonStreamAsync<AiStreamChunk>(readerStream))
        {
            if (chunk.Type == "content" && chunk.Text is not null)
            {
                tokenCount++;
                if (tokenCount == 1)
                {
                    firstTokenMs = sw.ElapsedMilliseconds;
                }

                Console.Write(chunk.Text);
            }
            else if (chunk.Type == "done")
            {
                sw.Stop();
                Console.WriteLine("\n");
                Console.WriteLine($"[Metrics]");
                Console.WriteLine($"Time-To-First-Token (TTFT): {firstTokenMs} ms");
                Console.WriteLine($"Total Tokens: {chunk.TotalTokens}");
                Console.WriteLine($"Total Execution Time: {sw.ElapsedMilliseconds} ms");
            }
        }

        await producerTask;
    }

    private class StreamView : Stream
    {
        private readonly MemoryStream _inner;
        private long _readPosition = 0;

        public StreamView(MemoryStream inner) => _inner = inner;

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => _inner.Length;
        public override long Position { get => _readPosition; set => throw new NotSupportedException(); }

        public override int Read(byte[] buffer, int offset, int count)
        {
            lock (_inner)
            {
                _inner.Position = _readPosition;
                int bytesRead = _inner.Read(buffer, offset, count);
                _readPosition = _inner.Position;
                return bytesRead;
            }
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            while (true)
            {
                lock (_inner)
                {
                    _inner.Position = _readPosition;
                    int bytesRead = _inner.Read(buffer, offset, count);
                    if (bytesRead > 0)
                    {
                        _readPosition = _inner.Position;
                        return bytesRead;
                    }
                }
                await Task.Delay(10, cancellationToken);
            }
        }

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            return new ValueTask<int>(ReadAsync(buffer.ToArray(), 0, buffer.Length, cancellationToken));
        }

        public override void Flush() { }
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
