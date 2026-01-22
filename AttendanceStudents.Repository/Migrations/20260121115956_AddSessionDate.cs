using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AttendanceStudents.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddSessionDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SessionDate",
                table: "Sessions",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SessionDate",
                table: "Sessions");
        }
    }
}
