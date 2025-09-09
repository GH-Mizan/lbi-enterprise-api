using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LbI.Migrations
{
    /// <inheritdoc />
    public partial class Default_Added_To_DueReceivedHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Default",
                table: "DueReceivedHistories",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Default",
                table: "DuePaymentHistories",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Default",
                table: "DueReceivedHistories");

            migrationBuilder.DropColumn(
                name: "Default",
                table: "DuePaymentHistories");
        }
    }
}
