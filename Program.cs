using Ntech.JsonL.Demos;

namespace Ntech.JsonL;

internal class Program
{
    private static async Task Main(string[] args)
    {
        SafeSetTitle("Ntech.JsonL - Demo Streaming JSON Lines in .NET");

        if (args.Length > 0)
        {
            var flag = args[0].ToLowerInvariant();
            if (flag is "--all" or "5")
            {
                await RunAllDemosAsync();
                return;
            }
            if (flag is "--ai" or "1")
            {
                await AiLlmStreamingDemo.RunAsync();
                return;
            }
            if (flag is "--export" or "2")
            {
                await LargeDataExportDemo.RunAsync();
                return;
            }
            if (flag is "--event" or "3")
            {
                await EventStreamingDemo.RunAsync();
                return;
            }
            if (flag is "--http" or "4")
            {
                await AspNetCoreEndpointDemo.RunAsync();
                return;
            }
        }

        bool exit = false;

        while (!exit)
        {
            SafeClear();
            PrintHeader();

            Console.WriteLine("  Danh sach demo:");
            Console.WriteLine("  1. Demo 1: AI & LLM Streaming Responses");
            Console.WriteLine("  2. Demo 2: Large Data Export APIs (Memory & TTFB Comparison)");
            Console.WriteLine("  3. Demo 3: Real-Time Event Streaming System");
            Console.WriteLine("  4. Demo 4: ASP.NET Core NDJSON Endpoint & HttpClient Stream Reader");
            Console.WriteLine("  5. Chay toan bo Demo");
            Console.WriteLine("  0. Thoat\n");

            Console.Write("Nhap luong chon [0-5]: ");
            var input = Console.ReadLine()?.Trim();

            switch (input)
            {
                case "1":
                    await AiLlmStreamingDemo.RunAsync();
                    PressAnyKeyToContinue();
                    break;
                case "2":
                    await LargeDataExportDemo.RunAsync();
                    PressAnyKeyToContinue();
                    break;
                case "3":
                    await EventStreamingDemo.RunAsync();
                    PressAnyKeyToContinue();
                    break;
                case "4":
                    await AspNetCoreEndpointDemo.RunAsync();
                    PressAnyKeyToContinue();
                    break;
                case "5":
                    await RunAllDemosAsync();
                    PressAnyKeyToContinue();
                    break;
                case "0":
                    exit = true;
                    Console.WriteLine("\nDa me chuong trinh.");
                    break;
                default:
                    Console.WriteLine("\nLua chon khong hop le.");
                    PressAnyKeyToContinue();
                    break;
            }
        }
    }

    private static void PrintHeader()
    {
        Console.WriteLine("===================================================================");
        Console.WriteLine(" Ntech.JsonL: Streaming JSON Lines (NDJSON) Architecture Demo");
        Console.WriteLine("===================================================================\n");
    }

    private static async Task RunAllDemosAsync()
    {
        SafeClear();
        PrintHeader();
        Console.WriteLine("--- CHAY TAT CA DEMO ---\n");

        await AiLlmStreamingDemo.RunAsync();
        Console.WriteLine("\n--------------------------------------------------------\n");
        await Task.Delay(300);

        await LargeDataExportDemo.RunAsync();
        Console.WriteLine("\n--------------------------------------------------------\n");
        await Task.Delay(300);

        await EventStreamingDemo.RunAsync();
        Console.WriteLine("\n--------------------------------------------------------\n");
        await Task.Delay(300);

        await AspNetCoreEndpointDemo.RunAsync();
    }

    private static void SafeClear()
    {
        try
        {
            if (!Console.IsOutputRedirected)
            {
                Console.Clear();
            }
        }
        catch { }
    }

    private static void SafeSetTitle(string title)
    {
        try
        {
            if (!Console.IsOutputRedirected)
            {
                Console.Title = title;
            }
        }
        catch { }
    }

    private static void PressAnyKeyToContinue()
    {
        Console.WriteLine("\nNhan phim bat ky de tiep tuc...");
        try
        {
            if (!Console.IsInputRedirected)
            {
                Console.ReadKey(true);
            }
            else
            {
                Console.ReadLine();
            }
        }
        catch { }
    }
}
