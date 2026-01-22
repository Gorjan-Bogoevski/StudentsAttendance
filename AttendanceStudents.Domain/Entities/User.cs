using AttendanceStudents.Domain.Common;

namespace AttendanceStudents.Domain.Entities;


public abstract class User : BaseEntity
{

    public string Username { get; set; } 
    public string Password { get; set; }

    protected User(string username, string password)
    {
        Username = username;
        Password = password;
    }
}