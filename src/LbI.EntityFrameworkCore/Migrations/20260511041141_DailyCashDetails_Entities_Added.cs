using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LbI.Migrations
{
    /// <inheritdoc />
    public partial class DailyCashDetails_Entities_Added : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DayEndAdvanceBalance",
                table: "DailyCashes");

            migrationBuilder.RenameColumn(
                name: "TotalVirtualTransaction",
                table: "DailyCashes",
                newName: "TotalIncome");

            migrationBuilder.RenameColumn(
                name: "TotalActualIncome",
                table: "DailyCashes",
                newName: "TotalDayEndCash");

            migrationBuilder.RenameColumn(
                name: "Metadata",
                table: "DailyCashes",
                newName: "Remarks");

            migrationBuilder.RenameColumn(
                name: "DayStartCashBalance",
                table: "DailyCashes",
                newName: "TotalAdvance");

            migrationBuilder.RenameColumn(
                name: "DayStartAdvanceBalance",
                table: "DailyCashes",
                newName: "PaperBalance");

            migrationBuilder.RenameColumn(
                name: "DayEndCashBalance",
                table: "DailyCashes",
                newName: "ActualBalance");

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "DailyCashes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "DailyCashAdvanceDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DailyCashId = table.Column<int>(type: "int", nullable: false),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Head = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
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
                    table.PrimaryKey("PK_DailyCashAdvanceDetails", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DailyCashDayEndDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DailyCashId = table.Column<int>(type: "int", nullable: false),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Head = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
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
                    table.PrimaryKey("PK_DailyCashDayEndDetails", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DailyCashExpenseDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DailyCashId = table.Column<int>(type: "int", nullable: false),
                    ExpenseId = table.Column<int>(type: "int", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Head = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
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
                    table.PrimaryKey("PK_DailyCashExpenseDetails", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DailyCashIncomeDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DailyCashId = table.Column<int>(type: "int", nullable: false),
                    IncomeId = table.Column<int>(type: "int", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Head = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
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
                    table.PrimaryKey("PK_DailyCashIncomeDetails", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DailyCashAdvanceDetails");

            migrationBuilder.DropTable(
                name: "DailyCashDayEndDetails");

            migrationBuilder.DropTable(
                name: "DailyCashExpenseDetails");

            migrationBuilder.DropTable(
                name: "DailyCashIncomeDetails");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "DailyCashes");

            migrationBuilder.RenameColumn(
                name: "TotalIncome",
                table: "DailyCashes",
                newName: "TotalVirtualTransaction");

            migrationBuilder.RenameColumn(
                name: "TotalDayEndCash",
                table: "DailyCashes",
                newName: "TotalActualIncome");

            migrationBuilder.RenameColumn(
                name: "TotalAdvance",
                table: "DailyCashes",
                newName: "DayStartCashBalance");

            migrationBuilder.RenameColumn(
                name: "Remarks",
                table: "DailyCashes",
                newName: "Metadata");

            migrationBuilder.RenameColumn(
                name: "PaperBalance",
                table: "DailyCashes",
                newName: "DayStartAdvanceBalance");

            migrationBuilder.RenameColumn(
                name: "ActualBalance",
                table: "DailyCashes",
                newName: "DayEndCashBalance");

            migrationBuilder.AddColumn<decimal>(
                name: "DayEndAdvanceBalance",
                table: "DailyCashes",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
