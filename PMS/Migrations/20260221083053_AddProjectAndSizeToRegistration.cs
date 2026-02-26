using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PMS.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectAndSizeToRegistration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProjectID",
                table: "Registration",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Size",
                table: "Registration",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Registration_ProjectID",
                table: "Registration",
                column: "ProjectID");

            migrationBuilder.AddForeignKey(
                name: "FK_Registration_Projects_ProjectID",
                table: "Registration",
                column: "ProjectID",
                principalTable: "Projects",
                principalColumn: "ProjectID",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Registration_Projects_ProjectID",
                table: "Registration");

            migrationBuilder.DropIndex(
                name: "IX_Registration_ProjectID",
                table: "Registration");

            migrationBuilder.DropColumn(
                name: "ProjectID",
                table: "Registration");

            migrationBuilder.DropColumn(
                name: "Size",
                table: "Registration");
        }
    }
}
