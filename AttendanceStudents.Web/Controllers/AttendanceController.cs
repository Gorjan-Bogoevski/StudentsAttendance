using System.Net;
using System.Security.Cryptography;
using System.Text;
using AttendanceStudents.Domain.Enums;
using AttendanceStudents.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AttendanceStudents.Web.Controllers;

public class AttendanceController : Controller
{
    private readonly ISessionService _sessionService;
    private readonly IAttendanceService _attendanceService;

    public AttendanceController(ISessionService sessionService, IAttendanceService attendanceService)
    {
        _sessionService = sessionService;
        _attendanceService = attendanceService;
    }

    [HttpGet]
    public IActionResult Join(Guid sessionId, string code)
    {
        var userIdStr = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrWhiteSpace(userIdStr))
        {
            var returnUrl = Url.Action("Join", "Attendance", new { sessionId, code });
            return RedirectToAction("Login", "Account", new { returnUrl });
        }

        var userType = HttpContext.Session.GetString("UserType");
        var clientIp = GetClientIp(HttpContext);
        var session = _sessionService.GetById(sessionId);

        var msg = _attendanceService.CheckQrCodeandStudent(sessionId, code, userType, clientIp);
        if (msg !="")
        {
            return View("JoinError", msg);
        }

        var studentId = Guid.Parse(userIdStr);
        _attendanceService.MarkPresent(sessionId, studentId);

        return View("JoinSuccess", session);
    }
    
    [HttpGet]
    public IActionResult AttendanceCount(Guid sessionId)
    {
        var count = _attendanceService.CountForSession(sessionId);
        return Json(new { count });
    }
    
    private static IPAddress? GetClientIp(HttpContext ctx)
    {
        // ако има reverse proxy / IIS, ќе го решиме со ForwardedHeaders (подолу)
        var ip = ctx.Connection.RemoteIpAddress;

        // Ако е IPv6 mapped (на пр. ::ffff:192.168.1.10) -> нормализирај
        if (ip != null && ip.IsIPv4MappedToIPv6)
            ip = ip.MapToIPv4();

        return ip;
    }

   
}