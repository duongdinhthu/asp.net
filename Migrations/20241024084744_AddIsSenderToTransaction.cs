using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ATMManagementApplication.Migrations
{
    /// <inheritdoc />
    public partial class AddIsSenderToTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSender",
                table: "Transaction",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSender",
                table: "Transaction");
        }
    }
}
