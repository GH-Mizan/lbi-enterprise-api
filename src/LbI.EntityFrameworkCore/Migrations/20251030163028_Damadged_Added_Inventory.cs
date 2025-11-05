using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LbI.Migrations
{
    /// <inheritdoc />
    public partial class Damadged_Added_Inventory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProductName",
                table: "Inventories");

            migrationBuilder.AddColumn<bool>(
                name: "Damadged",
                table: "Inventories",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Damadged",
                table: "Inventories");

            migrationBuilder.AddColumn<string>(
                name: "ProductName",
                table: "Inventories",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
