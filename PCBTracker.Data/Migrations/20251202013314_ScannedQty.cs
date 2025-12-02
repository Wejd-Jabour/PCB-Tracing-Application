using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PCBTracker.Data.Migrations
{
    /// <inheritdoc />
    public partial class ScannedQty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ScannedQty",
                table: "MaraHollyOrderLine",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ScannedQty",
                table: "MaraHollyOrderLine");
        }
    }
}
