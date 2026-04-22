using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedicalClinic.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixExternalDocumentCascade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExternalDocument_AspNetUsers_UploadedById",
                table: "ExternalDocument");

            migrationBuilder.DropIndex(
                name: "IX_ExternalDocument_UploadedById",
                table: "ExternalDocument");

            migrationBuilder.DropColumn(
                name: "UploadedById",
                table: "ExternalDocument");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UploadedById",
                table: "ExternalDocument",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalDocument_UploadedById",
                table: "ExternalDocument",
                column: "UploadedById");

            migrationBuilder.AddForeignKey(
                name: "FK_ExternalDocument_AspNetUsers_UploadedById",
                table: "ExternalDocument",
                column: "UploadedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
