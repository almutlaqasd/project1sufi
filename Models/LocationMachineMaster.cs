using System.ComponentModel.DataAnnotations;

namespace IncidentRegistrationMvc.Models;

public class LocationMachineMaster : IClientEntity
{
    public int Id { get; set; }
    public int ClientId { get; set; }

    [Required(ErrorMessage = "Echo Machine is required.")]
    [StringLength(100)]
    [Display(Name = "Echo Machine")]
    public string EchoMachine { get; set; } = string.Empty;

    [Required(ErrorMessage = "Clinical Engineering Number is required.")]
    [StringLength(50)]
    [Display(Name = "Clinical Engineering Number")]
    public string ClinicalEngNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Location is required.")]
    [StringLength(100)]
    public string Location { get; set; } = string.Empty;

    [Required(ErrorMessage = "Serial Number is required.")]
    [StringLength(100)]
    [Display(Name = "Serial Number")]
    public string? SerialNumber { get; set; }

    [Required(ErrorMessage = "Property Tag Number is required.")]
    [StringLength(100)]
    [Display(Name = "Property Tag Number")]
    public string PropertyTagNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Date Acquired is required.")]
    [StringLength(50)]
    [Display(Name = "Date Acquired")]
    public string? DateAcquired { get; set; }

    public string? Problems { get; set; }

    [Required(ErrorMessage = "Date Reported is required.")]
    [StringLength(50)]
    [Display(Name = "Date Reported")]
    public string? DateReported { get; set; }

    [Display(Name = "Action Taken")]
    public string? ActionTaken { get; set; }

    [Display(Name = "Final Action/Recommendation")]
    public string? FinalActionRecommendation { get; set; }

    [Required(ErrorMessage = "Weekly Filter Cleaning is required.")]
    [StringLength(20)]
    [Display(Name = "Weekly Filter Cleaning")]
    public string? WeeklyFilterCleaningDone { get; set; }

    [Required(ErrorMessage = "Annual Due Date is required.")]
    [StringLength(50)]
    [Display(Name = "Annual Due Date")]
    public string? AnnualDueDate { get; set; }

    [Required(ErrorMessage = "Type is required.")]
    [Display(Name = "Type")]
    public int? UserAccessRightId { get; set; }

    public UserAccessRight? UserAccessRight { get; set; }

    [Required(ErrorMessage = "Annual Preventive Due Date is required.")]
    [Display(Name = "Annual Preventive Due Date")]
    public DateTime? AnnualPreventiveDueDate { get; set; }

    public bool IsActive { get; set; } = true;
}
