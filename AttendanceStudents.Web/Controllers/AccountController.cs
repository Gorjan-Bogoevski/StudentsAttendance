using Microsoft.AspNetCore.Mvc;
using AttendanceStudents.Service.Interfaces;
using AttendanceStudents.Domain.Entities;

namespace AttendanceStudents.Web.Controllers;

public class AccountController : Controller
{
    private readonly IUserService _userService;

    public AccountController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public IActionResult Login()
    {
        // if already logged in -> redirect
        if (HttpContext.Session.GetString("UserId") != null)
            return RedirectToAction("Index", "Home");

        return View();
    }

    [HttpPost]
    public IActionResult Login(string username, string password, string? returnUrl = null)
    {
        var user = _userService.Login(username, password);

        if (user == null)
        {
            ViewBag.Error = "Invalid username or password.";
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        HttpContext.Session.SetString("UserId", user.Id.ToString());
        HttpContext.Session.SetString("UserType", user is Professor ? "Professor" : "Student");

        if (user is Professor p)
            HttpContext.Session.SetString("IsAdmin", p.IsAdmin ? "true" : "false");

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Index", user is Professor ? "Professor" : "Home");
    }

    [HttpPost]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction(nameof(Login));
    }
    
}