using AttendanceStudents.Domain.DTOs;
using AttendanceStudents.Domain.Entities;
using AttendanceStudents.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AttendanceStudents.Web.Controllers;

public class ProfessorController : Controller
{
    private readonly ICourseService _courseService;
    private readonly IProfessorService _professorService;
    private readonly IReportService _reportService;

    public ProfessorController(ICourseService courseService, IProfessorService professorService, IReportService reportService)
    {
        _courseService = courseService;
        _professorService = professorService;
        _reportService = reportService;
    }

    private bool IsLoggedIn() => HttpContext.Session.GetString("UserId") != null;
    private bool IsProfessor() => HttpContext.Session.GetString("UserType") == "Professor";
    private bool IsAdmin() => HttpContext.Session.GetString("IsAdmin") == "true";

    private bool TryGetProfessorId(out Guid professorId)
    {
        professorId = Guid.Empty;
        var idStr = HttpContext.Session.GetString("UserId");
        return Guid.TryParse(idStr, out professorId);
    }
    [HttpGet]
    public IActionResult Index()
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
        if (!IsProfessor()) return RedirectToAction("Index", "Home");
        if (!TryGetProfessorId(out var professorId)) return RedirectToAction("Login", "Account");
        ViewBag.MyCourseIds = _professorService.GetMyCourseIds(professorId);
        var myCourses = _professorService.GetMyCourses(professorId);
        return View(myCourses);
    }
    
    [HttpGet]
    public IActionResult Courses()
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
        if (!IsProfessor()) return RedirectToAction("Index", "Home");
        if (!TryGetProfessorId(out var professorId)) return RedirectToAction("Login", "Account");

        var courses = _courseService.GetAll();

        ViewBag.IsAdmin = IsAdmin();
        ViewBag.MyCourseIds = _professorService.GetMyCourseIds(professorId);

        return View(courses);
    }

    [HttpGet]
    public IActionResult MyCourses()
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
        if (!IsProfessor()) return RedirectToAction("Index", "Home");
        if (!TryGetProfessorId(out var professorId)) return RedirectToAction("Login", "Account");

        var myCourses = _professorService.GetMyCourses(professorId);
        return View(myCourses);
    }

    [HttpPost]
    public IActionResult AddToMyCourses(Guid id)
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
        if (!IsProfessor()) return RedirectToAction("Index", "Home");
        if (!TryGetProfessorId(out var professorId)) return RedirectToAction("Login", "Account");

        _professorService.AddCourseToProfessor(professorId, id);
        return RedirectToAction(nameof(Courses));
    }

    [HttpPost]
    public IActionResult RemoveFromMyCourses(Guid id) 
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
        if (!IsProfessor()) return RedirectToAction("Index", "Home");
        if (!TryGetProfessorId(out var professorId)) return RedirectToAction("Login", "Account");

        _professorService.RemoveCourseFromProfessor(professorId, id);
        return RedirectToAction(nameof(Index));
    }


    [HttpGet]
    public IActionResult CreateCourse()
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
        if (!IsProfessor()) return RedirectToAction("Index", "Home");
        if (!IsAdmin()) return RedirectToAction(nameof(Courses));

        return View(new Course());
    }

    [HttpPost]
    public IActionResult CreateCourse(Course model)
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
        if (!IsProfessor()) return RedirectToAction("Index", "Home");
        if (!IsAdmin()) return RedirectToAction(nameof(Courses));

        try
        {
            _courseService.Create(model);
            return RedirectToAction(nameof(Courses));
        }
        catch (Exception ex)
        {
            ViewBag.Error = ex.Message;
            return View(model);
        }
    }

    [HttpGet]
    public IActionResult EditCourse(Guid id)
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
        if (!IsProfessor()) return RedirectToAction("Index", "Home");
        if (!IsAdmin()) return RedirectToAction(nameof(Courses));

        var course = _courseService.GetById(id);
        if (course == null) return RedirectToAction(nameof(Courses));

        return View(course);
    }

    [HttpPost]
    public IActionResult EditCourse(Course model)
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
        if (!IsProfessor()) return RedirectToAction("Index", "Home");
        if (!IsAdmin()) return RedirectToAction(nameof(Courses));

        try
        {
            _courseService.Update(model);
            return RedirectToAction(nameof(Courses));
        }
        catch (Exception ex)
        {
            ViewBag.Error = ex.Message;
            return View(model);
        }
    }

    [HttpPost]
    public IActionResult DeleteCourse(Guid id)
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
        if (!IsProfessor()) return RedirectToAction("Index", "Home");
        if (!IsAdmin()) return RedirectToAction(nameof(Courses));

        _courseService.Delete(id);
        return RedirectToAction(nameof(Courses));
    }
    
    [HttpGet]
    public IActionResult Reports()
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
        if (!IsProfessor()) return RedirectToAction("Index", "Home");

        var professorId = Guid.Parse(HttpContext.Session.GetString("UserId")!);
        var courses = _professorService.GetMyCourses(professorId);

        return View(courses); 
    }

    [HttpGet]
    public IActionResult CourseReport(Guid courseId, string? search, int? week)
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
        if (!IsProfessor()) return RedirectToAction("Index", "Home");

        var report = _reportService.BuildCourseReport(courseId, search, week);

        var vm = new CourseReportViewModel
        {
            Report = report,
            Filter = new CourseReportFilterDto
            {
                CourseId = courseId,
                Search = search,
                Week = week,
            }
        };

        return View(vm);
    }
}