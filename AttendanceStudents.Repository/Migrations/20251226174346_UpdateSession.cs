using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AttendanceStudents.Repository.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccessCodeHash",
                table: "Sessions",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CodeIssuedAt",
                table: "Sessions",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CodeValidUntil",
                table: "Sessions",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccessCodeHash",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "CodeIssuedAt",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "CodeValidUntil",
                table: "Sessions");
        }
    }
}
