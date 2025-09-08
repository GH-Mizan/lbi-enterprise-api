using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LbI.Migrations
{
    /// <inheritdoc />
    public partial class DailyCash_Added : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DailyCashDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DailyCashId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ActualIncomeHead = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ActualIncome = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    VirtualIncomeHead = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VirtualTransaction = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    AdvanceHead = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Advance = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    ExpenseHead = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Expense = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    DayEndCashHead = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DayEndCash = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyCashDetails", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DailyCashes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalActualIncome = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    TotalVirtualTransaction = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    TotalExpense = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    DayStartCashBalance = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    DayStartAdvanceBalance = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    DayEndCashBalance = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    DayEndAdvanceBalance = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Difference = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyCashes", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DailyCashDetails");

            migrationBuilder.DropTable(
                name: "DailyCashes");
        }
    }
}
