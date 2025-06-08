using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SJOB_EXE201.Migrations
{
    /// <inheritdoc />
    public partial class updateDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "banners");

            migrationBuilder.DropTable(
                name: "company_profiles");

            migrationBuilder.DropTable(
                name: "marketing_campaigns");

            migrationBuilder.DropTable(
                name: "payments");

            migrationBuilder.DropTable(
                name: "service_usages");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "banners",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    bid_amount = table.Column<decimal>(type: "decimal(15,2)", nullable: true),
                    click_count = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    end_date = table.Column<DateTime>(type: "datetime", nullable: true),
                    image_url = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    impression_count = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    position = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    redirect_url = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    start_date = table.Column<DateTime>(type: "datetime", nullable: true),
                    status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true, defaultValue: "pending"),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__banners__3213E83FF8839A19", x => x.id);
                    table.ForeignKey(
                        name: "FK_banners_users",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "company_profiles",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    company_description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    company_logo = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    company_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    company_size = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    company_website = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    free_posts_remaining = table.Column<int>(type: "int", nullable: true, defaultValue: 5),
                    industry = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    verified_badge = table.Column<bool>(type: "bit", nullable: true, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__company___3213E83F0FD57A10", x => x.id);
                    table.ForeignKey(
                        name: "FK_company_profiles_users",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "marketing_campaigns",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    email_template = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    end_date = table.Column<DateTime>(type: "datetime", nullable: true),
                    name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    sent_count = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    start_date = table.Column<DateTime>(type: "datetime", nullable: true),
                    status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true, defaultValue: "draft"),
                    target_count = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__marketin__3213E83F3184CE76", x => x.id);
                    table.ForeignKey(
                        name: "FK_marketing_campaigns_users",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "payments",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    amount = table.Column<decimal>(type: "decimal(15,2)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    payment_date = table.Column<DateTime>(type: "datetime", nullable: true),
                    payment_method = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true, defaultValue: "vnpay"),
                    payment_status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true, defaultValue: "pending"),
                    payment_type = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    reference_id = table.Column<int>(type: "int", nullable: true),
                    transaction_id = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    vnpay_transaction_info = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__payments__3213E83F928D54E8", x => x.id);
                    table.ForeignKey(
                        name: "FK_payments_users",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "service_usages",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    job_post_id = table.Column<int>(type: "int", nullable: false),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    end_date = table.Column<DateTime>(type: "datetime", nullable: true),
                    reference_id = table.Column<int>(type: "int", nullable: true),
                    reference_type = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    service_type = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    start_date = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true, defaultValue: "active")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__service___3213E83F58B18C5F", x => x.id);
                    table.ForeignKey(
                        name: "FK_service_usages_job_posts",
                        column: x => x.job_post_id,
                        principalTable: "job_posts",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_service_usages_users",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_banners_user_id",
                table: "banners",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "idx_company_profiles_user_id",
                table: "company_profiles",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_marketing_campaigns_user_id",
                table: "marketing_campaigns",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_payments_user_id",
                table: "payments",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_service_usages_job_post_id",
                table: "service_usages",
                column: "job_post_id");

            migrationBuilder.CreateIndex(
                name: "IX_service_usages_user_id",
                table: "service_usages",
                column: "user_id");
        }
    }
}
