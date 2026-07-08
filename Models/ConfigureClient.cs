using System.ComponentModel.DataAnnotations;

namespace IncidentRegistrationMvc.Models;

public class ConfigureClient
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Project Title")]
    public string ProjectTitle { get; set; } = string.Empty;

    [Display(Name = "Client Name")]
    public string? ClientName { get; set; }

    [Display(Name = "Contact No")]
    public string? ContactNo { get; set; }

    [EmailAddress]
    public string? Email { get; set; }

    public string? Website { get; set; }

    [Display(Name = "Address")]
    public string? Address { get; set; }

    public string? City { get; set; }

    public string? Country { get; set; }

    [Display(Name = "Support Contact")]
    public string? SupportContact { get; set; }

    [Display(Name = "Support Email")]
    [EmailAddress]
    public string? SupportEmail { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedOn { get; set; } = DateTime.Now;

    public DateTime? UpdatedOn { get; set; }
}
