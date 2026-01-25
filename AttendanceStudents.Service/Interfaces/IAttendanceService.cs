using System.Net;

namespace AttendanceStudents.Service.Interfaces;

public interface IAttendanceService
{
    bool MarkPresent(Guid sessionId, Guid studentId);
    int CountForSession(Guid sessionId);
    string CheckQrCodeandStudent(Guid sessionId, string code,string? userType,IPAddress? clientIp);
}