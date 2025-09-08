using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LbI.Migrations
{
    /// <inheritdoc />
    public partial class Remove_DailyCashDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DailyCashDetails");

            migrationBuilder.AddColumn<string>(
                name: "Metadata",
                table: "DailyCashes",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Metadata",
                table: "DailyCashes");

            migrationBuilder.CreateTable(
                name: "DailyCashDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActualIncome = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    ActualIncomeHead = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Advance = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    AdvanceHead = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: true),
                    DailyCashId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DayEndCash = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    DayEndCashHead = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Expense = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    ExpenseHead = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    VirtualIncomeHead = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VirtualTransaction = table.Column<decimal>(type: "decimal(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyCashDetails", x => x.Id);
                });
        }
    }
}
