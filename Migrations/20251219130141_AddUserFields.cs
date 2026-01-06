using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mess_management.Migrations
{
    /// <inheritdoc />
    public partial class AddUserFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The database already contains an AspNetUsers table from the original scaffold.
            // Add the new columns we added to the model so migration is safe to apply on existing DB.
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "AspNetUsers",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "AspNetUsers",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAdmin",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            // Note: IsPasswordChanged already exists in the original schema, so we do not add it here.

            // MonthlyBill table already exists in the database; skipping creation in migration.

            // TeacherAttendance table already exists in the database; skipping creation in migration.

            // WeeklyMenu table already exists in the database; skipping creation in migration.

            // Indices for MonthlyBill, TeacherAttendance, WeeklyMenu already exist in database; skipping index creation.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // revert added columns from AspNetUsers
            migrationBuilder.DropColumn(
                name: "Email",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "IsAdmin",
                table: "AspNetUsers");

            // IsPasswordChanged may have already existed; attempt to drop if present
            try
            {
                migrationBuilder.DropColumn(
                    name: "IsPasswordChanged",
                    table: "AspNetUsers");
            }
            catch
            {
                // ignore if column not present
            }

            migrationBuilder.DropTable(
                name: "MonthlyBill");

            migrationBuilder.DropTable(
                name: "TeacherAttendance");

            migrationBuilder.DropTable(
                name: "WeeklyMenu");
        }
    }
}
