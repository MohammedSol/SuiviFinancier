using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SuiviFinancier.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccountNumber",
                table: "Accounts",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "Accounts",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "TargetAmount",
                table: "Accounts",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountNumber",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "TargetAmount",
                table: "Accounts");
        }
    }
}
