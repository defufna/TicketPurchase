
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Text.Json;
using TicketAPIContract;
using VeloxDB.Client;

namespace StressClient;

internal class Program
{
    static string hostAddress;
    static ConcurrentBag<HttpClient> pool;

    static string connectionString;

    static void Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Usage: Program <hostAddress> <userCount>");
            return;
        }

        hostAddress = args[0];
        if (!int.TryParse(args[1], out var userCount))
        {
            Console.WriteLine("Invalid user count. Please provide a valid integer.");
            return;
        }

        pool = new ConcurrentBag<HttpClient>();

        Uri uri;
        try
        {
            uri = new Uri(hostAddress);
        }
        catch (UriFormatException)
        {
            Console.WriteLine("Invalid host address. Please provide a valid URI.");
            return;
        }

        bool vlx = uri.Scheme.Equals("vlx", StringComparison.OrdinalIgnoreCase);
        Func<Task> tryReserveTicket = TryReserveTicket;

        if (vlx)
        {
            ConnectionStringParams connectionParams = new();
            connectionParams.AddAddress($"{uri.Host}:{uri.Port}");
            connectionParams.PoolSize = 4;
            connectionParams.BufferPoolSize = 1024 * 1024 * 128;
            connectionParams.OpenTimeout = 5000;
            connectionString = connectionParams.GenerateConnectionString();
            tryReserveTicket = TryReserveTicketVlx;
        }

        Stopwatch s = Stopwatch.StartNew();

        List<Task> l = new List<Task>();
        for (int i = 0; i < userCount; i++)
        {
            l.Add(Task.Run(tryReserveTicket));
        }

        Task.WaitAll(l.ToArray());
        s.Stop();

        Console.WriteLine(s.ElapsedMilliseconds);
    }

    private static async Task TryReserveTicketVlx()
    {
        while (true)
        {
            ITicketingService ticketingService = ConnectionFactory.Get<ITicketingService>(connectionString);
            try
            {
                int[] ticketNumbers = await ticketingService.GetAvailableTickets();
                if (ticketNumbers.Length == 0)
                {
                    return;
                }

                int ticketNumber = ticketNumbers[Random.Shared.Next(ticketNumbers.Length)];
                bool success = await ticketingService.TryReserveTicket(ticketNumber);

                if (success)
                    return;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }


    private static async Task TryReserveTicket()
    {
        while (true)
        {
            pool.TryTake(out var client);
            if (client == null)
            {
                CookieContainer cookies = new CookieContainer();
                HttpClientHandler handler = new HttpClientHandler();
                handler.CookieContainer = cookies;
                client = new HttpClient(handler);
                client.DefaultRequestVersion = new Version(1, 1);
                client.DefaultRequestHeaders.ConnectionClose = false;
            }

            try
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"{hostAddress}/ticket/get-available")
                {
                    Version = HttpVersion.Version11,
                    VersionPolicy = HttpVersionPolicy.RequestVersionOrHigher
                };

                HttpResponseMessage response = await client.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                    throw new HttpRequestException();

                int[] ticketNumbers = JsonSerializer.Deserialize<int[]>(await response.Content.ReadAsStringAsync());
                if (ticketNumbers.Length == 0)
                {
                    Console.WriteLine("Done");
                    return;
                }

                int ticketNumber = ticketNumbers[Random.Shared.Next(ticketNumbers.Length)];
                request = new HttpRequestMessage(HttpMethod.Post, $"{hostAddress}/ticket/try-reserve/{ticketNumber}")
                {
                    Version = HttpVersion.Version11,
                    VersionPolicy = HttpVersionPolicy.RequestVersionOrHigher
                };

                response = await client.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                    throw new HttpRequestException();

                bool success = JsonSerializer.Deserialize<bool>(await response.Content.ReadAsStringAsync());
                pool.Add(client);

                if (success)
                    return;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                client.Dispose();
            }
        }
    }
}
