namespace AttendanceStudents.Service.Interfaces;

public interface IQRCodeService
{
    string GeneratePngDataUri(string text);
}