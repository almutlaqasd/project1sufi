namespace IncidentRegistrationMvc.Services;

public interface IClientConfigurationService
{
    Task<ClientConfigurationSnapshot> GetClientConfigurationAsync();
}
