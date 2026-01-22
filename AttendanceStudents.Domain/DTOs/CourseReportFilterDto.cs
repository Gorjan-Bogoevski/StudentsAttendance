namespace AttendanceStudents.Domain.DTOs;

public class CourseReportFilterDto
{
    public Guid CourseId { get; set; }

    public int? Week { get; set; }

    public string? Search { get; set; }

    public bool? PresentOnly { get; set; }
    
    public Dictionary<int, DateOnly?> WeekDates { get; set; } = new();

    public List<int> Weeks { get; set; } = new();
}