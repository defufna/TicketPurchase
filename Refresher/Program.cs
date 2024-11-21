using TicketAPIContract;
using VeloxDB.Client;

namespace Refresher;

internal class Program
{
    static async Task Main(string[] args)
    {
        string connStr = args[0];
        ITicketingService ticketingService = ConnectionFactory.Get<ITicketingService>(connStr);

        int period = args.Length > 1 ? int.Parse(args[1]) : 1000;
        while (true)
        {
            try
            {
                int[] ticketNumbers = await ticketingService.GetAvailableTicketsFresh();
                await ticketingService.RefreshAvailableTicketList(ticketNumbers);
                Console.WriteLine($"Ticket state refreshed at {DateTime.Now}. Remaining tickets {ticketNumbers.Length}.");
                await Task.Delay(period);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.GetType().FullName);
                Console.WriteLine(e.Message);
            }
        }
    }
}
