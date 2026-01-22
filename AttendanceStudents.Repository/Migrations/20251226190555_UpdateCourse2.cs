using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AttendanceStudents.Repository.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCourse2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TimesOpened",
                table: "Sessions",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TimesOpened",
                table: "Sessions");
        }
    }
}
