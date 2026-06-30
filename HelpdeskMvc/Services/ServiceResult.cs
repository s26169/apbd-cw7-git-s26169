namespace HelpdeskMvc.Services;

public class ServiceResult
{
    public bool Success { get; set; }
    public List<string> Errors { get; set; } = new();
    public int? TicketId { get; set; }
}
