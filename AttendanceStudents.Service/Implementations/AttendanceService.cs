using System.Net;
using System.Security.Cryptography;
using System.Text;
using AttendanceStudents.Domain.Entities;
using AttendanceStudents.Domain.Enums;
using AttendanceStudents.Repository;
using AttendanceStudents.Service.Interfaces;

namespace AttendanceStudents.Service.Implementations;

public class AttendanceService : IAttendanceService
{
    private readonly IRepository<Attendance> _attendanceRepo;
    private readonly ISessionService _sessionService;


    public AttendanceService(IRepository<Attendance> attendanceRepo, ISessionService sessionService)
    {
        _attendanceRepo = attendanceRepo;
        _sessionService = sessionService;
    }

    public bool MarkPresent(Guid sessionId, Guid studentId)
    {
    
        var exists = _attendanceRepo.Get(a =>
            a.SessionId == sessionId && a.StudentId == studentId);

        if (exists != null)
            return true; 

        var attendance = new Attendance
        {
            SessionId = sessionId,
            StudentId = studentId
        };

        _attendanceRepo.Insert(attendance);
        return true;
    }

    public int CountForSession(Guid sessionId)
    {
        return _attendanceRepo
            .GetAll(a => a.SessionId == sessionId)
            .Count();
    }

    public string CheckQrCodeandStudent(Guid sessionId, string code, string? userType, IPAddress? clientIp)
    {   
        var codeHash = Sha256(code);
        
        var session = _sessionService.GetById(sessionId);
        if (userType != "Student")
            return "Само студент може да пријави присуство.";
        if (clientIp == null || !IsAllowedForHomeTest(clientIp))
            return "Пријавата е дозволена само од локалната мрежа";
        if (session == null)
            return "Сесијата не постои.";

        if (session.Status != SessionStatus.Open)
            return "Сесијата не е активна.";
    
        if (!string.Equals(codeHash, session.AccessCodeHash, StringComparison.OrdinalIgnoreCase))
            return"Невалиден QR код. Скенирај повторно.";
        
        return "";
    }
    private static bool IsAllowedForHomeTest(IPAddress ip)
    {
        // дозволи localhost (за тест на истата машина)
        if (IPAddress.IsLoopback(ip)) return true;

        // дозволи типични локални range-ови:
        // 10.0.0.0/8
        // 172.16.0.0/12
        // 192.168.0.0/16
        // return IsInCidr(ip, "10.0.0.0/8")
        //        || IsInCidr(ip, "172.16.0.0/12")
        //        || IsInCidr(ip, "192.168.0.0/16");
        return IsInCidr(ip, "192.168.1.0/25");
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

        for (int i = 0; i < fullBytes; i++)
            if (ipBytes[i] != baseBytes[i]) return false;

        if (remainingBits == 0) return true;

        int mask = (byte)~(0xFF >> remainingBits);
        return (ipBytes[fullBytes] & mask) == (baseBytes[fullBytes] & mask);
    }
    private static string Sha256(string input)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
        var sb = new StringBuilder(bytes.Length * 2);
        foreach (var b in bytes) sb.Append(b.ToString("x2"));
        return sb.ToString();
    }
}