using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PMS.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerMobileNoAndMobileNo2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MobileNo",
                table: "Customers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MobileNo2",
                table: "Customers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            // Legacy rows: ensure Phone is non-empty before NOT NULL
            migrationBuilder.Sql("""
                UPDATE [Customers]
                SET [Phone] = N'00000000000'
                WHERE [Phone] IS NULL OR LTRIM(RTRIM([Phone])) = N'';
                """);

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "Customers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            // MobileNo: copy from Phone, then ensure digit-normalized value differs from Phone when they match
            migrationBuilder.Sql("""
                UPDATE [Customers]
                SET [MobileNo] = [Phone]
                WHERE [MobileNo] IS NULL;

                UPDATE c
                SET c.[MobileNo] = LEFT(c.[Phone], 49) + N'9'
                FROM [Customers] c
                WHERE REPLACE(REPLACE(REPLACE(ISNULL(c.[Phone], N''), N'+', N''), N'-', N''), N' ', N'')
                    = REPLACE(REPLACE(REPLACE(ISNULL(c.[MobileNo], N''), N'+', N''), N'-', N''), N' ', N'')
                  AND LEN(REPLACE(REPLACE(REPLACE(ISNULL(c.[Phone], N''), N'+', N''), N'-', N''), N' ', N'')) > 0;
                """);

            migrationBuilder.AlterColumn<string>(
                name: "MobileNo",
                table: "Customers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MobileNo2",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "MobileNo",
                table: "Customers");

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "Customers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);
        }
    }
}
