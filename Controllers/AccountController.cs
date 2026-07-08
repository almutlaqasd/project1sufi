using IncidentRegistrationMvc.Business;
using IncidentRegistrationMvc.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace IncidentRegistrationMvc.Controllers;

public class AccountController : Controller
{
    private readonly IApplicationService _service;
    private readonly PasswordHasher<User> _passwordHasher = new();

    public AccountController(IApplicationService service) => _service = service;

    [HttpGet]
    public IActionResult Login() => View(new LoginViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        string login = model.Username.Trim();
        User? user = await _service.FindUserForLoginAsync<User>(login);

        if (user != null && VerifyPassword(user, model.Password))
        {
            HttpContext.Session.SetString("IsLoggedIn", "true");
            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("UserType", user.UserType);
            HttpContext.Session.SetInt32("ClientId", user.ClientId);

            return RedirectToAction("Index", "Dashboard");
        }

        ViewBag.Error = "Invalid username or password.";
        return View(model);
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }

    private bool VerifyPassword(User user, string password)
    {
        if (string.Equals(user.PasswordHash, password, StringComparison.Ordinal))
        {
            return true;
        }

        if (IsSha256HashMatch(user.PasswordHash, password))
        {
            return true;
        }

        if (!LooksLikeIdentityPasswordHash(user.PasswordHash))
        {
            return false;
        }

        PasswordVerificationResult result;
        try
        {
            result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
        }
        catch (FormatException)
        {
            result = PasswordVerificationResult.Failed;
        }

        if (result == PasswordVerificationResult.Success || result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            return true;
        }

        return false;
    }

    private static bool LooksLikeIdentityPasswordHash(string storedHash)
    {
        return storedHash.StartsWith("AQAAAA", StringComparison.Ordinal);
    }

    private static bool IsAdmin(string userType)
    {
        return string.Equals(userType, "Admin", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsSha256HashMatch(string storedHash, string password)
    {
        if (storedHash.Length != 64 || !storedHash.All(Uri.IsHexDigit))
        {
            return false;
        }

        byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
        byte[] hashBytes = SHA256.HashData(passwordBytes);
        string computedHash = Convert.ToHexString(hashBytes);
        return string.Equals(storedHash, computedHash, StringComparison.OrdinalIgnoreCase);
    }
}
