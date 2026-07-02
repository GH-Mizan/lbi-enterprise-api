using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LbI.Migrations
{
    /// <inheritdoc />
    public partial class Uid_Added_DailyCaseDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Uid",
                table: "DailyCashIncomeDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Uid",
                table: "DailyCashExpenseDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Uid",
                table: "DailyCashDayEndDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Uid",
                table: "DailyCashAdvanceDetails",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Uid",
                table: "DailyCashIncomeDetails");

            migrationBuilder.DropColumn(
                name: "Uid",
                table: "DailyCashExpenseDetails");

            migrationBuilder.DropColumn(
                name: "Uid",
                table: "DailyCashDayEndDetails");

            migrationBuilder.DropColumn(
                name: "Uid",
                table: "DailyCashAdvanceDetails");
        }
    }
}
