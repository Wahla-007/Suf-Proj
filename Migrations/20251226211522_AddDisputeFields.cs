using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mess_management.Migrations
{
    /// <inheritdoc />
    public partial class AddDisputeFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "VerificationNote",
                table: "TeacherAttendance",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1)",
                oldMaxLength: 1,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DisputeReason",
                table: "TeacherAttendance",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DisputeStatus",
                table: "TeacherAttendance",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                defaultValue: "None");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisputeReason",
                table: "TeacherAttendance");

            migrationBuilder.DropColumn(
                name: "DisputeStatus",
                table: "TeacherAttendance");

            migrationBuilder.AlterColumn<string>(
                name: "VerificationNote",
                table: "TeacherAttendance",
                type: "nvarchar(1)",
                maxLength: 1,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);
        }
    }
}
