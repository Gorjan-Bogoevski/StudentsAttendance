using System.Security.Cryptography;
using System.Text;
using AttendanceStudents.Domain.DTOs;
using AttendanceStudents.Domain.Entities;
using AttendanceStudents.Domain.Enums;
using AttendanceStudents.Repository;
using AttendanceStudents.Service.Interfaces;

namespace AttendanceStudents.Service.Implementations;

public class SessionService : ISessionService
{
    private readonly IRepository<Session> _sessionRepo;
    private readonly IRepository<Attendance> _attendanceRepo;
    private readonly IQRCodeService _qrCodeService;
    private readonly IRepository<Student> _studentRepo;


    public SessionService(IRepository<Session> sessionRepo, IRepository<Attendance> attendanceRepo, IQRCodeService qrCodeService, IRepository<Student> studentRepo)
    {
        _sessionRepo = sessionRepo;
        _attendanceRepo = attendanceRepo;
        _qrCodeService = qrCodeService;
        _studentRepo = studentRepo;
    }

    public List<Session> GetByCourse(Guid courseId)
        => _sessionRepo.GetAll(s => s.CourseId == courseId)
                       .OrderBy(s => s.WeekNumber)
                       .ToList();

    public Session? GetById(Guid id)
        => _sessionRepo.Get(s => s.Id == id);

    public void Create(Session session)
    {
        Validate(session);

        session.Status = SessionStatus.Closed;
        session.OpenedAt = null;
        session.ClosedAt = null;

        session.AccessCodeHash = null;
        session.CodeIssuedAt = null;
        session.CodeValidUntil = null;

        session.TimesOpened = 0;

        _sessionRepo.Insert(session);
    }

    public void Update(Session session)
    {
        Validate(session);

        var existing = _sessionRepo.Get(s => s.Id == session.Id);
        if (existing == null)
            throw new ArgumentException("Session does not exist.");

        if (existing.Status == SessionStatus.Open)
            throw new InvalidOperationException("Не можеш да менуваш сесија додека е отворена.");

        existing.WeekNumber = session.WeekNumber;
        existing.LoginWindowSeconds = session.LoginWindowSeconds;

        _sessionRepo.Update(existing);
    }

    public void Delete(Guid id)
    {
        var existing = _sessionRepo.Get(s => s.Id == id);
        if (existing == null) return;

        if (existing.Status == SessionStatus.Open)
            throw new InvalidOperationException("Не можеш да избришеш отворена сесија.");

        _sessionRepo.Delete(existing);
    }

    public string Open(Guid sessionId)
    {
        var session = _sessionRepo.Get(s => s.Id == sessionId);
        if (session == null)
            throw new ArgumentException("Session not found.");

        if (session.Status == SessionStatus.Open)
            throw new InvalidOperationException("Сесијата е веќе отворена.");
        
        if (session.WeekNumber > 1)
        {
            var week1 = _sessionRepo.Get(s => s.CourseId == session.CourseId && s.WeekNumber == 1);
            if (week1 == null || !week1.SessionDate.HasValue)
                throw new InvalidOperationException("Мора прво да се започне Недела 1 (за да се постават датумите).");
        }

        var baseDate = DateOnly.FromDateTime(DateTime.UtcNow);
        
        DateOnly startDate;
        var week1ForCourse = _sessionRepo.Get(s => s.CourseId == session.CourseId && s.WeekNumber == 1);

        if (session.WeekNumber == 1)
        {
            startDate = week1ForCourse?.SessionDate ?? baseDate;

            if (week1ForCourse != null && !week1ForCourse.SessionDate.HasValue)
                week1ForCourse.SessionDate = startDate;
        }
        else
        {
            startDate = week1ForCourse!.SessionDate!.Value; 
        }

        var allSessions = _sessionRepo.GetAll(s => s.CourseId == session.CourseId).ToList();

        foreach (var s in allSessions)
        {
            var offsetDays = 7 * (s.WeekNumber - 1) + (s.WeekNumber >= 7 ? 7 : 0);
            var computed = startDate.AddDays(offsetDays);

            if (!s.SessionDate.HasValue)
                s.SessionDate = computed;
        }

  
        foreach (var s in allSessions)
            _sessionRepo.Update(s);

        if (!session.SessionDate.HasValue)
        {
            var offsetDays = 7 * (session.WeekNumber - 1) + (session.WeekNumber >= 7 ? 7 : 0);
            session.SessionDate = startDate.AddDays(offsetDays);
        }

        var now = DateTime.UtcNow;

        var rawCode = GenerateNumericCode(6);

        session.Status = SessionStatus.Open;
        session.OpenedAt = now;
        session.ClosedAt = null;

        session.TimesOpened += 1;

        session.CodeIssuedAt = now;
        if (session.LoginWindowSeconds <= 0 || session.LoginWindowSeconds > 300)
            session.LoginWindowSeconds = 300;
        session.CodeValidUntil = now.AddSeconds(session.LoginWindowSeconds);

        session.AccessCodeHash = Sha256(rawCode);

        _sessionRepo.Update(session);

        return rawCode;
    }

    public void Close(Guid sessionId)
    {
        var session = _sessionRepo.Get(s => s.Id == sessionId);
        if (session == null)
            throw new ArgumentException("Session not found.");

        if (session.Status == SessionStatus.Closed)
            return;

        session.Status = SessionStatus.Closed;
        session.ClosedAt = DateTime.UtcNow;

        // invalidate code
        session.AccessCodeHash = null;
        session.CodeIssuedAt = null;
        session.CodeValidUntil = null;

        _sessionRepo.Update(session);
    }

    public bool VerifyAccessCode(Guid sessionId, string code)
    {
        var session = _sessionRepo.Get(s => s.Id == sessionId);
        if (session == null) return false;

        if (session.Status != SessionStatus.Open) return false;
        if (session.AccessCodeHash == null) return false;

        var now = DateTime.UtcNow;
        if (session.CodeValidUntil.HasValue && now > session.CodeValidUntil.Value)
            return false;

        var hash = Sha256(code.Trim());
        return string.Equals(hash, session.AccessCodeHash, StringComparison.OrdinalIgnoreCase);
    }

    public bool RegisterAttendance(Guid sessionId, Guid studentId, string code)
    {
        if (studentId == Guid.Empty) return false;
        if (!VerifyAccessCode(sessionId, code)) return false;

        // prevent duplicates
        var existing = _attendanceRepo.Get(a => a.SessionId == sessionId && a.StudentId == studentId);
        if (existing != null) return false;

        var attendance = new Attendance
        {
            SessionId = sessionId,
            StudentId = studentId,
            AttendedAt = DateTime.UtcNow, };

        _attendanceRepo.Insert(attendance);
        return true;
    }

    private static void Validate(Session s)
    {
        if (s.CourseId == Guid.Empty)
            throw new ArgumentException("CourseId is required.");

        if (s.WeekNumber < 1 || s.WeekNumber > 12)
            throw new ArgumentException("WeekNumber мора да биде 1..12.");

        if (s.LoginWindowSeconds <= 0 || s.LoginWindowSeconds > 3600)
            throw new ArgumentException("LoginWindowSeconds мора да биде 1..3600 (1 час max).");
    }

    private static string GenerateNumericCode(int digits)
    {
        var bytes = RandomNumberGenerator.GetBytes(digits);
        var sb = new StringBuilder(digits);
        for (int i = 0; i < digits; i++)
            sb.Append((bytes[i] % 10).ToString());
        return sb.ToString();
    }

    private static string Sha256(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes);
    }
    
  
   public SessionLiveDto GetLiveInfo(Guid sessionId)
{
    var session = _sessionRepo.Get(s => s.Id == sessionId);
    if (session == null) throw new ArgumentException("Session not found.");

    var now = DateTime.UtcNow;

    // 1) земи сите attendance за оваа сесија
    var attendances = _attendanceRepo.GetAll(a => a.SessionId == session.Id).ToList();
    var attendanceCount = attendances.Count;

    // 2) земи студентите (по StudentId од attendance)
    var studentIds = attendances.Select(a => a.StudentId).Distinct().ToList();
    var students = _studentRepo.GetAll(s => studentIds.Contains(s.Id)).ToList();

    var studentDtos = students
        .OrderBy(s => s.Username)
        .Select(s => new LiveStudentDto
        {
            FullName = $"{s.FirstName} {s.LastName}".Trim(),
            Index = s.Username ?? ""
        })
        .ToList();

    // 3) ако не е OPEN -> врати count + листа (QR нема)
    if (session.Status != SessionStatus.Open || !session.OpenedAt.HasValue)
    {
        return new SessionLiveDto
        {
            IsOpen = false,
            SessionEndsAtMs = 0,
            AttendanceCount = attendanceCount,
            Students = studentDtos
        };
    }

    // 4) пресметај крај на сесијата (login window)
    var openedUtc = DateTime.SpecifyKind(session.OpenedAt.Value, DateTimeKind.Utc);
    var sessionEndsAt = openedUtc.AddSeconds(session.LoginWindowSeconds);

    // ако истече -> затвори, но врати листа!
    if (now >= sessionEndsAt)
    {
        session.Status = SessionStatus.Closed;
        session.ClosedAt = now;
        _sessionRepo.Update(session);

        return new SessionLiveDto
        {
            IsOpen = false,
            SessionEndsAtMs = new DateTimeOffset(sessionEndsAt).ToUnixTimeMilliseconds(),
            AttendanceCount = attendanceCount,
            Students = studentDtos
        };
    }

    // 5) OPEN: генерирај нов QR како што имаш
    var rawCode = GenerateNumericCode(6);
    session.AccessCodeHash = Sha256(rawCode);
    session.CodeIssuedAt = now;
    session.CodeValidUntil = now.AddSeconds(50);
    _sessionRepo.Update(session); 
    
    // var joinUrl = $"http://192.168.1.146:5035/Attendance/Join?sessionId={session.Id}&code={rawCode}";
    // var qr = _qrCodeService.GeneratePngDataUri(joinUrl);

    return new SessionLiveDto
    {
        IsOpen = true,
        SessionEndsAtMs = new DateTimeOffset(sessionEndsAt).ToUnixTimeMilliseconds(),
        AttendanceCount = attendanceCount,
        Students = studentDtos,
        QrCodeBase64 = session.Id.ToString() + " " + rawCode
    };
}

    
    
public void AdjustLoginWindow(Guid sessionId, int deltaSeconds)
{
    var session = _sessionRepo.Get(s => s.Id == sessionId);
    if (session == null) throw new ArgumentException("Session not found.");
    
    if (session.Status != SessionStatus.Open || !session.OpenedAt.HasValue) return;

    session.LoginWindowSeconds = Math.Max(0, session.LoginWindowSeconds + deltaSeconds);

    _sessionRepo.Update(session);
}
}