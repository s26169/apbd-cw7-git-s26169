using HelpdeskMvc.Models;
using HelpdeskMvc.Repositories;

namespace HelpdeskMvc.Tests.Fakes;

// A simple in-memory stand-in for ITicketRepository. We're testing the
// Service layer's logic (validation, status changes), not real ADO.NET/EF Core
// infrastructure, so a fake list is enough.
public class FakeTicketRepository : ITicketRepository
{
    private readonly List<Ticket> _tickets = new();
    private int _nextTicketId = 1;
    private int _nextCommentId = 1;

    public Task<List<Ticket>> GetAllAsync()
    {
        return Task.FromResult(_tickets.ToList());
    }

    public Task<Ticket?> GetByIdAsync(int id)
    {
        return Task.FromResult(_tickets.FirstOrDefault(t => t.Id == id));
    }

    public Task AddTicketWithCommentAsync(Ticket ticket, TicketComment comment)
    {
        ticket.Id = _nextTicketId++;
        comment.Id = _nextCommentId++;
        comment.TicketId = ticket.Id;

        ticket.Comments.Add(comment);
        _tickets.Add(ticket);

        return Task.CompletedTask;
    }

    public Task UpdateAsync(Ticket ticket)
    {
        var existing = _tickets.FirstOrDefault(t => t.Id == ticket.Id);
        if (existing is not null)
        {
            existing.Status = ticket.Status;
        }

        return Task.CompletedTask;
    }
}
