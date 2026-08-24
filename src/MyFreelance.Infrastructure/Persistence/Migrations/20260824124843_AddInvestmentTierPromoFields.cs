using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyFreelance.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddInvestmentTierPromoFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "InvestmentTiers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PromoBannerText",
                table: "InvestmentTiers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PromoEndUtc",
                table: "InvestmentTiers",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "InvestmentTiers");

            migrationBuilder.DropColumn(
                name: "PromoBannerText",
                table: "InvestmentTiers");

            migrationBuilder.DropColumn(
                name: "PromoEndUtc",
                table: "InvestmentTiers");
        }
    }
}
