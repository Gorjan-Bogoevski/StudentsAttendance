using AttendanceStudents.Domain.DTOs;

namespace AttendanceStudents.Service.Interfaces;

public interface IReportService
{
    CourseReportDto BuildCourseReport(Guid courseId, string? search = null, int? week = null);
}