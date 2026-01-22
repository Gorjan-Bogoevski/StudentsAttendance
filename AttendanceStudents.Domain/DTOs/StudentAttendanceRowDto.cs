namespace AttendanceStudents.Domain.DTOs;

public class StudentAttendanceRowDto
{
    public Guid StudentId { get; set; }
    public string Index { get; set; } = "";      
    public string FullName { get; set; } = "";   


    public Dictionary<int, AttendanceCellDto> ByWeek { get; set; } = new();
    
    public int PresentCount { get; set; }
    public int AbsentCount { get; set; }
}