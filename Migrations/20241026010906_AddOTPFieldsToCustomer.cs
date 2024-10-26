using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ATMManagementApplication.Migrations
{
    /// <inheritdoc />
    public partial class AddOTPFieldsToCustomer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OTP",
                table: "Customer",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "OTPExpiration",
                table: "Customer",
                type: "datetime(6)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OTP",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "OTPExpiration",
                table: "Customer");
        }
    }
}
