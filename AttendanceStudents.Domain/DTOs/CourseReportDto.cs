namespace AttendanceStudents.Domain.DTOs;

public class CourseReportDto
{
    public Guid CourseId { get; set; }
    public string CourseName { get; set; } = "";
    public string Semester { get; set; } = ""; 
    public Dictionary<int, DateOnly?> WeekDates { get; set; } = new();
    public int TotalWeeks { get; set; } = 12;
    public List<int> AllWeeks { get; set; } = new();
    public List<int> Weeks { get; set; } = new();
    
    public List<StudentAttendanceRowDto> Students { get; set; } = new();
    
    public int TotalStudents => Students.Count;
    public int TotalAttendances { get; set; } 
}