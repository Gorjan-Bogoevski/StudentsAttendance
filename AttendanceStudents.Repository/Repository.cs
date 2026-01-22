
using Microsoft.EntityFrameworkCore;
using AttendanceStudents.Domain.Common;
using System.Linq.Expressions;

namespace AttendanceStudents.Repository;
public class Repository<T> : IRepository<T> where T : BaseEntity
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<T> _entities;

    public Repository(ApplicationDbContext context)
    {
        _context = context;
        _entities = context.Set<T>();
    }

    public T Insert(T entity)
    {
        _entities.Add(entity);
        _context.SaveChanges();
        return entity;
    }

    public T Update(T entity)
    {
        _entities.Update(entity);
        _context.SaveChanges();
        return entity;
    }

    public T Delete(T entity)
    {
        _entities.Remove(entity);
        _context.SaveChanges();
        return entity;
    }

    public T? Get(Expression<Func<T, bool>> predicate)
        => _entities.FirstOrDefault(predicate);

    public IEnumerable<T> GetAll(Expression<Func<T, bool>>? predicate = null)
        => predicate == null ? _entities.ToList() : _entities.Where(predicate).ToList();
    
    public IEnumerable<T> InsertRange(IEnumerable<T> entities)
    {
        _context.Set<T>().AddRange(entities);
        _context.SaveChanges();
        return entities;
    }
    public int Count(Expression<Func<T, bool>> predicate)
    {
        return _entities.Count(predicate);
    }
}