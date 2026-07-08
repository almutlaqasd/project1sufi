using IncidentRegistrationMvc.Business;
using IncidentRegistrationMvc.Models;
using Microsoft.AspNetCore.Mvc;

namespace IncidentRegistrationMvc.Controllers;

public class DashboardController : Controller
{
    private readonly IApplicationService _service;
    public DashboardController(IApplicationService service) => _service = service;

    public async Task<IActionResult> Index()
    {
        var data = await GetVisibleResponsesAsync();
        if (data == null)
            return RedirectToAction("Login", "Account");

        ViewBag.IsAdmin = IsAdmin();
        return View(data);
    }

    public async Task<IActionResult> Reports()
    {
        var data = await GetVisibleResponsesAsync();
        if (data == null)
            return RedirectToAction("Login", "Account");

        SetReportData(data);
        return View();
    }

    private void SetReportData(List<Models.FormResponse> data)
    {
        ViewBag.PriorityLabels = data.GroupBy(x => x.Priority).Select(g => g.Key).ToArray();
        ViewBag.PriorityCounts = data.GroupBy(x => x.Priority).Select(g => g.Count()).ToArray();
        ViewBag.StatusLabels = data.GroupBy(x => x.ResolvedStatus).Select(g => g.Key).ToArray();
        ViewBag.StatusCounts = data.GroupBy(x => x.ResolvedStatus).Select(g => g.Count()).ToArray();
        ViewBag.MachineLabels = data.GroupBy(x => x.EchoMachine ?? x.Device).OrderByDescending(g => g.Count()).Take(10).Select(g => g.Key).ToArray();
        ViewBag.MachineCounts = data.GroupBy(x => x.EchoMachine ?? x.Device).OrderByDescending(g => g.Count()).Take(10).Select(g => g.Count()).ToArray();
        ViewBag.DepartmentLabels = data.GroupBy(x => x.Device).OrderByDescending(g => g.Count()).Select(g => g.Key).ToArray();
        ViewBag.DepartmentCounts = data.GroupBy(x => x.Device).OrderByDescending(g => g.Count()).Select(g => g.Count()).ToArray();
        ViewBag.StaffLabels = data.GroupBy(x => x.ReportedByUser?.Username ?? x.ReporterName).OrderByDescending(g => g.Count()).Take(10).Select(g => g.Key).ToArray();
        ViewBag.StaffCounts = data.GroupBy(x => x.ReportedByUser?.Username ?? x.ReporterName).OrderByDescending(g => g.Count()).Take(10).Select(g => g.Count()).ToArray();
        DateTime monthStart = new(DateTime.Today.Year, DateTime.Today.Month, 1);
        var months = Enumerable.Range(0, 12).Select(i => monthStart.AddMonths(i - 11)).ToList();
        ViewBag.MonthLabels = months.Select(x => x.ToString("MMM yyyy")).ToArray();
        ViewBag.MonthCounts = months.Select(x => data.Count(r => r.CreatedOn.Year == x.Year && r.CreatedOn.Month == x.Month)).ToArray();
    }

    private async Task<List<Models.FormResponse>?> GetVisibleResponsesAsync()
    {
        if (HttpContext.Session.GetString("IsLoggedIn") != "true")
            return null;

        if (!await IsActiveLoggedInUserAsync())
        {
            HttpContext.Session.Clear();
            return null;
        }

        int? userId = HttpContext.Session.GetInt32("UserId");
        var query = _service.Query<FormResponse>();
        if (!IsAdmin())
            query = query.Where(x => x.ReportedBy == userId);

        return await query.OrderByDescending(x => x.Id).ToListAsync();
    }

    [HttpPost]
    public async Task<IActionResult> UpdateStatus(int id, string status)
    {
        if (HttpContext.Session.GetString("IsLoggedIn") != "true")
            return RedirectToAction("Login", "Account");

        if (!await IsActiveLoggedInUserAsync())
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }

        if (!IsAdmin())
            return RedirectToAction("Create", "Registration");

        var item = await _service.FindAsync<FormResponse>(id);
        if (item != null)
        {
            string oldStatus = item.ResolvedStatus;
            item.ResolvedStatus = status;
            item.ResolvedOn = string.Equals(status, "Resolved", StringComparison.OrdinalIgnoreCase)
                ? DateTime.Now
                : null;
            await _service.SaveChangesAsync();
        }
        return RedirectToAction("Index");
    }

    private bool IsAdmin()
    {
        return new[] { "Admin", "SuperAdmin" }.Contains(HttpContext.Session.GetString("UserType"), StringComparer.OrdinalIgnoreCase);
    }

    private async Task<bool> IsActiveLoggedInUserAsync()
    {
        int? userId = HttpContext.Session.GetInt32("UserId");
        return userId.HasValue && await _service.Query<User>().AnyAsync(x => x.UserId == userId && x.IsActive);
    }
}
