using Microsoft.AspNetCore.Mvc;
using TicketAPIContract;
using VeloxDB.Client;

namespace TicketAPI.Controllers
{
    [ApiController]
    public class TicketController : ControllerBase
    {
        const string ticketingDB = "Ticketing";

        readonly IConfiguration configuration;

        public TicketController(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        [HttpGet]
        [Route("ticket/get-available")]
        public async Task<int[]> GetAvailableTicketsAsync()
        {
            ITicketingService ticketingService = ConnectionFactory.Get<ITicketingService>(configuration.GetConnectionString(ticketingDB));
            return await ticketingService.GetAvailableTickets();
        }

        [HttpPost]
        [Route("ticket/try-reserve/{ticketNum}")]
        public async Task<bool> TryReserveTicketAsync(int ticketNum)
        {
            ITicketingService ticketingService = ConnectionFactory.Get<ITicketingService>(configuration.GetConnectionString(ticketingDB));
            return await ticketingService.TryReserveTicket(ticketNum);
        }
    }
}
