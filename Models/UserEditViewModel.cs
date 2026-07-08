using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IncidentRegistrationMvc.Models;

public class UserEditViewModel
{
    public int UserId { get; set; }

    [Required]
    [StringLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "New Password")]
    public string? NewPassword { get; set; }

    [Required]
    [StringLength(20)]
    public string UserType { get; set; } = "User";

    public bool IsActive { get; set; } = true;

    [Display(Name = "Access Rights")]
    public List<int> SelectedAccessRightIds { get; set; } = [];

    public List<SelectListItem> AccessRightOptions { get; set; } = [];
}
