using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LbI.Migrations
{
    /// <inheritdoc />
    public partial class VehicleId_Renamed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "VehicleId",
                table: "Sales",
                newName: "StockPointId");

            migrationBuilder.RenameColumn(
                name: "VehicleId",
                table: "Purchases",
                newName: "StockPointId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StockPointId",
                table: "Sales",
                newName: "VehicleId");

            migrationBuilder.RenameColumn(
                name: "StockPointId",
                table: "Purchases",
                newName: "VehicleId");
        }
    }
}
