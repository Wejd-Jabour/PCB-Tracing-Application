using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PCBTracker.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddNewTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LE",
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
                    table.PrimaryKey("PK_LE", x => x.SerialNumber);
                    table.ForeignKey(
                        name: "FK_LE_Skids_SkidID",
                        column: x => x.SkidID,
                        principalTable: "Skids",
                        principalColumn: "SkidID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LE_Upgrade",
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
                    table.PrimaryKey("PK_LE_Upgrade", x => x.SerialNumber);
                    table.ForeignKey(
                        name: "FK_LE_Upgrade_Skids_SkidID",
                        column: x => x.SkidID,
                        principalTable: "Skids",
                        principalColumn: "SkidID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SAD",
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
                    table.PrimaryKey("PK_SAD", x => x.SerialNumber);
                    table.ForeignKey(
                        name: "FK_SAD_Skids_SkidID",
                        column: x => x.SkidID,
                        principalTable: "Skids",
                        principalColumn: "SkidID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SAD_Upgrade",
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
                    table.PrimaryKey("PK_SAD_Upgrade", x => x.SerialNumber);
                    table.ForeignKey(
                        name: "FK_SAD_Upgrade_Skids_SkidID",
                        column: x => x.SkidID,
                        principalTable: "Skids",
                        principalColumn: "SkidID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SAT",
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
                    table.PrimaryKey("PK_SAT", x => x.SerialNumber);
                    table.ForeignKey(
                        name: "FK_SAT_Skids_SkidID",
                        column: x => x.SkidID,
                        principalTable: "Skids",
                        principalColumn: "SkidID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SAT_Upgrade",
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
                    table.PrimaryKey("PK_SAT_Upgrade", x => x.SerialNumber);
                    table.ForeignKey(
                        name: "FK_SAT_Upgrade_Skids_SkidID",
                        column: x => x.SkidID,
                        principalTable: "Skids",
                        principalColumn: "SkidID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LE_SkidID",
                table: "LE",
                column: "SkidID");

            migrationBuilder.CreateIndex(
                name: "IX_LE_Upgrade_SkidID",
                table: "LE_Upgrade",
                column: "SkidID");

            migrationBuilder.CreateIndex(
                name: "IX_SAD_SkidID",
                table: "SAD",
                column: "SkidID");

            migrationBuilder.CreateIndex(
                name: "IX_SAD_Upgrade_SkidID",
                table: "SAD_Upgrade",
                column: "SkidID");

            migrationBuilder.CreateIndex(
                name: "IX_SAT_SkidID",
                table: "SAT",
                column: "SkidID");

            migrationBuilder.CreateIndex(
                name: "IX_SAT_Upgrade_SkidID",
                table: "SAT_Upgrade",
                column: "SkidID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LE");

            migrationBuilder.DropTable(
                name: "LE_Upgrade");

            migrationBuilder.DropTable(
                name: "SAD");

            migrationBuilder.DropTable(
                name: "SAD_Upgrade");

            migrationBuilder.DropTable(
                name: "SAT");

            migrationBuilder.DropTable(
                name: "SAT_Upgrade");
        }
    }
}
