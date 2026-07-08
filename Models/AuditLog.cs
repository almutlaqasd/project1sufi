namespace IncidentRegistrationMvc.Models;

public class AuditLog : IClientEntity
{
    public int ClientId { get; set; }
    public long Id { get; set; }
    public int? ActorUserId { get; set; }
    public User? ActorUser { get; set; }
    public string ActorName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string? Changes { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.Now;
}
