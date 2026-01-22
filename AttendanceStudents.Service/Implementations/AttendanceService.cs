using AttendanceStudents.Domain.Entities;
using AttendanceStudents.Repository;
using AttendanceStudents.Service.Interfaces;

namespace AttendanceStudents.Service.Implementations;

public class AttendanceService : IAttendanceService
{
    private readonly IRepository<Attendance> _attendanceRepo;

    public AttendanceService(IRepository<Attendance> attendanceRepo)
    {
        _attendanceRepo = attendanceRepo;
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
}