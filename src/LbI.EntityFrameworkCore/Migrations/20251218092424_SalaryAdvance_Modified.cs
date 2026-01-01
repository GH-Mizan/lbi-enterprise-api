using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LbI.Migrations
{
    /// <inheritdoc />
    public partial class SalaryAdvance_Modified : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Loan",
                table: "SalaryAdvances",
                newName: "LoanToCompany");

            migrationBuilder.RenameColumn(
                name: "Loan",
                table: "SalaryAdvanceHistories",
                newName: "LoanToCompany");

            migrationBuilder.AddColumn<int>(
                name: "LoanFromCompany",
                table: "SalaryAdvances",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LoanFromCompany",
                table: "SalaryAdvanceHistories",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LoanFromCompany",
                table: "SalaryAdvances");

            migrationBuilder.DropColumn(
                name: "LoanFromCompany",
                table: "SalaryAdvanceHistories");

            migrationBuilder.RenameColumn(
                name: "LoanToCompany",
                table: "SalaryAdvances",
                newName: "Loan");

            migrationBuilder.RenameColumn(
                name: "LoanToCompany",
                table: "SalaryAdvanceHistories",
                newName: "Loan");
        }
    }
}
