using HelpdeskMvc.Services;
using HelpdeskMvc.Tests.Fakes;
using Xunit;

namespace HelpdeskMvc.Tests;

public class TicketServiceTests
{
    [Fact]
    public async Task CreateTicketAsync_WithValidData_CreatesTicketAndComment()
    {
        var repository = new FakeTicketRepository();
        var service = new TicketService(repository);

        var result = await service.CreateTicketAsync(
            "Printer is broken",
            "Office printer jams on every print job.",
            "Anna",
            "It won't print anything since this morning.");

        Assert.True(result.Success);
        Assert.NotNull(result.TicketId);

        var tickets = await repository.GetAllAsync();
        Assert.Single(tickets);
        Assert.Equal("Printer is broken", tickets[0].Title);
        Assert.Single(tickets[0].Comments);
        Assert.Equal("Anna", tickets[0].Comments[0].Author);
    }

    [Fact]
    public async Task CreateTicketAsync_WithEmptyTitle_ReturnsValidationErrorAndSavesNothing()
    {
        var repository = new FakeTicketRepository();
        var service = new TicketService(repository);

        var result = await service.CreateTicketAsync(
            title: "   ",
            description: "Some description",
            author: "Anna",
            commentContent: "First comment");

        Assert.False(result.Success);
        Assert.Contains(result.Errors, e => e.Contains("Title"));

        var tickets = await repository.GetAllAsync();
        Assert.Empty(tickets);
    }

    [Fact]
    public async Task CreateTicketAsync_WithEmptyComment_ReturnsValidationErrorAndSavesNothing()
    {
        var repository = new FakeTicketRepository();
        var service = new TicketService(repository);

        var result = await service.CreateTicketAsync(
            title: "Valid title",
            description: "Some description",
            author: "Anna",
            commentContent: "   ");

        Assert.False(result.Success);
        Assert.Contains(result.Errors, e => e.Contains("comment"));

        var tickets = await repository.GetAllAsync();
        Assert.Empty(tickets);
    }

    [Fact]
    public async Task CloseTicketAsync_OnExistingTicket_ChangesStatusToClosed()
    {
        var repository = new FakeTicketRepository();
        var service = new TicketService(repository);

        var created = await service.CreateTicketAsync("Title", "Desc", "Anna", "First comment");

        var closed = await service.CloseTicketAsync(created.TicketId!.Value);

        Assert.True(closed);
        var ticket = await repository.GetByIdAsync(created.TicketId.Value);
        Assert.Equal("Closed", ticket!.Status);
    }

    [Fact]
    public async Task CloseTicketAsync_OnNonExistingTicket_ReturnsFalse()
    {
        var repository = new FakeTicketRepository();
        var service = new TicketService(repository);

        var closed = await service.CloseTicketAsync(999);

        Assert.False(closed);
    }
}
