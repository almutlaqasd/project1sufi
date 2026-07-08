using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IncidentRegistrationMvc.Models;

public class UserRegistrationViewModel
{
    [Required]
    [StringLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string UserType { get; set; } = "User";

    [Display(Name = "Access Rights")]
    public List<int> SelectedAccessRightIds { get; set; } = [];

    public List<SelectListItem> AccessRightOptions { get; set; } = [];
}
