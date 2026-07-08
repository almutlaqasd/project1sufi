using System.ComponentModel.DataAnnotations;

namespace IncidentRegistrationMvc.Models;

public class User : IClientEntity
{
    public int UserId { get; set; }
    public int ClientId { get; set; }

    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    public string UserType { get; set; } = "User";

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public ICollection<UserAccessRightMapping> AccessRights { get; set; } = [];
}
