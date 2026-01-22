using AttendanceStudents.Domain.Entities;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace AttendanceStudents.Repository;
using Microsoft.EntityFrameworkCore;


public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Professor> Professors => Set<Professor>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<ProfessorCourse> ProfessorCourses => Set<ProfessorCourse>();
    public DbSet<Session> Sessions { get; set; }
    public DbSet<Attendance> Attendances { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasDiscriminator<string>("UserType")
            .HasValue<Student>("Student")
            .HasValue<Professor>("Professor");
        
        var timeOnlyConverter = new ValueConverter<TimeOnly?, string?>(
            v => v.HasValue ? v.Value.ToString("HH:mm") : null,
            v => string.IsNullOrEmpty(v) ? null : TimeOnly.Parse(v)
        );

        modelBuilder.Entity<Course>()
            .Property(c => c.StartTime)
            .HasConversion(timeOnlyConverter);

        modelBuilder.Entity<Course>()
            .Property(c => c.EndTime)
            .HasConversion(timeOnlyConverter);
        
        modelBuilder.Entity<ProfessorCourse>()
            .HasOne(pc => pc.Professor)
            .WithMany(p => p.ProfessorCourses)
            .HasForeignKey(pc => pc.ProfessorId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ProfessorCourse>()
            .HasOne(pc => pc.Course)
            .WithMany(c => c.ProfessorCourses)
            .HasForeignKey(pc => pc.CourseId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<ProfessorCourse>()
            .HasIndex(pc => new { pc.ProfessorId, pc.CourseId })
            .IsUnique();
        modelBuilder.Entity<Attendance>()
            .HasIndex(a => new { a.SessionId, a.StudentId })
            .IsUnique();
        
        var nullableDateOnlyConverter = new ValueConverter<DateOnly?, string?>(
            d => d.HasValue ? d.Value.ToString("yyyy-MM-dd") : null,
            s => string.IsNullOrWhiteSpace(s) ? null : DateOnly.Parse(s)
        );

        modelBuilder.Entity<Session>()
            .Property(s => s.SessionDate)
            .HasConversion(nullableDateOnlyConverter);
    }
}