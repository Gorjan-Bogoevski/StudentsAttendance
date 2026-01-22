using AttendanceStudents.Domain.Common;

namespace AttendanceStudents.Domain.Entities;

public class Student : User
{
    public string FirstName { get; set; } 
    public string LastName { get; set; }

    public Student(string username, string password, string firstName, string lastName) : base(username, password)
    {
        FirstName = firstName;
        LastName = lastName;
    }
}