using AttendanceStudents.Domain.DTOs;
using AttendanceStudents.Domain.Entities;

namespace AttendanceStudents.Service.Interfaces;

public interface ISessionService
{
    List<Session> GetByCourse(Guid courseId);
    Session? GetById(Guid id);

    void Create(Session session);
    void Update(Session session);
    void Delete(Guid id);

    string Open(Guid sessionId);

    void Close(Guid sessionId);

    bool VerifyAccessCode(Guid sessionId, string code);
    bool RegisterAttendance(Guid sessionId, Guid studentId, string code);
    
    SessionLiveDto GetLiveInfo(Guid sessionId);
    public void AdjustLoginWindow(Guid sessionId, int deltaSeconds);


}