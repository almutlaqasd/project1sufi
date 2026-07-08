using System.ComponentModel.DataAnnotations;

namespace IncidentRegistrationMvc.Models;

public class ReportMessage : IClientEntity
{
    public int ClientId { get; set; }
    public long Id { get; set; }
    public int? FormResponseId { get; set; }
    public FormResponse? FormResponse { get; set; }
    public int SenderUserId { get; set; }
    public User SenderUser { get; set; } = null!;
    public int RecipientUserId { get; set; }
    public User RecipientUser { get; set; } = null!;
    [Required, StringLength(2000)] public string Message { get; set; } = string.Empty;
    public DateTime CreatedOn { get; set; } = DateTime.Now;
    public DateTime? ReadOn { get; set; }
}
