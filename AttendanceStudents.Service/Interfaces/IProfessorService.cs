using AttendanceStudents.Domain.Entities;

namespace AttendanceStudents.Service.Interfaces;

public interface IProfessorService
{
    List<Course> GetMyCourses(Guid professorId);
    HashSet<Guid> GetMyCourseIds(Guid professorId);

    bool AddCourseToProfessor(Guid professorId, Guid courseId);
    void RemoveCourseFromProfessor(Guid professorId, Guid courseId);

    bool IsCourseAssigned(Guid professorId, Guid courseId);
}