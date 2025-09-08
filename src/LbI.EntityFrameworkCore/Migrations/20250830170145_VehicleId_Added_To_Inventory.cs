using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LbI.Migrations
{
    /// <inheritdoc />
    public partial class VehicleId_Added_To_Inventory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BillNumber",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "BillNumber",
                table: "Purchases");

            migrationBuilder.AddColumn<int>(
                name: "VehicleId",
                table: "Sales",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "VehicleId",
                table: "Purchases",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "VehicleId",
                table: "Inventories",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VehicleId",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "VehicleId",
                table: "Purchases");

            migrationBuilder.DropColumn(
                name: "VehicleId",
                table: "Inventories");

            migrationBuilder.AddColumn<string>(
                name: "BillNumber",
                table: "Sales",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BillNumber",
                table: "Purchases",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
