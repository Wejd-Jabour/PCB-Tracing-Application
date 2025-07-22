using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PCBTracker.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddNewTablesAndModifySkid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "designatedType",
                table: "Skids",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "designatedType",
                table: "Skids");
        }
    }
}
