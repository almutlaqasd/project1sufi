namespace IncidentRegistrationMvc.Models;

/// <summary>Marks data that belongs to exactly one configured client.</summary>
public interface IClientEntity
{
    int ClientId { get; set; }
}
