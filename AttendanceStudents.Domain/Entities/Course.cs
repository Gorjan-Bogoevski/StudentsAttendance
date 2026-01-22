using AttendanceStudents.Domain.Common;
using AttendanceStudents.Domain.Enums;

namespace AttendanceStudents.Domain.Entities;

public class Course : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public SemesterType Semester { get; set; }

    public WeekDay? DayOfWeek { get; set; }

    public TimeOnly ?StartTime { get; set; }
    public TimeOnly ?EndTime { get; set; }
    
    public int WeeksCount { get; set; } = 12;

    public ICollection<ProfessorCourse> ProfessorCourses { get; set; }
        = new List<ProfessorCourse>();
    
    public ICollection<Session> Sessions { get; set; } = new List<Session>();
}