using IncidentRegistrationMvc.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;

namespace IncidentRegistrationMvc.Services;

public class ClientConfigurationService : IClientConfigurationService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ClientConfigurationService(ApplicationDbContext context, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<ClientConfigurationSnapshot> GetClientConfigurationAsync()
    {
        ClientConfigurationSnapshot fallback = new()
        {
            ProjectTitle = _configuration["Application:Title"] ?? "Equipment Maintenance & Reporting System"
        };

        try
        {
            int? clientId = _httpContextAccessor.HttpContext?.Session.GetInt32("ClientId");
            if (!clientId.HasValue)
                return fallback;

            var client = await _context.ConfigureClients
                .Where(x => x.Id == clientId.Value && x.IsActive)
                .OrderByDescending(x => x.UpdatedOn ?? x.CreatedOn)
                .FirstOrDefaultAsync();

            if (client == null)
                return fallback;

            return new ClientConfigurationSnapshot
            {
                ProjectTitle = string.IsNullOrWhiteSpace(client.ProjectTitle) ? fallback.ProjectTitle : client.ProjectTitle,
                ClientName = client.ClientName,
                ContactNo = client.ContactNo,
                Email = client.Email,
                Website = client.Website,
                Address = client.Address,
                City = client.City,
                Country = client.Country,
                SupportContact = client.SupportContact,
                SupportEmail = client.SupportEmail
            };
        }
        catch (SqlException ex) when (ex.Number == 208 || ex.Number == 207)
        {
            return fallback;
        }
    }
}
