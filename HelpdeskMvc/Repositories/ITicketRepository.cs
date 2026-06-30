using HelpdeskMvc.Models;

namespace HelpdeskMvc.Repositories;

public interface ITicketRepository
{
    Task<List<Ticket>> GetAllAsync();
    Task<Ticket?> GetByIdAsync(int id);

    // Saves a Ticket and its first TicketComment in a single transaction.
    Task AddTicketWithCommentAsync(Ticket ticket, TicketComment comment);

    Task UpdateAsync(Ticket ticket);
}
