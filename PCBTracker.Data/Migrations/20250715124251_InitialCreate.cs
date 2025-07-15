using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PCBTracker.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Skids",
                columns: table => new
                {
                    SkidID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SkidName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Skids", x => x.SkidID);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserID);
                });

            migrationBuilder.CreateTable(
                name: "Boards",
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
                    table.PrimaryKey("PK_Boards", x => x.SerialNumber);
                    table.ForeignKey(
                        name: "FK_Boards_Skids_SkidID",
                        column: x => x.SkidID,
                        principalTable: "Skids",
                        principalColumn: "SkidID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Boards_SerialNumber",
                table: "Boards",
                column: "SerialNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Boards_SkidID",
                table: "Boards",
                column: "SkidID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Boards");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Skids");
        }
    }
}
