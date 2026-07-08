using System.ComponentModel.DataAnnotations;

namespace IncidentRegistrationMvc.Models;

public class DropdownMaster : IClientEntity
{
    public int Id { get; set; }
    public int ClientId { get; set; }

    [Required]
    public string Category { get; set; } = string.Empty;

    [Required]
    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public int SortOrder { get; set; }
}
