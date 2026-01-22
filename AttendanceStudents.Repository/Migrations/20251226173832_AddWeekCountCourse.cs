using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AttendanceStudents.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddWeekCountCourse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WeeksCount",
                table: "Courses",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WeeksCount",
                table: "Courses");
        }
    }
}
