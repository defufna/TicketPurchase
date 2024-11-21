using VeloxDB.ObjectInterface;

namespace TicketPurchase.Model;

// Ticket:
// Order number
// IsAvailable

[DatabaseClass]
[HashIndex(OrderNumberIndexName, true, [nameof(Number)])]
public abstract class Ticket : DatabaseObject
{
    public const string OrderNumberIndexName = "OrderNumberIndex";

    [DatabaseProperty]
    public abstract int Number { get; set; }

    [DatabaseProperty]
    public abstract bool IsAvailable { get; set; }

    // Any additional information regarding the ticket (such as location, purchaser information...)
}
