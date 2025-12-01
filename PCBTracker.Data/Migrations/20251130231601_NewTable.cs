using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PCBTracker.Data.Migrations
{
    /// <inheritdoc />
    public partial class NewTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MaraHollyOrderLine",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrderNbr = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CustomerOrderNbr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LineNbr = table.Column<int>(type: "int", nullable: false),
                    InventoryId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrderQty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OpenQty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RequestDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProcessingStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastUpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaraHollyOrderLine", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MaraHollyOrderLine_OrderNbr_LineNbr",
                table: "MaraHollyOrderLine",
                columns: new[] { "OrderNbr", "LineNbr" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MaraHollyOrderLine");
        }
    }
}
