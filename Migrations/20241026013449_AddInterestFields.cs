using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ATMManagementApplication.Migrations
{
    /// <inheritdoc />
    public partial class AddInterestFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "InterestRate",
                table: "Customer",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastInterestApplied",
                table: "Customer",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InterestRate",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "LastInterestApplied",
                table: "Customer");
        }
    }
}
