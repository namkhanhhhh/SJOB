using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SJOB_EXE201.Migrations
{
    /// <inheritdoc />
    public partial class AddWishlistTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "wishlists",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    job_post_id = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    JobPostId1 = table.Column<int>(type: "int", nullable: true),
                    UserId1 = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__wishlist__3213E83F", x => x.id);
                    table.ForeignKey(
                        name: "FK_wishlists_job_posts",
                        column: x => x.job_post_id,
                        principalTable: "job_posts",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_wishlists_job_posts_JobPostId1",
                        column: x => x.JobPostId1,
                        principalTable: "job_posts",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_wishlists_users",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_wishlists_users_UserId1",
                        column: x => x.UserId1,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_wishlists_job_post_id",
                table: "wishlists",
                column: "job_post_id");

            migrationBuilder.CreateIndex(
                name: "IX_wishlists_JobPostId1",
                table: "wishlists",
                column: "JobPostId1");

            migrationBuilder.CreateIndex(
                name: "IX_wishlists_UserId1",
                table: "wishlists",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "UQ__wishlists__user_job",
                table: "wishlists",
                columns: new[] { "user_id", "job_post_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "wishlists");
        }
    }
}
