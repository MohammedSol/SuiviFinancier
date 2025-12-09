using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SuiviFinancier.Migrations
{
    /// <inheritdoc />
    public partial class AddTransactionReceipts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReceiptPath",
                table: "Transactions",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReceiptPath",
                table: "Transactions");
        }
    }
}
