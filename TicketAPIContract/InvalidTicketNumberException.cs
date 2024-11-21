using System;
using VeloxDB.Protocol;

namespace TicketAPIContract;

public class InvalidTicketNumberException : DbAPIErrorException
{
    public InvalidTicketNumberException()
    {
    }

    public InvalidTicketNumberException(string message) : base(message)
    {
    }
}
