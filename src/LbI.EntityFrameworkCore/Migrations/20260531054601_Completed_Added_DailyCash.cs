using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LbI.Migrations
{
    /// <inheritdoc />
    public partial class Completed_Added_DailyCash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Completed",
                table: "DailyCashes",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Completed",
                table: "DailyCashes");
        }
    }
}
