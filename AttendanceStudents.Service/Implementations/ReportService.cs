using AttendanceStudents.Domain.Entities;
using AttendanceStudents.Domain.DTOs;
using AttendanceStudents.Service.Interfaces;
using AttendanceStudents.Repository; 
using System;
using System.Collections.Generic;
using System.Linq;

namespace AttendanceStudents.Service.Implementations
{
    public class ReportService : IReportService
    {
        private readonly IRepository<Course> _courseRepo;
        private readonly IRepository<Session> _sessionRepo;
        private readonly IRepository<Attendance> _attendanceRepo;
        private readonly IRepository<Student> _studentRepo;

        public ReportService(
            IRepository<Course> courseRepo,
            IRepository<Session> sessionRepo,
            IRepository<Attendance> attendanceRepo,
            IRepository<Student> studentRepo)
        {
            _courseRepo = courseRepo;
            _sessionRepo = sessionRepo;
            _attendanceRepo = attendanceRepo;
            _studentRepo = studentRepo;
        }

        public CourseReportDto BuildCourseReport(Guid courseId, string? search = null, int? week = null)
        {
            var course = _courseRepo.Get(c => c.Id == courseId);
            if (course == null) throw new ArgumentException("Course not found");

            // 1) Сесии за курс (обично 12)
            var sessions = _sessionRepo
                .GetAll(s => s.CourseId == courseId)
                .ToList();

            var allWeeks = sessions
                .Select(s => s.WeekNumber)
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            var weeksToShow = (week.HasValue && week.Value > 0)
                ? new List<int> { week.Value }
                : allWeeks;
            
            var weekDates = sessions
                .GroupBy(s => s.WeekNumber)
                .ToDictionary(
                    g => g.Key,
                    g => g.First().SessionDate
                );
            // 2) Attendance за тие сесии
            var sessionIds = sessions.Select(s => s.Id).ToList();

            var attendance = _attendanceRepo
                .GetAll(a => sessionIds.Contains(a.SessionId))
                .ToList();

            // 3) Студенти што барем еднаш се појавиле (плус можеш да прошириш ако имаш „enrollment“ табела)
            var studentIds = attendance.Select(a => a.StudentId).Distinct().ToList();
            var students = _studentRepo.GetAll(s => studentIds.Contains(s.Id)).ToList();

            // 4) Search filter (по индекс/име)
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                students = students
                    .Where(s =>
                        ((s.Username ?? "").ToLower().Contains(term)))
                    .ToList();
            }

            // 5) Мапа sessionId -> weekNumber (за брзо)
            var sessionWeek = sessions.ToDictionary(x => x.Id, x => x.WeekNumber);

            // 6) Гради редови
            var rows = new List<StudentAttendanceRowDto>();

            foreach (var st in students.OrderBy(s => s.Username))
            {
                var row = new StudentAttendanceRowDto
                {
                    StudentId = st.Id,
                    Index = st.Username ?? "",
                    FullName = "",
                    ByWeek = new Dictionary<int, AttendanceCellDto>()
                };

             

                foreach (var w in weeksToShow)
                {
                    row.ByWeek[w] = new AttendanceCellDto
                    {
                        WeekNumber = w,
                        Present = false,
                        JoinedAtUtc = null,
                        SessionId = sessions.FirstOrDefault(ss => ss.WeekNumber == w)?.Id
                    };
                }

                // пополни присуство од attendance
                var my = attendance.Where(a => a.StudentId == st.Id).ToList();

                foreach (var a in my)
                {
                    if (!sessionWeek.TryGetValue(a.SessionId, out var w)) continue;

                    if (week.HasValue && week.Value > 0 && w != week.Value) continue;

                    if (!row.ByWeek.ContainsKey(w))
                    {
                        row.ByWeek[w] = new AttendanceCellDto { WeekNumber = w };
                    }

                    row.ByWeek[w].Present = true;

                   
                    row.ByWeek[w].SessionId = a.SessionId;

                    row.ByWeek[w].JoinCount += 1;
                }

                row.PresentCount = row.ByWeek.Values.Count(x => x.Present);
                row.AbsentCount = row.ByWeek.Values.Count(x => !x.Present);

                rows.Add(row);
            }

            return new CourseReportDto
            {
                CourseId = course.Id,
                CourseName = course.Name,
                Semester = course.Semester.ToString(),
                TotalWeeks = allWeeks.Count == 0 ? 12 : allWeeks.Count,
                AllWeeks = allWeeks,          
                Weeks = weeksToShow,  
                Students = rows,
                TotalAttendances = attendance.Count,
                WeekDates = weekDates
            };
        }
    }
}