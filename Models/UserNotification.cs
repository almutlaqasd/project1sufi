namespace IncidentRegistrationMvc.Models;

public class UserNotification : IClientEntity
{
    public int ClientId { get; set; }
    public long Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Url { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.Now;
    public DateTime? ReadOn { get; set; }
}
