using IncidentRegistrationMvc.Business;
using IncidentRegistrationMvc.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace IncidentRegistrationMvc.Controllers;

public class ActivityLogsController : Controller
{
    private readonly IApplicationService _service;

    public ActivityLogsController(IApplicationService service) => _service = service;

    public async Task<IActionResult> Users()
    {
        IActionResult? authResult = await EnsureAdminAsync();
        if (authResult != null)
            return authResult;

        List<UserActivityLog> logs;
        try
        {
            logs = await _service.Query<UserActivityLog>()
                .OrderByDescending(x => x.ActivityOn)
                .Take(300)
                .ToListAsync();
        }
        catch (SqlException ex) when (ex.Number == 208)
        {
            logs = [];
            ViewBag.Error = "User activity log table has not been created yet.";
        }

        return View(logs);
    }

    public async Task<IActionResult> AccessRights()
    {
        IActionResult? authResult = await EnsureAdminAsync();
        if (authResult != null)
            return authResult;

        List<UserAccessRightMappingLog> logs;
        try
        {
            logs = await _service.Query<UserAccessRightMappingLog>()
                .OrderByDescending(x => x.ActivityOn)
                .Take(300)
                .ToListAsync();
        }
        catch (SqlException ex) when (ex.Number == 208)
        {
            logs = [];
            ViewBag.Error = "Access-right activity log table has not been created yet.";
        }

        return View(logs);
    }

    public async Task<IActionResult> Audit()
    {
        IActionResult? authResult = await EnsureSuperAdminAsync();
        if (authResult != null) return authResult;
        return View(await _service.Query<AuditLog>().OrderByDescending(x => x.CreatedOn).Take(500).ToListAsync());
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

    private async Task<IActionResult?> EnsureSuperAdminAsync()
    {
        IActionResult? result = await EnsureAdminAsync();
        if (result != null) return result;
        return string.Equals(HttpContext.Session.GetString("UserType"), "SuperAdmin", StringComparison.OrdinalIgnoreCase) ? null : Forbid();
    }
}
