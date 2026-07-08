using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace IncidentRegistrationMvc.Models;

public class FormResponse : IClientEntity
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public string ResponseId => Id.ToString("0000");
    public DateTime CreatedOn { get; set; } = DateTime.Now;

    public int? ReportedBy { get; set; }

    public User? ReportedByUser { get; set; }

    [EmailAddress]
    [Display(Name = "Email Address")]
    public string? EmailAddress { get; set; }

    [Required]
    [Display(Name = "Reporter Name")]
    public string ReporterName { get; set; } = string.Empty;

    [Required]
    public string Type { get; set; } = string.Empty;

    [Required]
    public string Device { get; set; } = string.Empty;

    [NotMapped]
    [Display(Name = "Other Device")]
    public string? OtherDevice { get; set; }

    [Display(Name = "Location")]
    public string? Location { get; set; }

    [Display(Name = "Echo Machine")]
    public string? EchoMachine { get; set; }

    [Display(Name = "Clinical Eng. #")]
    public string? ClinicalEngNumber { get; set; }

    [Display(Name = "Serial No.")]
    public string? SerialNumber { get; set; }

    [Display(Name = "Property Tag Number")]
    public string? PropertyTagNumber { get; set; }

    [Display(Name = "Annual Preventive Due Date")]
    [DataType(DataType.Date)]
    public DateTime? AnnualPreventiveDueDate { get; set; }

    [Required]
    public string Description { get; set; } = string.Empty;

    public string? ImagePath { get; set; }

    [Required]
    public string Priority { get; set; } = "Low";

    [Display(Name = "Resolved Status")]
    public string ResolvedStatus { get; set; } = "pending";

    public DateTime? ResolvedOn { get; set; }

    [NotMapped]
    public IFormFile? ImageFile { get; set; }
}
