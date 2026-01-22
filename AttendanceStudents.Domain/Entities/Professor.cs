namespace AttendanceStudents.Domain.Entities;

public class Professor : User
{
    public Professor(string username, string password, bool isAdmin) : base(username, password)
    {
        IsAdmin = isAdmin;
    }

    public bool IsAdmin { get; set; } = false;
    public ICollection<ProfessorCourse> ProfessorCourses { get; set; }
        = new List<ProfessorCourse>();
}