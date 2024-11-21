using System;
using VeloxDB.ObjectInterface;

namespace TicketPurchase.Model;

[DatabaseClass]
[HashIndex(NodeIdIndexName, true, new[] { nameof(NodeId) })]
public abstract class AvailableTicketNode : DatabaseObject
{
    // We will only have a single instance of this type, so assign it a fixed identifier to be able to easily locate it
    public const int RootId = 0;

    public const string NodeIdIndexName = "NodeIdIndex";

    [DatabaseProperty]
    public abstract int NodeId { get; set; }

    // Either ids of children nodes or ticket order numbers
    [DatabaseProperty]
    public abstract DatabaseArray<int> Children { get; set; }
}
