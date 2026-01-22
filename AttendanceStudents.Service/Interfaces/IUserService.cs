using AttendanceStudents.Domain.Entities;

namespace AttendanceStudents.Service.Interfaces;

public interface IUserService
{
    User? Login(string username, string password);
}