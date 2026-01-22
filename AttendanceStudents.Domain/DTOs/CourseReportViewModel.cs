namespace AttendanceStudents.Domain.DTOs;

public class CourseReportViewModel
{
    public CourseReportDto Report { get; set; } = new();
    public CourseReportFilterDto Filter { get; set; } = new();
}