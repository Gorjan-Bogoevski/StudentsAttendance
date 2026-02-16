# Attendance Management System

A robust and efficient web-based application designed to streamline the process of tracking student attendance in educational institutions. Built with **ASP.NET Core MVC 9.0**, this system leverages **QR code technology** to allow for quick, secure, and verifiable attendance marking.

##  Key Features

*   **Role-Based Access Control:** Secure login and distinct dashboards for **Professors** and **Students**.
*   **Dynamic QR Code Generation:** Professors can generate unique QR codes for each class session which refresh dynamically or are valid for a specific window.
*   **Real-Time Attendance Tracking:** Students can mark their attendance instantly by scanning the QR code displayed in class.
*   **Live Session Monitoring:** Professors can view attendance counts and student lists update in real-time as students join.
*   **Session Management:**
    *   Open and Close attendance windows.
    *   Set custom time limits for check-ins.
    *   Manual adjustments for late arrivals or special cases.
*   **Comprehensive Reporting:** detailed attendance reports per course, week, or student.
*   **Secure Architecture:** Built with layered architecture principles ensuring separation of concerns and maintainability.

##  Architecture

The solution follows a clean, **N-Layered Architecture** to ensure scalability and testability:

1.  **AttendanceStudents.Web**: The Presentation Layer. Contains Controllers, Views (Razor), and ViewModel logic. Handles HTTP requests and renders the UI.
2.  **AttendanceStudents.Service**: The Business Logic Layer. Contains services (`AttendanceService`, `SessionService`, etc.) that implement the core business rules and orchestrate data flow.
3.  **AttendanceStudents.Repository**: The Data Access Layer. Implements the **Repository Pattern** using **Entity Framework Core**. Handles all database interactions.
4.  **AttendanceStudents.Domain**: The Core Layer. Contains the fundamental Entities (`Student`, `Course`, `Session`), DTOs, Enums, and common interfaces.

##  Tech Stack

*   **Framework:** .NET 9.0 (ASP.NET Core MVC)
*   **Database:** SQLite (default for development) / SQL Server (supported via EF Core)
*   **ORM:** Entity Framework Core 9.0.11
*   **Libraries:**
    *   `QRCoder`: For generating session QR codes.
    *   `AutoMapper` (Conceptually used in services for DTO mapping).
*   **Frontend:** HTML5, CSS3, JavaScript, Bootstrap.

##  Getting Started

### Prerequisites
*   [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) installed.
*   An IDE like JetBrains Rider, Visual Studio 2022, or VS Code.

### Installation

1.  **Clone the repository:**
    ```bash
    git clone https://github.com/your-username/AttendanceStudents.git
    cd AttendanceStudents
    ```

2.  **Configuration:**
    *   Check `AttendanceStudents.Web/appsettings.json` for the database connection string. By default, it uses a local SQLite database (`attendance.db`).

3.  **Database Setup:**
    *   Apply the Entity Framework Core migrations to create the database schema:
    ```bash
    dotnet ef database update --project AttendanceStudents.Repository --startup-project AttendanceStudents.Web
    ```
    *   *Note: Ensure you have the `dotnet-ef` tool installed globally (`dotnet tool install --global dotnet-ef`).*

4.  **Run the Application:**
    ```bash
    dotnet run --project AttendanceStudents.Web
    ```
    *   The application will typically start at `http://localhost:5000` or `https://localhost:5001`.

##  Usage Guide

### For Professors
1.  **Log In:** Use your professor credentials.
2.  **Manage Courses:** View your assigned courses.
3.  **Start Session:** Select a course and "Open Session". A unique QR code will be displayed.
4.  **Monitor:** Watch as students scan the code; the attendance list updates live.
5.  **Close Session:** Manually close the session or let the timer expire.

### For Students
1.  **Log In:** Use your student credentials.
2.  **Join Session:**
    *   **Scan:** Use a mobile device to scan the QR code projected by the professor.
    *   **Manual Entry:** Alternatively, enter the unique session code if provided.
3.  **Confirmation:** You will receive a success message confirming your attendance for that date and course.
