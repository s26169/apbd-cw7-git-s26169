using HelpdeskMvc.Services;
using HelpdeskMvc.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace HelpdeskMvc.Controllers;

public class TicketsController : Controller
{
    private readonly ITicketService _ticketService;

    public TicketsController(ITicketService ticketService)
    {
        _ticketService = ticketService;
    }

    // GET /Tickets
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var tickets = await _ticketService.GetAllTicketsAsync();
        return View(tickets);
    }

    // GET /Tickets/Create
    [HttpGet]
    public IActionResult Create()
    {
        return View(new CreateTicketViewModel());
    }

    // POST /Tickets/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateTicketViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _ticketService.CreateTicketAsync(
            model.Title,
            model.Description ?? string.Empty,
            model.Author ?? string.Empty,
            model.CommentContent);

        if (!result.Success)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }

            return View(model);
        }

        return RedirectToAction(nameof(Details), new { id = result.TicketId });
    }

    // GET /Tickets/Details/{id}
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var ticket = await _ticketService.GetTicketDetailsAsync(id);

        if (ticket is null)
        {
            return NotFound();
        }

        return View(ticket);
    }

    // POST /Tickets/Close/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Close(int id)
    {
        await _ticketService.CloseTicketAsync(id);
        return RedirectToAction(nameof(Details), new { id });
    }
}
