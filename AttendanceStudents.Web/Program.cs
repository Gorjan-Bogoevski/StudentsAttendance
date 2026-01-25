using AttendanceStudents.Domain.Entities;
using AttendanceStudents.Repository;
using AttendanceStudents.Service.Interfaces;
using AttendanceStudents.Service.Implementations;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// MVC
builder.Services.AddControllersWithViews();

// --------------------
// DB (SQLite) – Azure-safe path
// --------------------
static string ResolveSqlitePath(IHostEnvironment env)
{
    // Local dev -> keep db in project folder (easy)
    if (env.IsDevelopment())
        return Path.Combine(env.ContentRootPath, "attendance.db");

    // Azure App Service -> persistent storage under HOME/data
    // Windows: HOME = D:\home
    // Linux:   HOME = /home
    var home = Environment.GetEnvironmentVariable("HOME");
    if (!string.IsNullOrWhiteSpace(home))
    {
        var dataDir = Path.Combine(home, "data");
        Directory.CreateDirectory(dataDir);
        return Path.Combine(dataDir, "attendance.db");
    }

    // Fallback
    return Path.Combine(env.ContentRootPath, "attendance.db");
}

var sqlitePath = ResolveSqlitePath(builder.Environment);
var sqliteConn = $"Data Source={sqlitePath}";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(sqliteConn));

// Repositories
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IProfessorService, ProfessorService>();
builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddScoped<IAttendanceService, AttendanceService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IQRCodeService, QrCodeService>();

// Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;

    // Cross-site cookies are needed only if you use ngrok / different domain.
    // Keep as you had it:
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

var app = builder.Build();

// --------------------
// Forwarded Headers (MUST be early)
// --------------------
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto
});

// Error handling
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Static + routing
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Session before auth
app.UseSession();
app.UseAuthorization();

// --------------------
// Apply migrations on startup (so Azure creates/updates DB)
// --------------------
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

// Routes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();