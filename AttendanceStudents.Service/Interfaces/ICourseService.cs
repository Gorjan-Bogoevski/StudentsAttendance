using AttendanceStudents.Domain.Entities;

namespace AttendanceStudents.Service.Interfaces;

public interface ICourseService
{
    List<Course> GetAll();
    Course? GetById(Guid id);
    void Create(Course course);
    void Update(Course course);
    void Delete(Guid id);
}