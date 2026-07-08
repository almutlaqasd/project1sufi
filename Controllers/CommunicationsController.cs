using IncidentRegistrationMvc.Business;
using IncidentRegistrationMvc.Models;
using Microsoft.AspNetCore.Mvc;

namespace IncidentRegistrationMvc.Controllers;

public class CommunicationsController : Controller
{
    private readonly IApplicationService _service;
    public CommunicationsController(IApplicationService service) => _service = service;

    public async Task<IActionResult> Index(int? reportId)
    {
        int? userId = await ActiveUserIdAsync();
        if (!userId.HasValue) return RedirectToAction("Login", "Account");
        bool admin = IsAdmin();
        var query = _service.Query<ReportMessage>()
            .Where(x => x.SenderUserId == userId || x.RecipientUserId == userId);
        if (reportId.HasValue) query = query.Where(x => x.FormResponseId == reportId);
        ViewBag.ReportId = reportId;
        ViewBag.Recipients = await _service.Query<User>().Where(x => x.IsActive && x.UserId != userId && (admin || x.UserType == "Admin" || x.UserType == "SuperAdmin")).OrderBy(x => x.Username).ToListAsync();
        var unread = await query.Where(x => x.RecipientUserId == userId && x.ReadOn == null).ToListAsync();
        unread.ForEach(x => x.ReadOn = DateTime.Now);
        await _service.SaveChangesAsync();
        return View(await query.OrderByDescending(x => x.CreatedOn).Take(200).ToListAsync());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Send(int recipientUserId, string message, int? reportId)
    {
        int? userId = await ActiveUserIdAsync();
        if (!userId.HasValue) return RedirectToAction("Login", "Account");
        if (string.IsNullOrWhiteSpace(message) || !await _service.Query<User>().AnyAsync(x => x.UserId == recipientUserId && x.IsActive))
            return RedirectToAction(nameof(Index), new { reportId });
        if (reportId.HasValue && !IsAdmin() && !await _service.Query<FormResponse>().AnyAsync(x => x.Id == reportId && x.ReportedBy == userId)) return Forbid();
        _service.Add(new ReportMessage { SenderUserId = userId.Value, RecipientUserId = recipientUserId, FormResponseId = reportId, Message = message.Trim() });
        _service.Add(new UserNotification { UserId = recipientUserId, Title = reportId.HasValue ? $"New reply on report #{reportId:0000}" : "New message", Message = message.Trim(), Url = Url.Action(nameof(Index), new { reportId }) });
        await _service.SaveChangesAsync();
        return RedirectToAction(nameof(Index), new { reportId });
    }

    [HttpGet]
    public async Task<IActionResult> Notifications()
    {
        int? userId = await ActiveUserIdAsync();
        if (!userId.HasValue) return Unauthorized();
        var items = await _service.Query<UserNotification>().Where(x => x.UserId == userId && x.ReadOn == null).OrderByDescending(x => x.CreatedOn).Take(10).Select(x => new { x.Id, x.Title, x.Message, x.Url, x.CreatedOn }).ToListAsync();
        return Json(items);
    }

    [HttpPost]
    public async Task<IActionResult> MarkNotificationsRead()
    {
        int? userId = await ActiveUserIdAsync();
        if (!userId.HasValue) return Unauthorized();
        await _service.MarkUnreadNotificationsReadAsync(userId.Value, DateTime.Now);
        return Ok();
    }

    private bool IsAdmin() => new[] { "Admin", "SuperAdmin" }.Contains(HttpContext.Session.GetString("UserType"), StringComparer.OrdinalIgnoreCase);
    private async Task<int?> ActiveUserIdAsync() { int? id = HttpContext.Session.GetInt32("UserId"); return id.HasValue && await _service.Query<User>().AnyAsync(x => x.UserId == id && x.IsActive) ? id : null; }
}
