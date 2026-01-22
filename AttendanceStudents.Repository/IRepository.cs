using AttendanceStudents.Domain.Common;
using System.Linq.Expressions;

namespace AttendanceStudents.Repository;

public interface IRepository<T> where T : BaseEntity
{
    T Insert(T entity);
    T Update(T entity);
    T Delete(T entity);

    T? Get(Expression<Func<T, bool>> predicate);
    IEnumerable<T> GetAll(Expression<Func<T, bool>>? predicate = null);
    IEnumerable<T> InsertRange(IEnumerable<T> entities);
    int Count(Expression<Func<T, bool>> predicate);

}