using AttendanceStudents.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;
using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;
using AttendanceStudents.Domain.Enums;

namespace AttendanceStudents.Web.Controllers;

public class SessionController : Controller
{
    private readonly ISessionService _sessionService;


    public SessionController(ISessionService sessionService)
    {
        _sessionService = sessionService;
    
    }

    private bool IsLoggedIn() => HttpContext.Session.GetString("UserId") != null;
    private bool IsProfessor() => HttpContext.Session.GetString("UserType") == "Professor";

    [HttpGet]
    public IActionResult CourseSessions(Guid courseId)
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
        if (!IsProfessor()) return RedirectToAction("Index", "Home");

        var sessions = _sessionService.GetByCourse(courseId);
        ViewBag.CourseId = courseId;
        return View(sessions);
    }

    [HttpGet]
    public IActionResult Details(Guid id)
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
        if (!IsProfessor()) return RedirectToAction("Index", "Home");

        var session = _sessionService.GetById(id);
        if (session == null) return NotFound();

        ViewBag.QrCode = null;

        return View(session);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Open(Guid id)
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
        if (!IsProfessor()) return RedirectToAction("Index", "Home");

        var code = _sessionService.Open(id);
        TempData["SessionCode"] = code;

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Close(Guid id)
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
        if (!IsProfessor()) return RedirectToAction("Index", "Home");

        _sessionService.Close(id);
        return RedirectToAction(nameof(Details), new { id });
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult PlusMinute(Guid id)
    {
        _sessionService.AdjustLoginWindow(id, +60);

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult MinusMinute(Guid id)
    {
        _sessionService.AdjustLoginWindow(id, -60);

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet]
    public IActionResult LiveInfo(Guid id)
    {
        if (!IsLoggedIn()) return Unauthorized();
        if (!IsProfessor()) return Forbid();
        
        var dto = _sessionService.GetLiveInfo(id); 
        return Json(dto);
    }

}