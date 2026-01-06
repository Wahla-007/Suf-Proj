using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mess_management.Migrations
{
    /// <inheritdoc />
    public partial class AddMealNamesToWeeklyPlanDay : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BreakfastName",
                table: "WeeklyPlanDays",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Breakfast");

            migrationBuilder.AddColumn<string>(
                name: "DinnerName",
                table: "WeeklyPlanDays",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Dinner");

            migrationBuilder.AddColumn<string>(
                name: "LunchName",
                table: "WeeklyPlanDays",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Lunch");

            migrationBuilder.AlterColumn<decimal>(
                name: "BreakfastRate",
                table: "WeeklyMenu",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,0)",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BreakfastName",
                table: "WeeklyPlanDays");

            migrationBuilder.DropColumn(
                name: "DinnerName",
                table: "WeeklyPlanDays");

            migrationBuilder.DropColumn(
                name: "LunchName",
                table: "WeeklyPlanDays");

            migrationBuilder.AlterColumn<decimal>(
                name: "BreakfastRate",
                table: "WeeklyMenu",
                type: "decimal(18,0)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2,
                oldNullable: true);
        }
    }
}
