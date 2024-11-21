using TicketAPIContract;
using VeloxDB.Client;

namespace Initializer
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            string connStr = args[0];
            ITicketingService ticketingService = ConnectionFactory.Get<ITicketingService>(connStr);

            int count = int.Parse(args[1]);
            await ticketingService.CreateTickets(count);
        }
    }
}
