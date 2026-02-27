using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyApp.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddManevraEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Convoys",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Convoys", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tracks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Zone = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tracks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Wagons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WagonNumber = table.Column<int>(type: "int", nullable: false),
                    Line = table.Column<byte>(type: "tinyint", nullable: false),
                    IsOnlyMiddle = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    ConvoyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wagons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Wagons_Convoys_ConvoyId",
                        column: x => x.ConvoyId,
                        principalTable: "Convoys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "CleanupHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WagonId = table.Column<int>(type: "int", nullable: false),
                    CleanupDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CleanupHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CleanupHistories_Wagons_WagonId",
                        column: x => x.WagonId,
                        principalTable: "Wagons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrackSlots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TrackId = table.Column<int>(type: "int", nullable: false),
                    SectionType = table.Column<byte>(type: "tinyint", nullable: false),
                    SlotIndex = table.Column<byte>(type: "tinyint", nullable: false),
                    WagonId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackSlots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrackSlots_Tracks_TrackId",
                        column: x => x.TrackId,
                        principalTable: "Tracks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TrackSlots_Wagons_WagonId",
                        column: x => x.WagonId,
                        principalTable: "Wagons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "WagonTransfers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WagonId = table.Column<int>(type: "int", nullable: false),
                    FromSlotId = table.Column<int>(type: "int", nullable: false),
                    ToSlotId = table.Column<int>(type: "int", nullable: false),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WagonTransfers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WagonTransfers_TrackSlots_FromSlotId",
                        column: x => x.FromSlotId,
                        principalTable: "TrackSlots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WagonTransfers_TrackSlots_ToSlotId",
                        column: x => x.ToSlotId,
                        principalTable: "TrackSlots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WagonTransfers_Wagons_WagonId",
                        column: x => x.WagonId,
                        principalTable: "Wagons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CleanupHistories_WagonId",
                table: "CleanupHistories",
                column: "WagonId");

            migrationBuilder.CreateIndex(
                name: "IX_TrackSlots_TrackId",
                table: "TrackSlots",
                column: "TrackId");

            migrationBuilder.CreateIndex(
                name: "IX_TrackSlots_WagonId",
                table: "TrackSlots",
                column: "WagonId",
                unique: true,
                filter: "[WagonId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Wagons_ConvoyId",
                table: "Wagons",
                column: "ConvoyId");

            migrationBuilder.CreateIndex(
                name: "IX_WagonTransfers_FromSlotId",
                table: "WagonTransfers",
                column: "FromSlotId");

            migrationBuilder.CreateIndex(
                name: "IX_WagonTransfers_ToSlotId",
                table: "WagonTransfers",
                column: "ToSlotId");

            migrationBuilder.CreateIndex(
                name: "IX_WagonTransfers_WagonId",
                table: "WagonTransfers",
                column: "WagonId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CleanupHistories");

            migrationBuilder.DropTable(
                name: "WagonTransfers");

            migrationBuilder.DropTable(
                name: "TrackSlots");

            migrationBuilder.DropTable(
                name: "Tracks");

            migrationBuilder.DropTable(
                name: "Wagons");

            migrationBuilder.DropTable(
                name: "Convoys");
        }
    }
}
