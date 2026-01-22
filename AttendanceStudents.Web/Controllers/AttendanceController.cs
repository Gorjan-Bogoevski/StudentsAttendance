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
        // 1) Ако не е логнат → Login со returnUrl (важно: да ги пренесе И sessionId И code)
        var userIdStr = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrWhiteSpace(userIdStr))
        {
            var returnUrl = Url.Action("Join", "Attendance", new { sessionId, code });
            return RedirectToAction("Login", "Account", new { returnUrl });
        }

        // 2) Само студент
        var userType = HttpContext.Session.GetString("UserType");
        if (userType != "Student")
            return View("JoinError", "Само студент може да пријави присуство.");

        var clientIp = GetClientIp(HttpContext);
        if (clientIp == null || !IsAllowedForHomeTest(clientIp))
            return View("JoinError", "Пријавата е дозволена само од локалната мрежа (тест). Поврзи се на Wi-Fi и скенирај повторно.");

        // 3) Провери сесија
        var session = _sessionService.GetById(sessionId);
        if (session == null)
            return View("JoinError", "Сесијата не постои.");

        if (session.Status != SessionStatus.Open)
            return View("JoinError", "Сесијата не е активна.");

        // 4) Провери дали QR прозорецот важи (CodeValidUntil)
        if (!session.CodeValidUntil.HasValue || DateTime.UtcNow > session.CodeValidUntil.Value)
            return View("JoinError", "QR кодот е истечен. Скенирај повторно.");

        // 5) Провери дали кодот од QR е точен (hash match)
        if (string.IsNullOrWhiteSpace(code))
            return View("JoinError", "Недостасува код. Скенирај повторно.");

        if (string.IsNullOrWhiteSpace(session.AccessCodeHash))
            return View("JoinError", "Кодот не е активен. Скенирај повторно.");

        var codeHash = Sha256(code);
        if (!string.Equals(codeHash, session.AccessCodeHash, StringComparison.OrdinalIgnoreCase))
            return View("JoinError", "Невалиден QR код. Скенирај повторно.");

        // 6) Mark present
        var studentId = Guid.Parse(userIdStr);
        _attendanceService.MarkPresent(sessionId, studentId);

        return View("JoinSuccess", session);
    }

    private static string Sha256(string input)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
        var sb = new StringBuilder(bytes.Length * 2);
        foreach (var b in bytes) sb.Append(b.ToString("x2"));
        return sb.ToString();
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

    private static bool IsAllowedForHomeTest(IPAddress ip)
    {
        // дозволи localhost (за тест на истата машина)
        if (IPAddress.IsLoopback(ip)) return true;

        // дозволи типични локални range-ови:
        // 10.0.0.0/8
        // 172.16.0.0/12
        // 192.168.0.0/16
        return IsInCidr(ip, "10.0.0.0/8")
               || IsInCidr(ip, "172.16.0.0/12")
               || IsInCidr(ip, "192.168.0.0/16");
    }

    private static bool IsInCidr(IPAddress ip, string cidr)
    {
        var parts = cidr.Split('/');
        var baseIp = IPAddress.Parse(parts[0]);
        var prefix = int.Parse(parts[1]);

        if (ip.AddressFamily != baseIp.AddressFamily) return false;

        var ipBytes = ip.GetAddressBytes();
        var baseBytes = baseIp.GetAddressBytes();

        int fullBytes = prefix / 8;
        int remainingBits = prefix % 8;

        // целосни бајти
        for (int i = 0; i < fullBytes; i++)
            if (ipBytes[i] != baseBytes[i]) return false;

        if (remainingBits == 0) return true;

        int mask = (byte)~(0xFF >> remainingBits);
        return (ipBytes[fullBytes] & mask) == (baseBytes[fullBytes] & mask);
    }
}