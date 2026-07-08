using IncidentRegistrationMvc.Business;
using IncidentRegistrationMvc.Models;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json;

namespace IncidentRegistrationMvc.Controllers;

public class RegistrationController : Controller
{
    private readonly IApplicationService _service;
    private readonly IWebHostEnvironment _environment;

    public RegistrationController(IApplicationService service, IWebHostEnvironment environment)
    {
        _service = service;
        _environment = environment;
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        if (HttpContext.Session.GetString("IsLoggedIn") != "true")
            return RedirectToAction("Login", "Account");

        if (!await IsActiveLoggedInUserAsync())
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }

        await LoadDropdownsAsync();
        return View(new FormResponse());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(FormResponse model)
    {
        if (HttpContext.Session.GetString("IsLoggedIn") != "true")
            return RedirectToAction("Login", "Account");

        if (!await IsActiveLoggedInUserAsync())
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }

        if (string.Equals(model.Device, "Other", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(model.OtherDevice))
            {
                ModelState.AddModelError(nameof(model.OtherDevice), "Please enter the device name.");
            }
            else
            {
                model.Device = model.OtherDevice.Trim();
            }
        }

        LocationMachineMaster? selectedMachine = await GetSelectedMachineAsync(model);
        if (!string.IsNullOrWhiteSpace(model.Location) && selectedMachine == null)
        {
            ModelState.AddModelError(nameof(model.EchoMachine), "Please select a valid machine for the selected location.");
        }
        else if (selectedMachine != null)
        {
            model.Location = selectedMachine.Location;
            model.EchoMachine = selectedMachine.EchoMachine;
            model.ClinicalEngNumber = string.IsNullOrWhiteSpace(model.ClinicalEngNumber)
                ? selectedMachine.ClinicalEngNumber
                : model.ClinicalEngNumber.Trim();
            model.SerialNumber = string.IsNullOrWhiteSpace(model.SerialNumber)
                ? selectedMachine.SerialNumber
                : model.SerialNumber.Trim();
            model.PropertyTagNumber = string.IsNullOrWhiteSpace(model.PropertyTagNumber)
                ? selectedMachine.PropertyTagNumber
                : model.PropertyTagNumber.Trim();
            model.AnnualPreventiveDueDate = selectedMachine.AnnualPreventiveDueDate;
        }

        if (!ModelState.IsValid)
        {
            await LoadDropdownsAsync();
            return View(model);
        }

        if (model.ImageFile != null && model.ImageFile.Length > 0)
        {
            string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadsFolder);

            string extension = Path.GetExtension(model.ImageFile.FileName);
            string fileName = $"incident_{DateTime.Now:yyyyMMddHHmmssfff}{extension}";
            string filePath = Path.Combine(uploadsFolder, fileName);

            using FileStream stream = new(filePath, FileMode.Create);
            await model.ImageFile.CopyToAsync(stream);
            model.ImagePath = "/uploads/" + fileName;
        }

        model.ReportedBy = HttpContext.Session.GetInt32("UserId");
        model.CreatedOn = DateTime.Now;
        model.ResolvedStatus = "pending";
        model.ResolvedOn = null;
        _service.Add(model);
        await _service.SaveChangesAsync();

        List<int> adminIds = await _service.Query<User>().Where(x => x.IsActive && (x.UserType == "Admin" || x.UserType == "SuperAdmin")).Select(x => x.UserId).ToListAsync();
        foreach (int adminId in adminIds)
        {
            _service.Add(new UserNotification
            {
                UserId = adminId,
                Title = $"New report #{model.ResponseId}",
                Message = $"{model.ReporterName} reported {model.EchoMachine ?? model.Device} ({model.Priority} priority).",
                Url = Url.Action("Index", "Dashboard", null, Request.Scheme)
            });
        }
        await _service.SaveChangesAsync();

        TempData["Success"] = "Your report has been submitted successfully.";
        return RedirectToAction("Create");
    }

    private async Task LoadDropdownsAsync()
    {
        List<DropdownMaster> dropdowns;
        try
        {
            dropdowns = await _service.Query<DropdownMaster>()
                .Where(x => x.IsActive)
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.Name)
                .ToListAsync();
        }
        catch (SqlException ex) when (ex.Number == 208)
        {
            dropdowns = GetDefaultDropdowns();
        }

        ViewBag.TypeOptions = ToSelectList(dropdowns, "Type");
        ViewBag.DeviceOptions = ToSelectList(dropdowns, "Device");
        ViewBag.PriorityOptions = ToSelectList(dropdowns, "Priority");
        await LoadLocationDropdownsAsync();
    }

    private async Task LoadLocationDropdownsAsync()
    {
        List<LocationMachineMaster> machines;
        try
        {
            IQueryable<LocationMachineMaster> query = _service.Query<LocationMachineMaster>()
                .Where(x => x.IsActive);

            if (!IsAdmin())
            {
                List<int> userAccessRightIds = await GetLoggedInUserAccessRightIdsAsync();
                query = query.Where(x => x.UserAccessRightId.HasValue && userAccessRightIds.Contains(x.UserAccessRightId.Value));
            }

            machines = await query
                    .OrderBy(x => x.Location)
                    .ThenBy(x => x.EchoMachine)
                    .ToListAsync();
        }
        catch (SqlException ex) when (ex.Number == 208 || ex.Number == 207)
        {
            machines = GetDefaultLocationMachines();
        }

        ViewBag.LocationOptions = machines
            .GroupBy(x => x.Location, StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x.Key)
            .Select(x =>
            {
                List<string> accessRightNames = x
                    .Select(machine => machine.UserAccessRight?.Name)
                    .Where(name => !string.IsNullOrWhiteSpace(name))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(name => name)
                    .ToList()!;

                string text = accessRightNames.Any()
                    ? $"{x.Key} ({string.Join(", ", accessRightNames)})"
                    : x.Key;

                return new SelectListItem { Text = text, Value = x.Key };
            })
            .ToList();

        ViewBag.LocationMachinesJson = JsonSerializer.Serialize(machines.Select(x => new
        {
            x.EchoMachine,
            x.ClinicalEngNumber,
            x.Location,
            x.SerialNumber,
            x.PropertyTagNumber,
            UserAccessRightName = x.UserAccessRight != null ? x.UserAccessRight.Name : null,
            AnnualPreventiveDueDate = x.AnnualPreventiveDueDate?.ToString("yyyy-MM-dd")
        }));
    }

    private async Task<LocationMachineMaster?> GetSelectedMachineAsync(FormResponse model)
    {
        if (string.IsNullOrWhiteSpace(model.Location) || string.IsNullOrWhiteSpace(model.EchoMachine))
        {
            return null;
        }

        try
        {
            IQueryable<LocationMachineMaster> query = _service.Query<LocationMachineMaster>()
                .Where(x => x.IsActive);

            if (!IsAdmin())
            {
                List<int> userAccessRightIds = await GetLoggedInUserAccessRightIdsAsync();
                query = query.Where(x => x.UserAccessRightId.HasValue && userAccessRightIds.Contains(x.UserAccessRightId.Value));
            }

            return await query
                .FirstOrDefaultAsync(x => x.Location == model.Location && x.EchoMachine == model.EchoMachine);
        }
        catch (SqlException ex) when (ex.Number == 208 || ex.Number == 207)
        {
            return GetDefaultLocationMachines()
                .FirstOrDefault(x =>
                    string.Equals(x.Location, model.Location, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(x.EchoMachine, model.EchoMachine, StringComparison.OrdinalIgnoreCase));
        }
    }

    private static List<DropdownMaster> GetDefaultDropdowns()
    {
        return
        [
            new() { Category = "Type", Name = "Equipment", SortOrder = 1 },
            new() { Category = "Type", Name = "Safety", SortOrder = 2 },
            new() { Category = "Device", Name = "Echo", SortOrder = 1 },
            new() { Category = "Device", Name = "TTE", SortOrder = 2 },
            new() { Category = "Device", Name = "ECG", SortOrder = 3 },
            new() { Category = "Device", Name = "Other", SortOrder = 4 },
            new() { Category = "Priority", Name = "Low", SortOrder = 1 },
            new() { Category = "Priority", Name = "Medium", SortOrder = 2 },
            new() { Category = "Priority", Name = "High", SortOrder = 3 }
        ];
    }

    private static List<LocationMachineMaster> GetDefaultLocationMachines()
    {
        return
        [
            new() { EchoMachine = "GE Vivid E95", ClinicalEngNumber = "117412", Location = "Room 40", SerialNumber = "AU02574", PropertyTagNumber = "M0066066" },
            new() { EchoMachine = "Philips Epiq 3", ClinicalEngNumber = "111763", Location = "Room 40", SerialNumber = "US014B0467", PropertyTagNumber = "M0045167" },
            new() { EchoMachine = "GE Vivid E95", ClinicalEngNumber = "117410", Location = "Room 43", SerialNumber = null, PropertyTagNumber = "M0066064" },
            new() { EchoMachine = "GE Vivid S70 - 17", ClinicalEngNumber = "19621", Location = "Room 40 (PO)", SerialNumber = null, PropertyTagNumber = "M0091628" },
            new() { EchoMachine = "GE Vivid S70- 24", ClinicalEngNumber = "19641", Location = "Room 41", SerialNumber = "120046S70N", PropertyTagNumber = "M0091831" }
        ];
    }

    private static List<SelectListItem> ToSelectList(List<DropdownMaster> dropdowns, string category)
    {
        return dropdowns
            .Where(x => string.Equals(x.Category, category, StringComparison.OrdinalIgnoreCase))
            .Select(x => new SelectListItem { Text = x.Name, Value = x.Name })
            .ToList();
    }

    private async Task<List<int>> GetLoggedInUserAccessRightIdsAsync()
    {
        int? userId = HttpContext.Session.GetInt32("UserId");
        if (!userId.HasValue)
        {
            return [];
        }

        try
        {
            return await _service.Query<UserAccessRightMapping>()
                .Where(x => x.UserId == userId.Value && x.UserAccessRight.IsActive)
                .Select(x => x.UserAccessRightId)
                .Distinct()
                .ToListAsync();
        }
        catch (SqlException ex) when (ex.Number == 208 || ex.Number == 207)
        {
            return [];
        }
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
