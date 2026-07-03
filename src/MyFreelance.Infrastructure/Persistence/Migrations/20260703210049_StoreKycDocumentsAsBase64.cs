using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyFreelance.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class StoreKycDocumentsAsBase64 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StoredPath",
                table: "KycDocuments",
                newName: "FileContentBase64");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FileContentBase64",
                table: "KycDocuments",
                newName: "StoredPath");
        }
    }
}
