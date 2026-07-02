using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LbI.Migrations
{
    /// <inheritdoc />
    public partial class DayEndCash_Added_Voucher : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Creator",
                table: "Vouchers");

            migrationBuilder.AddColumn<int>(
                name: "CreatorId",
                table: "Vouchers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "DayEndCash",
                table: "Vouchers",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "Vouchers");

            migrationBuilder.DropColumn(
                name: "DayEndCash",
                table: "Vouchers");

            migrationBuilder.AddColumn<string>(
                name: "Creator",
                table: "Vouchers",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
