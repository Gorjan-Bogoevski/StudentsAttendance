using AttendanceStudents.Domain.Entities;
using AttendanceStudents.Domain.Enums;
using AttendanceStudents.Repository;
using AttendanceStudents.Service.Interfaces;

namespace AttendanceStudents.Service.Implementations;

public class CourseService : ICourseService
{
    private readonly IRepository<Course> _courseRepo;
    private readonly IRepository<Session> _sessionRepo;

    public CourseService(IRepository<Course> courseRepo, IRepository<Session> sessionRepo)
    {
        _courseRepo = courseRepo;
        _sessionRepo = sessionRepo;
    }

    
    public List<Course> GetAll()
        => _courseRepo.GetAll().ToList();

    public Course? GetById(Guid id)
        => _courseRepo.Get( x => x.Id == id);

    public void Create(Course course)
    {
        Validate(course);
        
        var created = _courseRepo.Insert(course);

 
        var sessions = new List<Session>();
        for (int week = 1; week <= created.WeeksCount; week++)
        {
            sessions.Add(new Session
            {
                CourseId = created.Id,
                WeekNumber = week,
                Status = SessionStatus.Closed
               
            });
        }
        
        _sessionRepo.InsertRange(sessions);
    }

    public void Update(Course course)
    {
        Validate(course);

        var existing = _courseRepo.Get( x => x.Id == course.Id);
        if (existing == null)
            throw new ArgumentException("Предметот не постои.");

        existing.Name = course.Name;
        existing.Semester = course.Semester;
        existing.DayOfWeek = course.DayOfWeek;
        existing.StartTime = course.StartTime;
        existing.EndTime = course.EndTime;
        

        _courseRepo.Update(existing);
    }

    public void Delete(Guid id)
    {
        var entity = _courseRepo.Get( x => x.Id == id);
        if (entity == null) return;

        _courseRepo.Delete(entity);
    }

    private static void Validate(Course c)
    {
        if (string.IsNullOrWhiteSpace(c.Name))
            throw new ArgumentException("Името на предметот е задолжително.");
        
        if (c.StartTime.HasValue && c.EndTime.HasValue)
        {
            if (c.EndTime <= c.StartTime)
                throw new ArgumentException("Крајот мора да биде после почетокот.");
        }
    }
}