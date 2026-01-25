using AttendanceStudents.Domain.Entities;
using AttendanceStudents.Repository;
using AttendanceStudents.Service.Interfaces;

namespace AttendanceStudents.Service.Implementations;

public class ProfessorService : IProfessorService
{
    private readonly IRepository<Course> _courseRepo;
    private readonly IRepository<ProfessorCourse> _pcRepo;
    private readonly IRepository<Session> _sessionRepo;

    public ProfessorService(IRepository<Course> courseRepo, IRepository<ProfessorCourse> pcRepo, IRepository<Session> sessionRepo)
    {
        _courseRepo = courseRepo;
        _pcRepo = pcRepo;
        _sessionRepo = sessionRepo;
    }

    public List<Course> GetMyCourses(Guid professorId)
    {
        var ids = GetMyCourseIds(professorId);

        var courses = _courseRepo
            .GetAll(c => ids.Contains(c.Id))
            .ToList();

        foreach (var c in courses)
        {
            c.Sessions = _sessionRepo
                .GetAll(s => s.CourseId == c.Id)
                .OrderBy(s => s.WeekNumber)
                .ToList();
        }

        return courses;
    }

    public HashSet<Guid> GetMyCourseIds(Guid professorId)
    {
        return _pcRepo
            .GetAll( pc => pc.ProfessorId == professorId)
            .Select(pc => pc.CourseId)
            .ToHashSet();
    }

    public bool IsCourseAssigned(Guid professorId, Guid courseId)
        => _pcRepo.Get(pc => pc.ProfessorId == professorId && pc.CourseId == courseId) != null;

    public bool AddCourseToProfessor(Guid professorId, Guid courseId)
    {
        if (IsCourseAssigned(professorId, courseId))
            return false;

        var newCourse = _courseRepo.Get(c => c.Id == courseId);
        if (newCourse == null) return false;

        if (!newCourse.DayOfWeek.HasValue || !newCourse.StartTime.HasValue || !newCourse.EndTime.HasValue)
        {
            _pcRepo.Insert(new ProfessorCourse { ProfessorId = professorId, CourseId = courseId });
            return true;
        }

        var ns = newCourse.StartTime.Value;
        var ne = newCourse.EndTime.Value;

        var existingCourses = _courseRepo.GetAll(c =>
            c.ProfessorCourses.Any(pc => pc.ProfessorId == professorId) &&
            c.DayOfWeek == newCourse.DayOfWeek &&
            c.StartTime.HasValue && c.EndTime.HasValue
        ).ToList();

        var hasOverlap = existingCourses.Any(c =>
            ns < c.EndTime!.Value && ne > c.StartTime!.Value
        );

        _pcRepo.Insert(new ProfessorCourse { ProfessorId = professorId, CourseId = courseId });

        return !hasOverlap;
    }

    public void RemoveCourseFromProfessor(Guid professorId, Guid courseId)
    {
        var link = _pcRepo.Get(pc => pc.ProfessorId == professorId && pc.CourseId == courseId);
        if (link != null)
            _pcRepo.Delete(link);
    }
}