using AttendanceStudents.Domain.Common;
using AttendanceStudents.Domain.Enums;

namespace AttendanceStudents.Domain.Entities;

public class Session : BaseEntity
{
    public Guid CourseId { get; set; }

    public Course? Course { get; set; }

    public int WeekNumber { get; set; }

    public SessionStatus Status { get; set; } = SessionStatus.Closed;

    public DateTime? OpenedAt { get; set; }

    public int LoginWindowSeconds { get; set; } = 300;

    public DateTime? ClosedAt { get; set; }
    
    public string? AccessCodeHash { get; set; }    
    public DateTime? CodeIssuedAt { get; set; }
    public DateTime? CodeValidUntil { get; set; }
    public int TimesOpened { get; set; } = 0;  
    
    public DateOnly? SessionDate { get; set; }
}