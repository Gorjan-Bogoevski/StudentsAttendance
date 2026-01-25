using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AttendanceStudents.Domain;
using AttendanceStudents.Repository;

namespace AttendanceStudents.Web.Controllers;

public class HomeController : Controller

{
    public IActionResult Index()
    {
        if (HttpContext.Session.GetString("UserId") == null)
            return RedirectToAction("Login", "Account");

        return View();
    }

    public IActionResult StudentDashboard()
    {
        if (HttpContext.Session.GetString("UserId") == null)
            return RedirectToAction("Login", "Account");

        if (HttpContext.Session.GetString("UserType") != "Student")
            return RedirectToAction("Index");

        return View();
    }



    public IActionResult AdminDashboard()
    {
        if (HttpContext.Session.GetString("UserId") == null)
            return RedirectToAction("Login", "Account");

        if (HttpContext.Session.GetString("UserType") != "Professor")
            return RedirectToAction("Index");

        if (HttpContext.Session.GetString("IsAdmin") != "true")
            return RedirectToAction("Index", "Professor");

        return View();
    }
    
}