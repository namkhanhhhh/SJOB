using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SJOB_EXE201.Migrations
{
    /// <inheritdoc />
    public partial class TenMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "additional_services",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    price = table.Column<decimal>(type: "decimal(15,2)", nullable: false),
                    duration_days = table.Column<int>(type: "int", nullable: true),
                    service_type = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__addition__3213E83FAF0C9D62", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "job_categories",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    parent_id = table.Column<int>(type: "int", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__job_cate__3213E83FCA9C1F34", x => x.id);
                    table.ForeignKey(
                        name: "FK_job_categories_parent",
                        column: x => x.parent_id,
                        principalTable: "job_categories",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__roles__3213E83F596F8211", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "subscription_plans",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    price = table.Column<decimal>(type: "decimal(15,2)", nullable: false),
                    duration_days = table.Column<int>(type: "int", nullable: false),
                    silver_posts = table.Column<int>(type: "int", nullable: false),
                    gold_posts = table.Column<int>(type: "int", nullable: false),
                    diamond_posts = table.Column<int>(type: "int", nullable: false),
                    push_top_times = table.Column<int>(type: "int", nullable: false),
                    marketing_package = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    priority_level = table.Column<int>(type: "int", nullable: true),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__subscrip__3213E83FB090E19F", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    email = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    password = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    username = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    role_id = table.Column<int>(type: "int", nullable: false),
                    avatar = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    status = table.Column<int>(type: "int", nullable: false),
                    auth_provider = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true, defaultValue: "local"),
                    auth_provider_id = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__users__3213E83F2E2ADCD6", x => x.id);
                    table.ForeignKey(
                        name: "FK_users_roles",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "banners",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    image_url = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    redirect_url = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    position = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    start_date = table.Column<DateTime>(type: "datetime", nullable: true),
                    end_date = table.Column<DateTime>(type: "datetime", nullable: true),
                    status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true, defaultValue: "pending"),
                    bid_amount = table.Column<decimal>(type: "decimal(15,2)", nullable: true),
                    impression_count = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    click_count = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
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
                    company_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    company_description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    company_logo = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    company_website = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    company_size = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    industry = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    verified_badge = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    free_posts_remaining = table.Column<int>(type: "int", nullable: true, defaultValue: 5),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
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
                name: "credit_transactions",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    amount = table.Column<decimal>(type: "decimal(15,2)", nullable: false),
                    transaction_type = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    reference_id = table.Column<int>(type: "int", nullable: true),
                    reference_type = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    balance_after = table.Column<decimal>(type: "decimal(15,2)", nullable: false),
                    description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__credit_t__3213E83F89D5B902", x => x.id);
                    table.ForeignKey(
                        name: "FK_credit_transactions_users",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "job_posts",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    requirements = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    benefits = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    location = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    salary_min = table.Column<decimal>(type: "decimal(15,2)", nullable: true),
                    salary_max = table.Column<decimal>(type: "decimal(15,2)", nullable: true),
                    job_type = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    experience_level = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    deadline = table.Column<DateOnly>(type: "date", nullable: true),
                    image_main = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    image2 = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    image3 = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    image4 = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    post_type = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true, defaultValue: "silver"),
                    status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true, defaultValue: "draft"),
                    priority_level = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    view_count = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    is_featured = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    pushed_to_top_until = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__job_post__3213E83F53C3A6C9", x => x.id);
                    table.ForeignKey(
                        name: "FK_job_posts_users",
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
                    name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    email_template = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    target_count = table.Column<int>(type: "int", nullable: true),
                    sent_count = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    start_date = table.Column<DateTime>(type: "datetime", nullable: true),
                    end_date = table.Column<DateTime>(type: "datetime", nullable: true),
                    status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true, defaultValue: "draft"),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
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
                    payment_method = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true, defaultValue: "vnpay"),
                    transaction_id = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    payment_status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true, defaultValue: "pending"),
                    payment_type = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    reference_id = table.Column<int>(type: "int", nullable: true),
                    payment_date = table.Column<DateTime>(type: "datetime", nullable: true),
                    vnpay_transaction_info = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
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
                name: "service_orders",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    service_id = table.Column<int>(type: "int", nullable: false),
                    job_post_id = table.Column<int>(type: "int", nullable: true),
                    start_date = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    end_date = table.Column<DateTime>(type: "datetime", nullable: true),
                    status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true, defaultValue: "pending"),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__service___3213E83FA9296A27", x => x.id);
                    table.ForeignKey(
                        name: "FK_service_orders_services",
                        column: x => x.service_id,
                        principalTable: "additional_services",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_service_orders_users",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "subscriptions",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    plan_id = table.Column<int>(type: "int", nullable: false),
                    silver_posts_remaining = table.Column<int>(type: "int", nullable: false),
                    gold_posts_remaining = table.Column<int>(type: "int", nullable: false),
                    diamond_posts_remaining = table.Column<int>(type: "int", nullable: false),
                    push_top_remaining = table.Column<int>(type: "int", nullable: false),
                    start_date = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    end_date = table.Column<DateTime>(type: "datetime", nullable: false),
                    status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true, defaultValue: "active"),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__subscrip__3213E83FD26BB75F", x => x.id);
                    table.ForeignKey(
                        name: "FK_subscriptions_plans",
                        column: x => x.plan_id,
                        principalTable: "subscription_plans",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_subscriptions_users",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "user_credits",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    balance = table.Column<decimal>(type: "decimal(15,2)", nullable: true, defaultValue: 0m),
                    last_updated = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__user_cre__3213E83F00516FA5", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_credits_users",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "user_details",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    phoneNumber = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    address = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    headline = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    experience_years = table.Column<int>(type: "int", nullable: true),
                    education = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    skills = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    desired_position = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    desired_salary = table.Column<decimal>(type: "decimal(15,2)", nullable: true),
                    desired_location = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    availability = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    bio = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__user_det__3213E83F18C0F97A", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_details_users",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "applications",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    job_post_id = table.Column<int>(type: "int", nullable: false),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true, defaultValue: "pending"),
                    employer_rating = table.Column<int>(type: "int", nullable: true),
                    worker_rating = table.Column<int>(type: "int", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__applicat__3213E83F17E69A8C", x => x.id);
                    table.ForeignKey(
                        name: "FK_applications_job_posts",
                        column: x => x.job_post_id,
                        principalTable: "job_posts",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_applications_users",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "job_post_categories",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    job_post_id = table.Column<int>(type: "int", nullable: false),
                    category_id = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__job_post__3213E83F339E1BA6", x => x.id);
                    table.ForeignKey(
                        name: "FK_job_post_categories_categories",
                        column: x => x.category_id,
                        principalTable: "job_categories",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_job_post_categories_job_posts",
                        column: x => x.job_post_id,
                        principalTable: "job_posts",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "service_usages",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    job_post_id = table.Column<int>(type: "int", nullable: false),
                    service_type = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    reference_id = table.Column<int>(type: "int", nullable: true),
                    reference_type = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    start_date = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    end_date = table.Column<DateTime>(type: "datetime", nullable: true),
                    status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true, defaultValue: "active"),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
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

            migrationBuilder.CreateTable(
                name: "worker_visits",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    job_post_id = table.Column<int>(type: "int", nullable: false),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    visit_time = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    is_first_view = table.Column<bool>(type: "bit", nullable: true, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__worker_v__3213E83FEF2C8194", x => x.id);
                    table.ForeignKey(
                        name: "FK_worker_visits_job_posts",
                        column: x => x.job_post_id,
                        principalTable: "job_posts",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_worker_visits_users",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "idx_applications_status",
                table: "applications",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_applications_job_post_id",
                table: "applications",
                column: "job_post_id");

            migrationBuilder.CreateIndex(
                name: "IX_applications_user_id",
                table: "applications",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_banners_user_id",
                table: "banners",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "idx_company_profiles_user_id",
                table: "company_profiles",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_credit_transactions_user_id",
                table: "credit_transactions",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_job_categories_parent_id",
                table: "job_categories",
                column: "parent_id");

            migrationBuilder.CreateIndex(
                name: "IX_job_post_categories_category_id",
                table: "job_post_categories",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_job_post_categories_job_post_id",
                table: "job_post_categories",
                column: "job_post_id");

            migrationBuilder.CreateIndex(
                name: "idx_job_posts_post_type",
                table: "job_posts",
                column: "post_type");

            migrationBuilder.CreateIndex(
                name: "idx_job_posts_priority_level",
                table: "job_posts",
                column: "priority_level");

            migrationBuilder.CreateIndex(
                name: "idx_job_posts_pushed_to_top_until",
                table: "job_posts",
                column: "pushed_to_top_until");

            migrationBuilder.CreateIndex(
                name: "idx_job_posts_status",
                table: "job_posts",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "idx_job_posts_user_id",
                table: "job_posts",
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
                name: "UQ__roles__72E12F1B404E6A29",
                table: "roles",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_service_orders_status",
                table: "service_orders",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_service_orders_service_id",
                table: "service_orders",
                column: "service_id");

            migrationBuilder.CreateIndex(
                name: "IX_service_orders_user_id",
                table: "service_orders",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_service_usages_job_post_id",
                table: "service_usages",
                column: "job_post_id");

            migrationBuilder.CreateIndex(
                name: "IX_service_usages_user_id",
                table: "service_usages",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "idx_subscriptions_status",
                table: "subscriptions",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_subscriptions_plan_id",
                table: "subscriptions",
                column: "plan_id");

            migrationBuilder.CreateIndex(
                name: "IX_subscriptions_user_id",
                table: "subscriptions",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_credits_user_id",
                table: "user_credits",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "idx_user_details_user_id",
                table: "user_details",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "idx_users_role_id",
                table: "users",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "IX_worker_visits_job_post_id",
                table: "worker_visits",
                column: "job_post_id");

            migrationBuilder.CreateIndex(
                name: "IX_worker_visits_user_id",
                table: "worker_visits",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "applications");

            migrationBuilder.DropTable(
                name: "banners");

            migrationBuilder.DropTable(
                name: "company_profiles");

            migrationBuilder.DropTable(
                name: "credit_transactions");

            migrationBuilder.DropTable(
                name: "job_post_categories");

            migrationBuilder.DropTable(
                name: "marketing_campaigns");

            migrationBuilder.DropTable(
                name: "payments");

            migrationBuilder.DropTable(
                name: "service_orders");

            migrationBuilder.DropTable(
                name: "service_usages");

            migrationBuilder.DropTable(
                name: "subscriptions");

            migrationBuilder.DropTable(
                name: "user_credits");

            migrationBuilder.DropTable(
                name: "user_details");

            migrationBuilder.DropTable(
                name: "worker_visits");

            migrationBuilder.DropTable(
                name: "job_categories");

            migrationBuilder.DropTable(
                name: "additional_services");

            migrationBuilder.DropTable(
                name: "subscription_plans");

            migrationBuilder.DropTable(
                name: "job_posts");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "roles");
        }
    }
}
