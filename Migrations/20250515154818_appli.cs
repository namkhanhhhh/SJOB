using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SJOB_EXE201.Migrations
{
    /// <inheritdoc />
    public partial class appli : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ApplicationId1",
                table: "application_notes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_application_notes_ApplicationId1",
                table: "application_notes",
                column: "ApplicationId1",
                unique: true,
                filter: "[ApplicationId1] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_application_notes_applications_ApplicationId1",
                table: "application_notes",
                column: "ApplicationId1",
                principalTable: "applications",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_application_notes_applications_ApplicationId1",
                table: "application_notes");

            migrationBuilder.DropIndex(
                name: "IX_application_notes_ApplicationId1",
                table: "application_notes");

            migrationBuilder.DropColumn(
                name: "ApplicationId1",
                table: "application_notes");
        }
    }
}
