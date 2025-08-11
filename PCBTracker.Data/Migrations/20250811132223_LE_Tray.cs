using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PCBTracker.Data.Migrations
{
    /// <inheritdoc />
    public partial class LE_Tray : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LE_Tray",
                columns: table => new
                {
                    SerialNumber = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PartNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BoardType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SkidID = table.Column<int>(type: "int", nullable: false),
                    PrepDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ShipDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsShipped = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LE_Tray", x => x.SerialNumber);
                    table.ForeignKey(
                        name: "FK_LE_Tray_Skids_SkidID",
                        column: x => x.SkidID,
                        principalTable: "Skids",
                        principalColumn: "SkidID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LE_Tray_SerialNumber",
                table: "LE_Tray",
                column: "SerialNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LE_Tray_SkidID",
                table: "LE_Tray",
                column: "SkidID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LE_Tray");
        }
    }
}
