using AttendanceStudents.Domain.Common;

namespace AttendanceStudents.Domain.Entities;

public class ProfessorCourse : BaseEntity
{
    public Guid ProfessorId { get; set; }
    public Professor Professor { get; set; } = default!;

    public Guid CourseId { get; set; }
    public Course Course { get; set; } = default!;
}