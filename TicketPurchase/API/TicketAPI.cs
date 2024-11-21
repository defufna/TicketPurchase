using System;
using TicketAPIContract;
using TicketPurchase.Model;
using VeloxDB.ObjectInterface;
using VeloxDB.Protocol;

namespace TicketPurchase.API;

[DbAPI(Name = "TicketingService")]
public class TicketAPI
{
    const int groupSize = 128;

    [DbAPIOperation(ObjectGraphSupport = DbAPIObjectGraphSupportType.None, OperationType = DbAPIOperationType.ReadWrite)]
    public void CreateTickets(ObjectModel objectModel, int count)
    {
        for (int i = 0; i < count; i++)
        {
            var t = objectModel.CreateObject<Ticket>();
            t.Number = i;
            t.IsAvailable = true;
        }

        int groupCount = count / groupSize;
        if (groupCount * groupSize != count)
            groupCount++;

        var root = objectModel.CreateObject<AvailableTicketNode>();
        root.NodeId = AvailableTicketNode.RootId;
        root.Children = DatabaseArray<int>.Create(Enumerable.Range(1, groupCount));

        for (int i = 0; i < groupCount; i++)
        {
            var node = objectModel.CreateObject<AvailableTicketNode>();
            node.NodeId = i + 1;
            node.Children = DatabaseArray<int>.Create(Enumerable.Range(i * groupSize, Math.Min(groupSize, count - i * groupSize)));
        }
    }

    [DbAPIOperation(ObjectGraphSupport = DbAPIObjectGraphSupportType.None, OperationType = DbAPIOperationType.Read)]
    public int[] GetAvailableTickets(ObjectModel objectModel)
    {
        var reader = objectModel.GetHashIndex<AvailableTicketNode, int>(AvailableTicketNode.NodeIdIndexName);
        var root = reader.GetObject(AvailableTicketNode.RootId);
        if (root.Children.Count == 0)
            return Array.Empty<int>();

        int groupId = root.Children[Random.Shared.Next(root.Children.Count)];
        var group = reader.GetObject(groupId);
        return group.Children.ToArray();
    }

    [DbAPIOperation(ObjectGraphSupport = DbAPIObjectGraphSupportType.None, OperationType = DbAPIOperationType.Read)]
    public int[] GetAvailableTicketsFresh(ObjectModel objectModel)
    {
        var l = new List<int>();
        foreach (Ticket ticket in objectModel.GetAllObjects<Ticket>())
        {
            if (ticket.IsAvailable)
                l.Add(ticket.Number);
        }

        return l.ToArray();
    }

    [DbAPIOperation(ObjectGraphSupport = DbAPIObjectGraphSupportType.None, OperationType = DbAPIOperationType.ReadWrite)]
    public void RefreshAvailableTicketList(ObjectModel objectModel, int[] availableTickets)
    {
        var grouped = new Dictionary<int, List<int>>();
        var h = new HashSet<int>();

        for (int i = 0; i < availableTickets.Length; i++)
        {
            int groupId = availableTickets[i] / groupSize + 1;
            h.Add(groupId);

            if (!grouped.TryGetValue(groupId, out var l))
            {
                l = new List<int>();
                grouped.Add(groupId, l);
            }

            l.Add(availableTickets[i]);
        }

        var reader = objectModel.GetHashIndex<AvailableTicketNode, int>(AvailableTicketNode.NodeIdIndexName);
        var root = reader.GetObject(AvailableTicketNode.RootId);
        root.Children = DatabaseArray<int>.Create(h);

        foreach (var kv in grouped)
        {
            var node = reader.GetObject(kv.Key);
            node.Children = DatabaseArray<int>.Create(kv.Value);
        }
    }

    [DbAPIOperation(ObjectGraphSupport = DbAPIObjectGraphSupportType.None, OperationType = DbAPIOperationType.ReadWrite)]
    [DbAPIOperationError(typeof(InvalidTicketNumberException))]
    public bool TryReserveTicket(ObjectModel objectModel, int ticketNumber)
    {
        HashIndexReader<Ticket, int> r = objectModel.GetHashIndex<Ticket, int>(Ticket.OrderNumberIndexName);
        Ticket t = r.GetObject(ticketNumber);
        if (t == null)
            throw new InvalidTicketNumberException("Requested ticket not found.");

        if (!t.IsAvailable)
            return false;

        t.IsAvailable = false;
        return true;
    }
}
