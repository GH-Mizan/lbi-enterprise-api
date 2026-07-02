using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LbI.Migrations
{
    /// <inheritdoc />
    public partial class Remove_TenantId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "DailyCashes");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "AdditionalPartiesAdvances");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "DailyCashes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "AdditionalPartiesAdvances",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
