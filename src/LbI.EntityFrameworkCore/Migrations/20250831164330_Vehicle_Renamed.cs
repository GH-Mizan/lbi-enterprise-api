using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LbI.Migrations
{
    /// <inheritdoc />
    public partial class Vehicle_Renamed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "VehicleNumber",
                table: "Vehicles",
                newName: "StockPointNumber");

            migrationBuilder.RenameColumn(
                name: "VehicleId",
                table: "Inventories",
                newName: "StockPointId");

            migrationBuilder.AddColumn<int>(
                name: "StockPointType",
                table: "Vehicles",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StockPointType",
                table: "Vehicles");

            migrationBuilder.RenameColumn(
                name: "StockPointNumber",
                table: "Vehicles",
                newName: "VehicleNumber");

            migrationBuilder.RenameColumn(
                name: "StockPointId",
                table: "Inventories",
                newName: "VehicleId");
        }
    }
}
