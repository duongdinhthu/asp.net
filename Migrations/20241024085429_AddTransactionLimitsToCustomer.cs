using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ATMManagementApplication.Migrations
{
    /// <inheritdoc />
    public partial class AddTransactionLimitsToCustomer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DailyLimit",
                table: "Customer",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "TransactionCountLimit",
                table: "Customer",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DailyLimit",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "TransactionCountLimit",
                table: "Customer");
        }
    }
}
