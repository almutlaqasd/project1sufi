namespace IncidentRegistrationMvc.Models;

public class UserActivityLog : IClientEntity
{
    public int ClientId { get; set; }
    public int Id { get; set; }

    public int UserId { get; set; }

    public string ActivityType { get; set; } = string.Empty;

    public string? OldUsername { get; set; }

    public string? NewUsername { get; set; }

    public string? OldEmail { get; set; }

    public string? NewEmail { get; set; }

    public string? OldUserType { get; set; }

    public string? NewUserType { get; set; }

    public bool? OldIsActive { get; set; }

    public bool? NewIsActive { get; set; }

    public string? ActivityBy { get; set; }

    public string? HostName { get; set; }

    public string? ApplicationName { get; set; }

    public DateTime ActivityOn { get; set; }
}
