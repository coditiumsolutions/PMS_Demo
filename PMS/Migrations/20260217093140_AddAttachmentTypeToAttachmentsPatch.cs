using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PMS.Migrations
{
    /// <inheritdoc />
    public partial class AddAttachmentTypeToAttachmentsPatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[Attachments]', N'U') IS NOT NULL
AND NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[Attachments]')
      AND name = N'AttachmentType'
)
BEGIN
    ALTER TABLE [dbo].[Attachments]
    ADD [AttachmentType] NVARCHAR(50) NULL;
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[Attachments]', N'U') IS NOT NULL
AND EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[Attachments]')
      AND name = N'AttachmentType'
)
BEGIN
    ALTER TABLE [dbo].[Attachments]
    DROP COLUMN [AttachmentType];
END
");
        }
    }
}
