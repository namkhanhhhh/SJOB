using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SJOB_EXE201.Migrations
{
    /// <inheritdoc />
    public partial class AddUserPostCredits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_wishlists_users_UserId1",
                table: "wishlists");

            migrationBuilder.DropIndex(
                name: "IX_wishlists_UserId1",
                table: "wishlists");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "wishlists");

            migrationBuilder.DropColumn(
                name: "job_post_id",
                table: "service_orders");

            migrationBuilder.AddColumn<int>(
                name: "diamond_posts_applied",
                table: "service_orders",
                type: "int",
                nullable: true,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "gold_posts_applied",
                table: "service_orders",
                type: "int",
                nullable: true,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "post_credits_applied",
                table: "service_orders",
                type: "bit",
                nullable: true,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "silver_posts_applied",
                table: "service_orders",
                type: "int",
                nullable: true,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "requirements",
                table: "job_posts",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "benefits",
                table: "job_posts",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "diamond_posts_included",
                table: "additional_services",
                type: "int",
                nullable: true,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "gold_posts_included",
                table: "additional_services",
                type: "int",
                nullable: true,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "silver_posts_included",
                table: "additional_services",
                type: "int",
                nullable: true,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "user_post_credits",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    silver_posts_available = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    gold_posts_available = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    diamond_posts_available = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    last_updated = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_post_credits", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_post_credits_users",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_user_post_credits_user_id",
                table: "user_post_credits",
                column: "user_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_post_credits");

            migrationBuilder.DropColumn(
                name: "diamond_posts_applied",
                table: "service_orders");

            migrationBuilder.DropColumn(
                name: "gold_posts_applied",
                table: "service_orders");

            migrationBuilder.DropColumn(
                name: "post_credits_applied",
                table: "service_orders");

            migrationBuilder.DropColumn(
                name: "silver_posts_applied",
                table: "service_orders");

            migrationBuilder.DropColumn(
                name: "diamond_posts_included",
                table: "additional_services");

            migrationBuilder.DropColumn(
                name: "gold_posts_included",
                table: "additional_services");

            migrationBuilder.DropColumn(
                name: "silver_posts_included",
                table: "additional_services");

            migrationBuilder.AddColumn<int>(
                name: "UserId1",
                table: "wishlists",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "job_post_id",
                table: "service_orders",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "requirements",
                table: "job_posts",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "benefits",
                table: "job_posts",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_wishlists_UserId1",
                table: "wishlists",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_wishlists_users_UserId1",
                table: "wishlists",
                column: "UserId1",
                principalTable: "users",
                principalColumn: "id");
        }
    }
}
