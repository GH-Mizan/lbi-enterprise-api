using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LbI.Migrations
{
    /// <inheritdoc />
    public partial class TotalProfit_Adedd_To_SalesDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "TotalProfit",
                table: "SalesDetials",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalProfit",
                table: "SalesDetials");
        }
    }
}
