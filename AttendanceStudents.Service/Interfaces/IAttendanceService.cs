namespace AttendanceStudents.Service.Interfaces;

public interface IAttendanceService
{
    bool MarkPresent(Guid sessionId, Guid studentId);
    int CountForSession(Guid sessionId);
}