using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyApp.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddWeeklyMaintenanceEntries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WeeklyMaintenanceEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WagonId = table.Column<int>(type: "int", nullable: false),
                    WeekStartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    DayOfWeek = table.Column<byte>(type: "tinyint", nullable: false),
                    ShiftType = table.Column<byte>(type: "tinyint", nullable: false),
                    SlotIndex = table.Column<byte>(type: "tinyint", nullable: false),
                    Priority = table.Column<byte>(type: "tinyint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeeklyMaintenanceEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WeeklyMaintenanceEntries_Wagons_WagonId",
                        column: x => x.WagonId,
                        principalTable: "Wagons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WeeklyMaintenanceEntries_WagonId",
                table: "WeeklyMaintenanceEntries",
                column: "WagonId");

            migrationBuilder.CreateIndex(
                name: "IX_WeeklyMaintenanceEntries_WeekStartDate_DayOfWeek_ShiftType_SlotIndex",
                table: "WeeklyMaintenanceEntries",
                columns: new[] { "WeekStartDate", "DayOfWeek", "ShiftType", "SlotIndex" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WeeklyMaintenanceEntries");
        }
    }
}
