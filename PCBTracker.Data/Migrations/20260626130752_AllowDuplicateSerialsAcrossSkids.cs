using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PCBTracker.Data.Migrations
{
    /// <inheritdoc />
    public partial class AllowDuplicateSerialsAcrossSkids : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Boards",
                table: "Boards");

            migrationBuilder.DropIndex(
                name: "IX_Boards_SerialNumber",
                table: "Boards");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Boards",
                table: "Boards",
                columns: new[] { "SerialNumber", "SkidID" });

            migrationBuilder.CreateIndex(
                name: "IX_Boards_SerialNumber",
                table: "Boards",
                column: "SerialNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Boards",
                table: "Boards");

            migrationBuilder.DropIndex(
                name: "IX_Boards_SerialNumber",
                table: "Boards");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Boards",
                table: "Boards",
                column: "SerialNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Boards_SerialNumber",
                table: "Boards",
                column: "SerialNumber",
                unique: true);
        }
    }
}
