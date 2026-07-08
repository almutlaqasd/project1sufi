using IncidentRegistrationMvc.Business;
using IncidentRegistrationMvc.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IncidentRegistrationMvc.Controllers;

public class UsersController : Controller
{
    private readonly IApplicationService _service;
    private readonly PasswordHasher<User> _passwordHasher = new();

    public UsersController(IApplicationService service) => _service = service;

    public async Task<IActionResult> Index()
    {
        IActionResult? authResult = await EnsureAdminAsync();
        if (authResult != null)
            return authResult;

        bool superAdmin = string.Equals(HttpContext.Session.GetString("UserType"), "SuperAdmin", StringComparison.OrdinalIgnoreCase);
        List<User> users = await _service.Query<User>()
            .Where(x => x.UserType == "User" || (superAdmin && x.UserType == "Admin"))
            .OrderByDescending(x => x.IsActive)
            .ThenBy(x => x.Username)
            .ToListAsync();

        return View(users);
    }

    [HttpGet]
    public async Task<IActionResult> Register()
    {
        IActionResult? authResult = await EnsureAdminAsync();
        if (authResult != null)
            return authResult;

        return View(await BuildRegistrationModelAsync(new UserRegistrationViewModel()));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(UserRegistrationViewModel model)
    {
        IActionResult? authResult = await EnsureAdminAsync();
        if (authResult != null)
            return authResult;

        model.Username = model.Username.Trim();
        model.Email = model.Email.Trim();
        ValidateRoleAssignment(model.UserType);

        if (await _service.Query<User>().AnyAsync(x => x.Username == model.Username))
        {
            ModelState.AddModelError(nameof(model.Username), "Username already exists.");
        }

        if (await _service.Query<User>().AnyAsync(x => x.Email == model.Email))
        {
            ModelState.AddModelError(nameof(model.Email), "Email already exists.");
        }

        await ValidateAccessRightsAsync(model.SelectedAccessRightIds);

        if (!ModelState.IsValid)
        {
            return View(await BuildRegistrationModelAsync(model));
        }

        User user = new()
        {
            Username = model.Username,
            Email = model.Email,
            UserType = model.UserType,
            IsActive = true,
            CreatedAt = DateTime.Now
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, model.Password);

        AddAccessRights(user, model.SelectedAccessRightIds);

        _service.Add(user);
        await _service.SaveChangesAsync();

        TempData["Success"] = "User registered successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        IActionResult? authResult = await EnsureAdminAsync();
        if (authResult != null)
            return authResult;

        User? user = await _service.Query<User>()
            .FirstOrDefaultAsync(x => x.UserId == id);

        if (user == null)
            return NotFound();
        if (!CanManage(user)) return Forbid();

        UserEditViewModel model = new()
        {
            UserId = user.UserId,
            Username = user.Username,
            Email = user.Email,
            UserType = user.UserType,
            IsActive = user.IsActive,
            SelectedAccessRightIds = user.AccessRights.Select(x => x.UserAccessRightId).ToList()
        };

        return View(await BuildEditModelAsync(model));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UserEditViewModel model)
    {
        IActionResult? authResult = await EnsureAdminAsync();
        if (authResult != null)
            return authResult;

        model.Username = model.Username.Trim();
        model.Email = model.Email.Trim();
        ValidateRoleAssignment(model.UserType);

        User? user = await _service.Query<User>()
            .FirstOrDefaultAsync(x => x.UserId == model.UserId);

        if (user == null)
            return NotFound();
        if (!CanManage(user)) return Forbid();

        if (await _service.Query<User>().AnyAsync(x => x.UserId != model.UserId && x.Username == model.Username))
        {
            ModelState.AddModelError(nameof(model.Username), "Username already exists.");
        }

        if (await _service.Query<User>().AnyAsync(x => x.UserId != model.UserId && x.Email == model.Email))
        {
            ModelState.AddModelError(nameof(model.Email), "Email already exists.");
        }

        await ValidateAccessRightsAsync(model.SelectedAccessRightIds);

        if (!ModelState.IsValid)
        {
            return View(await BuildEditModelAsync(model));
        }

        user.Username = model.Username;
        user.Email = model.Email;
        user.UserType = model.UserType;
        user.IsActive = model.IsActive;

        if (!string.IsNullOrWhiteSpace(model.NewPassword))
        {
            user.PasswordHash = _passwordHasher.HashPassword(user, model.NewPassword);
        }

        user.AccessRights.Clear();
        AddAccessRights(user, model.SelectedAccessRightIds);

        await _service.SaveChangesAsync();

        TempData["Success"] = "User updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        IActionResult? authResult = await EnsureAdminAsync();
        if (authResult != null)
            return authResult;

        int? currentUserId = HttpContext.Session.GetInt32("UserId");
        if (currentUserId == id)
        {
            TempData["Error"] = "You cannot delete your own logged-in account.";
            return RedirectToAction(nameof(Index));
        }

        User? user = await _service.FindAsync<User>(id);
        if (user == null)
            return NotFound();
        if (!CanManage(user)) return Forbid();

        user.IsActive = false;
        await _service.SaveChangesAsync();

        TempData["Success"] = "User deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<UserRegistrationViewModel> BuildRegistrationModelAsync(UserRegistrationViewModel model)
    {
        model.AccessRightOptions = await BuildAccessRightOptionsAsync(model.SelectedAccessRightIds);
        return model;
    }

    private async Task<UserEditViewModel> BuildEditModelAsync(UserEditViewModel model)
    {
        model.AccessRightOptions = await BuildAccessRightOptionsAsync(model.SelectedAccessRightIds);
        return model;
    }

    private async Task<List<SelectListItem>> BuildAccessRightOptionsAsync(List<int> selectedIds)
    {
        return await _service.Query<UserAccessRight>()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Id.ToString(),
                Selected = selectedIds.Contains(x.Id)
            })
            .ToListAsync();
    }

    private async Task ValidateAccessRightsAsync(List<int> selectedAccessRightIds)
    {
        List<int> activeAccessRightIds = await _service.Query<UserAccessRight>()
            .Where(x => x.IsActive)
            .Select(x => x.Id)
            .ToListAsync();

        if (selectedAccessRightIds.Except(activeAccessRightIds).Any())
        {
            ModelState.AddModelError(nameof(UserRegistrationViewModel.SelectedAccessRightIds), "Please select valid access rights.");
        }
    }

    private static void AddAccessRights(User user, List<int> selectedAccessRightIds)
    {
        foreach (int accessRightId in selectedAccessRightIds.Distinct())
        {
            user.AccessRights.Add(new UserAccessRightMapping
            {
                User = user,
                UserAccessRightId = accessRightId
            });
        }
    }

    private async Task<IActionResult?> EnsureAdminAsync()
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

        return null;
    }

    private async Task<bool> IsActiveLoggedInUserAsync()
    {
        int? userId = HttpContext.Session.GetInt32("UserId");
        return userId.HasValue && await _service.Query<User>().AnyAsync(x => x.UserId == userId && x.IsActive);
    }

    private bool IsAdmin()
    {
        return new[] { "Admin", "SuperAdmin" }.Contains(HttpContext.Session.GetString("UserType"), StringComparer.OrdinalIgnoreCase);
    }

    private void ValidateRoleAssignment(string role)
    {
        string[] valid = ["User", "Admin"];
        if (!valid.Contains(role)) ModelState.AddModelError("UserType", "Invalid role.");
        if (!string.Equals(HttpContext.Session.GetString("UserType"), "SuperAdmin", StringComparison.OrdinalIgnoreCase) && !string.Equals(role, "User", StringComparison.OrdinalIgnoreCase))
            ModelState.AddModelError("UserType", "Only a Super Admin can assign administrator roles.");
    }

    private bool CanManage(User user) =>
        !string.Equals(user.UserType, "SuperAdmin", StringComparison.OrdinalIgnoreCase) &&
        (string.Equals(HttpContext.Session.GetString("UserType"), "SuperAdmin", StringComparison.OrdinalIgnoreCase) ||
         string.Equals(user.UserType, "User", StringComparison.OrdinalIgnoreCase));
}
