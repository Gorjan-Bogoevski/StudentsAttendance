namespace AttendanceStudents.Domain.DTOs;

public class AttendanceCellDto
{
    public int WeekNumber { get; set; }

    public bool Present { get; set; }

    public DateTime? JoinedAtUtc { get; set; }

    public Guid? SessionId { get; set; }

    public int JoinCount { get; set; } = 0;
}