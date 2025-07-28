using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PCBTracker.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddInspectionTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Inspections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProductType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SerialNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IssueDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SeverityLevel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImmediateActionTaken = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AdditionalNotes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AssembliesCompletedJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inspections", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Inspections");
        }
    }
}
