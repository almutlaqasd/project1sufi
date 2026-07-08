using IncidentRegistrationMvc.Business;
using IncidentRegistrationMvc.Models;
using Microsoft.AspNetCore.Mvc;

namespace IncidentRegistrationMvc.Controllers;

public class ConfigureClientsController : Controller
{
    private readonly IApplicationService _service;

    public ConfigureClientsController(IApplicationService service) => _service = service;

    public async Task<IActionResult> Index()
    {
        IActionResult? authResult = await EnsureAdminAsync();
        if (authResult != null)
            return authResult;

        ConfigureClient? client = await _service.Query<ConfigureClient>()
            .OrderByDescending(x => x.IsActive)
            .ThenByDescending(x => x.UpdatedOn ?? x.CreatedOn)
            .FirstOrDefaultAsync();

        if (client == null)
        {
            client = new ConfigureClient
            {
                ProjectTitle = "Equipment Maintenance & Reporting System",
                ClientName = "Hospital CMMS",
                IsActive = true,
                CreatedOn = DateTime.Now
            };

            _service.Add(client);
            await _service.SaveChangesAsync();
        }

        return View(client);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(ConfigureClient model)
    {
        IActionResult? authResult = await EnsureAdminAsync();
        if (authResult != null)
            return authResult;

        Normalize(model);

        if (!ModelState.IsValid)
            return View(model);

        ConfigureClient? client = await _service.FindAsync<ConfigureClient>(model.Id);
        if (client == null)
        {
            client = new ConfigureClient { CreatedOn = DateTime.Now };
            _service.Add(client);
        }

        client.ProjectTitle = model.ProjectTitle;
        client.ClientName = model.ClientName;
        client.ContactNo = model.ContactNo;
        client.Email = model.Email;
        client.Website = model.Website;
        client.Address = model.Address;
        client.City = model.City;
        client.Country = model.Country;
        client.SupportContact = model.SupportContact;
        client.SupportEmail = model.SupportEmail;
        client.IsActive = model.IsActive;
        client.UpdatedOn = DateTime.Now;

        await _service.SaveChangesAsync();

        TempData["Success"] = "Client configuration saved successfully.";
        return RedirectToAction(nameof(Index));
    }

    private static void Normalize(ConfigureClient model)
    {
        model.ProjectTitle = model.ProjectTitle.Trim();
        model.ClientName = string.IsNullOrWhiteSpace(model.ClientName) ? null : model.ClientName.Trim();
        model.ContactNo = string.IsNullOrWhiteSpace(model.ContactNo) ? null : model.ContactNo.Trim();
        model.Email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim();
        model.Website = string.IsNullOrWhiteSpace(model.Website) ? null : model.Website.Trim();
        model.Address = string.IsNullOrWhiteSpace(model.Address) ? null : model.Address.Trim();
        model.City = string.IsNullOrWhiteSpace(model.City) ? null : model.City.Trim();
        model.Country = string.IsNullOrWhiteSpace(model.Country) ? null : model.Country.Trim();
        model.SupportContact = string.IsNullOrWhiteSpace(model.SupportContact) ? null : model.SupportContact.Trim();
        model.SupportEmail = string.IsNullOrWhiteSpace(model.SupportEmail) ? null : model.SupportEmail.Trim();
    }

    private async Task<IActionResult?> EnsureAdminAsync()
    {
        if (HttpContext.Session.GetString("IsLoggedIn") != "true")
            return RedirectToAction("Login", "Account");

        int? userId = HttpContext.Session.GetInt32("UserId");
        bool activeUser = userId.HasValue && await _service.Query<User>().AnyAsync(x => x.UserId == userId && x.IsActive);
        if (!activeUser)
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }

        if (!new[] { "Admin", "SuperAdmin" }.Contains(HttpContext.Session.GetString("UserType"), StringComparer.OrdinalIgnoreCase))
            return RedirectToAction("Create", "Registration");

        return null;
    }
}
