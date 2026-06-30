using HelpdeskMvc.Models;
using HelpdeskMvc.Repositories;

namespace HelpdeskMvc.Services;

public class TicketService : ITicketService
{
    private readonly ITicketRepository _repository;

    public TicketService(ITicketRepository repository)
    {
        _repository = repository;
    }

    public Task<List<Ticket>> GetAllTicketsAsync() => _repository.GetAllAsync();

    public Task<Ticket?> GetTicketDetailsAsync(int id) => _repository.GetByIdAsync(id);

    public async Task<ServiceResult> CreateTicketAsync(string title, string description, string author, string commentContent)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(title))
        {
            errors.Add("Title is required.");
        }

        if (string.IsNullOrWhiteSpace(commentContent))
        {
            errors.Add("The first comment cannot be empty.");
        }

        if (errors.Count > 0)
        {
            return new ServiceResult { Success = false, Errors = errors };
        }

        var ticket = new Ticket
        {
            Title = title.Trim(),
            Description = description?.Trim() ?? string.Empty,
            Status = "Open",
            CreatedAt = DateTime.UtcNow
        };

        var comment = new TicketComment
        {
            Author = string.IsNullOrWhiteSpace(author) ? "Anonymous" : author.Trim(),
            Content = commentContent.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddTicketWithCommentAsync(ticket, comment);

        return new ServiceResult { Success = true, TicketId = ticket.Id };
    }

    public async Task<bool> CloseTicketAsync(int id)
    {
        var ticket = await _repository.GetByIdAsync(id);
        if (ticket is null)
        {
            return false;
        }

        ticket.Status = "Closed";
        await _repository.UpdateAsync(ticket);
        return true;
    }
}
