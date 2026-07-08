using IncidentRegistrationMvc.Business;
using IncidentRegistrationMvc.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IncidentRegistrationMvc.Controllers;

public class LocationMachineMastersController : Controller
{
    private readonly IApplicationService _service;

    public LocationMachineMastersController(IApplicationService service) => _service = service;

    public async Task<IActionResult> Index()
    {
        IActionResult? authResult = await EnsureAdminAsync();
        if (authResult != null)
            return authResult;

        List<LocationMachineMaster> machines = await _service.Query<LocationMachineMaster>()
            .OrderByDescending(x => x.IsActive)
            .ThenBy(x => x.Location)
            .ThenBy(x => x.EchoMachine)
            .ToListAsync();

        return View(machines);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        IActionResult? authResult = await EnsureAdminAsync();
        if (authResult != null)
            return authResult;

        await LoadTypeOptionsAsync();
        return View(new LocationMachineMaster());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LocationMachineMaster model)
    {
        IActionResult? authResult = await EnsureAdminAsync();
        if (authResult != null)
            return authResult;

        Normalize(model);
        await ValidateTypeAsync(model.UserAccessRightId);

        if (!ModelState.IsValid)
        {
            await LoadTypeOptionsAsync();
            return View(model);
        }

        _service.Add(model);
        await _service.SaveChangesAsync();

        TempData["Success"] = "Machine registered successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        IActionResult? authResult = await EnsureAdminAsync();
        if (authResult != null)
            return authResult;

        LocationMachineMaster? machine = await _service.FindAsync<LocationMachineMaster>(id);
        if (machine == null)
            return NotFound();

        await LoadTypeOptionsAsync(machine.UserAccessRightId);
        return View(machine);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(LocationMachineMaster model)
    {
        IActionResult? authResult = await EnsureAdminAsync();
        if (authResult != null)
            return authResult;

        Normalize(model);
        await ValidateTypeAsync(model.UserAccessRightId);

        if (!ModelState.IsValid)
        {
            await LoadTypeOptionsAsync(model.UserAccessRightId);
            return View(model);
        }

        LocationMachineMaster? machine = await _service.FindAsync<LocationMachineMaster>(model.Id);
        if (machine == null)
            return NotFound();

        machine.EchoMachine = model.EchoMachine;
        machine.ClinicalEngNumber = model.ClinicalEngNumber;
        machine.Location = model.Location;
        machine.SerialNumber = model.SerialNumber;
        machine.PropertyTagNumber = model.PropertyTagNumber;
        machine.DateAcquired = model.DateAcquired;
        machine.Problems = model.Problems;
        machine.DateReported = model.DateReported;
        machine.ActionTaken = model.ActionTaken;
        machine.FinalActionRecommendation = model.FinalActionRecommendation;
        machine.WeeklyFilterCleaningDone = model.WeeklyFilterCleaningDone;
        machine.AnnualDueDate = model.AnnualDueDate;
        machine.UserAccessRightId = model.UserAccessRightId;
        machine.AnnualPreventiveDueDate = model.AnnualPreventiveDueDate;
        machine.IsActive = model.IsActive;

        await _service.SaveChangesAsync();

        TempData["Success"] = "Machine updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        IActionResult? authResult = await EnsureAdminAsync();
        if (authResult != null)
            return authResult;

        LocationMachineMaster? machine = await _service.FindAsync<LocationMachineMaster>(id);
        if (machine == null)
            return NotFound();

        machine.IsActive = false;
        await _service.SaveChangesAsync();

        TempData["Success"] = "Machine deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    private static void Normalize(LocationMachineMaster model)
    {
        model.EchoMachine = model.EchoMachine?.Trim() ?? string.Empty;
        model.ClinicalEngNumber = model.ClinicalEngNumber?.Trim() ?? string.Empty;
        model.Location = model.Location?.Trim() ?? string.Empty;
        model.SerialNumber = string.IsNullOrWhiteSpace(model.SerialNumber) ? null : model.SerialNumber.Trim();
        model.PropertyTagNumber = model.PropertyTagNumber?.Trim() ?? string.Empty;
        model.DateAcquired = string.IsNullOrWhiteSpace(model.DateAcquired) ? null : model.DateAcquired.Trim();
        model.Problems = string.IsNullOrWhiteSpace(model.Problems) ? null : model.Problems.Trim();
        model.DateReported = string.IsNullOrWhiteSpace(model.DateReported) ? null : model.DateReported.Trim();
        model.ActionTaken = string.IsNullOrWhiteSpace(model.ActionTaken) ? null : model.ActionTaken.Trim();
        model.FinalActionRecommendation = string.IsNullOrWhiteSpace(model.FinalActionRecommendation) ? null : model.FinalActionRecommendation.Trim();
        model.WeeklyFilterCleaningDone = string.IsNullOrWhiteSpace(model.WeeklyFilterCleaningDone) ? null : model.WeeklyFilterCleaningDone.Trim();
        model.AnnualDueDate = string.IsNullOrWhiteSpace(model.AnnualDueDate) ? null : model.AnnualDueDate.Trim();
    }

    private async Task LoadTypeOptionsAsync(int? selectedId = null)
    {
        ViewBag.TypeOptions = await _service.Query<UserAccessRight>()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Id.ToString(),
                Selected = selectedId.HasValue && x.Id == selectedId.Value
            })
            .ToListAsync();
    }

    private async Task ValidateTypeAsync(int? userAccessRightId)
    {
        if (!userAccessRightId.HasValue)
            return;

        bool validType = await _service.Query<UserAccessRight>()
            .AnyAsync(x => x.Id == userAccessRightId.Value && x.IsActive);

        if (!validType)
            ModelState.AddModelError(nameof(LocationMachineMaster.UserAccessRightId), "Please select a valid active type.");
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
