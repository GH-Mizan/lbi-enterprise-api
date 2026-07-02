using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LbI.Migrations
{
    /// <inheritdoc />
    public partial class MtmKey_Added_DailyCases : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MtmKey",
                table: "DailyCashIncomeDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MtmKey",
                table: "DailyCashDayEndDetails",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MtmKey",
                table: "DailyCashIncomeDetails");

            migrationBuilder.DropColumn(
                name: "MtmKey",
                table: "DailyCashDayEndDetails");
        }
    }
}
