using VeloxDB.Client;
using VeloxDB.Protocol;

namespace TicketAPIContract
{
    [DbAPI(Name = "TicketingService")]
    public interface ITicketingService
    {
        [DbAPIOperation(ObjectGraphSupport = DbAPIObjectGraphSupportType.None, OperationType = DbAPIOperationType.ReadWrite)]
        DatabaseTask CreateTickets(int count);

        [DbAPIOperation(ObjectGraphSupport = DbAPIObjectGraphSupportType.None, OperationType = DbAPIOperationType.Read)]
        DatabaseTask<int[]> GetAvailableTickets();

        [DbAPIOperation(ObjectGraphSupport = DbAPIObjectGraphSupportType.None, OperationType = DbAPIOperationType.Read)]
        DatabaseTask<int[]> GetAvailableTicketsFresh();

        [DbAPIOperation(ObjectGraphSupport = DbAPIObjectGraphSupportType.None, OperationType = DbAPIOperationType.ReadWrite)]
        DatabaseTask RefreshAvailableTicketList(int[] availableTickets);

        [DbAPIOperation(ObjectGraphSupport = DbAPIObjectGraphSupportType.None, OperationType = DbAPIOperationType.ReadWrite)]
        [DbAPIOperationError(typeof(InvalidTicketNumberException))]
        DatabaseTask<bool> TryReserveTicket(int ticketNumber);
    }
}
