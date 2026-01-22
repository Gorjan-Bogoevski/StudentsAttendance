using AttendanceStudents.Domain.Common;

namespace AttendanceStudents.Domain.Entities;

public class Attendance : BaseEntity
{
    public Guid SessionId { get; set; }
    public Session? Session { get; set; }

    public Guid StudentId { get; set; }
    public Student? Student { get; set; }

    public DateTime AttendedAt { get; set; } = DateTime.UtcNow;

 
}