using System.ComponentModel.DataAnnotations;

namespace HelpdeskMvc.ViewModels;

public class CreateTicketViewModel
{
    [Required(ErrorMessage = "Title is required.")]
    [Display(Name = "Title")]
    public string Title { get; set; } = string.Empty;

    [Display(Name = "Description")]
    public string? Description { get; set; }

    [Display(Name = "Your name")]
    public string? Author { get; set; }

    [Required(ErrorMessage = "The first comment cannot be empty.")]
    [Display(Name = "First comment")]
    public string CommentContent { get; set; } = string.Empty;
}
