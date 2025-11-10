using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PMS.Migrations
{
    /// <inheritdoc />
    public partial class AddNationalityAndKinDocumentsToCustomer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Nationality",
                table: "Customers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NomineeNICDocumentPath",
                table: "Customers",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NomineePicturePath",
                table: "Customers",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Nationality",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "NomineeNICDocumentPath",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "NomineePicturePath",
                table: "Customers");
        }
    }
}
