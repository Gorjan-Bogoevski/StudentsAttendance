using AttendanceStudents.Domain.DTOs;
using AttendanceStudents.Domain.Entities;

namespace AttendanceStudents.Service.Interfaces;

public interface ISessionService
{
    List<Session> GetByCourse(Guid courseId);
    Session? GetById(Guid id);

    string Open(Guid sessionId);

    void Close(Guid sessionId);
    
    SessionLiveDto GetLiveInfo(Guid sessionId);
    public void AdjustLoginWindow(Guid sessionId, int deltaSeconds);


}