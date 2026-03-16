using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyApp.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddTableTypeToWeeklyMaintenance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WeeklyMaintenanceEntries_WeekStartDate_DayOfWeek_ShiftType_SlotIndex",
                table: "WeeklyMaintenanceEntries");

            migrationBuilder.AddColumn<byte>(
                name: "TableType",
                table: "WeeklyMaintenanceEntries",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)1);

            migrationBuilder.CreateIndex(
                name: "IX_WeeklyMaintenanceEntries_TableType_WeekStartDate_DayOfWeek_ShiftType_SlotIndex",
                table: "WeeklyMaintenanceEntries",
                columns: new[] { "TableType", "WeekStartDate", "DayOfWeek", "ShiftType", "SlotIndex" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WeeklyMaintenanceEntries_TableType_WeekStartDate_DayOfWeek_ShiftType_SlotIndex",
                table: "WeeklyMaintenanceEntries");

            migrationBuilder.DropColumn(
                name: "TableType",
                table: "WeeklyMaintenanceEntries");

            migrationBuilder.CreateIndex(
                name: "IX_WeeklyMaintenanceEntries_WeekStartDate_DayOfWeek_ShiftType_SlotIndex",
                table: "WeeklyMaintenanceEntries",
                columns: new[] { "WeekStartDate", "DayOfWeek", "ShiftType", "SlotIndex" },
                unique: true);
        }
    }
}
