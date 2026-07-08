namespace IncidentRegistrationMvc.Models;

public class UserAccessRightMapping : IClientEntity
{
    public int ClientId { get; set; }
    public int UserId { get; set; }

    public int UserAccessRightId { get; set; }

    public User User { get; set; } = null!;

    public UserAccessRight UserAccessRight { get; set; } = null!;
}
