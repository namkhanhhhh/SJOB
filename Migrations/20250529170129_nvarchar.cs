using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SJOB_EXE201.Migrations
{
    /// <inheritdoc />
    public partial class nvarchar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "job_type",
                table: "job_posts",
                type: "NVARCHAR(255)",
                unicode: false,
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(20)",
                oldUnicode: false,
                oldMaxLength: 20,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "job_type",
                table: "job_posts",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "NVARCHAR(255)",
                oldUnicode: false,
                oldMaxLength: 20,
                oldNullable: true);
        }
    }
}
