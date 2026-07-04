using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyFreelance.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddApplicationLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApplicationLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FunctionName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationLogs_CreatedAt",
                table: "ApplicationLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationLogs_Type",
                table: "ApplicationLogs",
                column: "Type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplicationLogs");
        }
    }
}
