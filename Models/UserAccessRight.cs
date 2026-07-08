using System.ComponentModel.DataAnnotations;

namespace IncidentRegistrationMvc.Models;

public class UserAccessRight : IClientEntity
{
    public int Id { get; set; }
    public int ClientId { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedOn { get; set; } = DateTime.Now;

    public ICollection<UserAccessRightMapping> Users { get; set; } = [];
}
