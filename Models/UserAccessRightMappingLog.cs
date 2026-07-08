namespace IncidentRegistrationMvc.Models;

public class UserAccessRightMappingLog : IClientEntity
{
    public int ClientId { get; set; }
    public int Id { get; set; }

    public int UserId { get; set; }

    public int UserAccessRightId { get; set; }

    public string? AccessRightName { get; set; }

    public string ActivityType { get; set; } = string.Empty;

    public string? ActivityBy { get; set; }

    public string? HostName { get; set; }

    public string? ApplicationName { get; set; }

    public DateTime ActivityOn { get; set; }
}
