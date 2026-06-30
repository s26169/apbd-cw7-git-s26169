using HelpdeskMvc.Data;
using HelpdeskMvc.Models;
using Microsoft.EntityFrameworkCore;

namespace HelpdeskMvc.Repositories;

public class TicketRepository : ITicketRepository
{
    private readonly AppDbContext _context;

    public TicketRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Ticket>> GetAllAsync()
    {
        return await _context.Tickets
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<Ticket?> GetByIdAsync(int id)
    {
        return await _context.Tickets
            .Include(t => t.Comments)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    // This is the transactional part required by the assignment: the ticket
    // and its first comment are written as two separate SaveChanges calls,
    // but wrapped in one explicit transaction. If the second insert fails,
    // everything (including the ticket insert) is rolled back.
    public async Task AddTicketWithCommentAsync(Ticket ticket, TicketComment comment)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync(); // needed so ticket.Id is generated

            comment.TicketId = ticket.Id;
            _context.TicketComments.Add(comment);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task UpdateAsync(Ticket ticket)
    {
        _context.Tickets.Update(ticket);
        await _context.SaveChangesAsync();
    }
}
