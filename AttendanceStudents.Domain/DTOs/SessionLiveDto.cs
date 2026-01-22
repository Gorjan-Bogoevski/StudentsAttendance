namespace AttendanceStudents.Domain.DTOs;

public class SessionLiveDto
{
    public bool IsOpen { get; set; }
    public long SessionEndsAtMs { get; set; }
    public int AttendanceCount { get; set; }
    public string? QrCodeBase64 { get; set; }
    public List<LiveStudentDto> Students { get; set; } = new();

}