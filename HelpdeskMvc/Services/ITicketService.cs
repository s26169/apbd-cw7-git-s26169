using HelpdeskMvc.Models;

namespace HelpdeskMvc.Services;

public interface ITicketService
{
    Task<List<Ticket>> GetAllTicketsAsync();
    Task<Ticket?> GetTicketDetailsAsync(int id);
    Task<ServiceResult> CreateTicketAsync(string title, string description, string author, string commentContent);
    Task<bool> CloseTicketAsync(int id);
}
