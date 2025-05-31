using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SJOB_EXE201.Migrations
{
    /// <inheritdoc />
    public partial class UpdateColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_application_notes_ApplicationId1",
                table: "application_notes");

            migrationBuilder.AlterColumn<string>(
                name: "experience_level",
                table: "job_posts",
                type: "NVARCHAR(255)",
                unicode: false,
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldUnicode: false,
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_application_notes_ApplicationId1",
                table: "application_notes",
                column: "ApplicationId1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_application_notes_ApplicationId1",
                table: "application_notes");

            migrationBuilder.AlterColumn<string>(
                name: "experience_level",
                table: "job_posts",
                type: "varchar(100)",
                unicode: false,
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "NVARCHAR(255)",
                oldUnicode: false,
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_application_notes_ApplicationId1",
                table: "application_notes",
                column: "ApplicationId1",
                unique: true,
                filter: "[ApplicationId1] IS NOT NULL");
        }
    }
}
