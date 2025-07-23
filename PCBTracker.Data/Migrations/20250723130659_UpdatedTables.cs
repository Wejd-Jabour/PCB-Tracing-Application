using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PCBTracker.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_SAT_Upgrade_SerialNumber",
                table: "SAT_Upgrade",
                column: "SerialNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SAT_SerialNumber",
                table: "SAT",
                column: "SerialNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SAD_Upgrade_SerialNumber",
                table: "SAD_Upgrade",
                column: "SerialNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SAD_SerialNumber",
                table: "SAD",
                column: "SerialNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LE_Upgrade_SerialNumber",
                table: "LE_Upgrade",
                column: "SerialNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LE_SerialNumber",
                table: "LE",
                column: "SerialNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SAT_Upgrade_SerialNumber",
                table: "SAT_Upgrade");

            migrationBuilder.DropIndex(
                name: "IX_SAT_SerialNumber",
                table: "SAT");

            migrationBuilder.DropIndex(
                name: "IX_SAD_Upgrade_SerialNumber",
                table: "SAD_Upgrade");

            migrationBuilder.DropIndex(
                name: "IX_SAD_SerialNumber",
                table: "SAD");

            migrationBuilder.DropIndex(
                name: "IX_LE_Upgrade_SerialNumber",
                table: "LE_Upgrade");

            migrationBuilder.DropIndex(
                name: "IX_LE_SerialNumber",
                table: "LE");
        }
    }
}
