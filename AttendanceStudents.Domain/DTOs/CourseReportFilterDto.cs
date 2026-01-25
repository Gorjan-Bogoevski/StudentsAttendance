namespace AttendanceStudents.Domain.DTOs;

public class CourseReportFilterDto
{
    public Guid CourseId { get; set; }

    public int? Week { get; set; }

    public string? Search { get; set; }
    
}