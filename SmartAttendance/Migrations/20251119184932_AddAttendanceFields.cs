using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAttendance.Migrations
{
    /// <inheritdoc />
    public partial class AddAttendanceFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CourseName",
                table: "Attendances",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Semester",
                table: "Attendances",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Year",
                table: "Attendances",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CourseName",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "Semester",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "Year",
                table: "Attendances");
        }
    }
}
