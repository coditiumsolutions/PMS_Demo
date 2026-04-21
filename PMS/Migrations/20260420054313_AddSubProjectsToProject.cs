using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PMS.Migrations
{
    /// <inheritdoc />
    public partial class AddSubProjectsToProject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH('Projects', 'SubProjects') IS NULL
BEGIN
    ALTER TABLE [Projects] ADD [SubProjects] nvarchar(1000) NULL;
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH('Projects', 'SubProjects') IS NOT NULL
BEGIN
    ALTER TABLE [Projects] DROP COLUMN [SubProjects];
END
");
        }
    }
}
