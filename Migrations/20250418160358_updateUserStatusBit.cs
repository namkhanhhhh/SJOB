using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SJOB_EXE201.Migrations
{
    /// <inheritdoc />
    public partial class updateUserStatusBit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Status",
                table: "users",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "LastName",
                table: "user_details",
                newName: "lastName");

            migrationBuilder.RenameColumn(
                name: "FirstName",
                table: "user_details",
                newName: "firstName");

            migrationBuilder.AlterColumn<bool>(
                name: "status",
                table: "users",
                type: "bit",
                maxLength: 50,
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "lastName",
                table: "user_details",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "firstName",
                table: "user_details",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "status",
                table: "users",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "lastName",
                table: "user_details",
                newName: "LastName");

            migrationBuilder.RenameColumn(
                name: "firstName",
                table: "user_details",
                newName: "FirstName");

            migrationBuilder.AlterColumn<bool>(
                name: "Status",
                table: "users",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldMaxLength: 50,
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                table: "user_details",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                table: "user_details",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);
        }
    }
}
