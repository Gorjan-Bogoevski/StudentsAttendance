namespace AttendanceStudents.Domain.DTOs;

public class CourseReportDto
{
    public Guid CourseId { get; set; }
    public string CourseName { get; set; } = "";
    public string Semester { get; set; } = ""; // "Winter"/"Summer" или enum->string
    public Dictionary<int, DateOnly?> WeekDates { get; set; } = new();
    // колку сесии има (обично 12)
    public int TotalWeeks { get; set; } = 12;
    public List<int> AllWeeks { get; set; } = new();


    // листа недели што постојат (1..12) - корисно ако некогаш не се 12
    public List<int> Weeks { get; set; } = new();

    // редови (студенти)
    public List<StudentAttendanceRowDto> Students { get; set; } = new();

    // статистика
    public int TotalStudents => Students.Count;
    public int TotalAttendances { get; set; } // вкупно „present“ записи за овој курс (опционално)
}