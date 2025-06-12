﻿using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace SJOB_EXE201.Models;

public partial class SjobContext : DbContext
{
    public SjobContext()
    {
    }

    public SjobContext(DbContextOptions<SjobContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AdditionalService> AdditionalServices { get; set; }

    public virtual DbSet<Application> Applications { get; set; }

    public virtual DbSet<CreditTransaction> CreditTransactions { get; set; }

    public virtual DbSet<JobCategory> JobCategories { get; set; }

    public virtual DbSet<JobPost> JobPosts { get; set; }

    public virtual DbSet<JobPostCategory> JobPostCategories { get; set; }
    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<ServiceOrder> ServiceOrders { get; set; }
    public virtual DbSet<Subscription> Subscriptions { get; set; }

    public virtual DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
    public virtual DbSet<UserPostCredit> UserPostCredits { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserCredit> UserCredits { get; set; }

    public virtual DbSet<UserDetail> UserDetails { get; set; }

    public virtual DbSet<WorkerVisit> WorkerVisits { get; set; }

    public virtual DbSet<Wishlist> Wishlists { get; set; }
    public virtual DbSet<ApplicationNote> ApplicationNotes { get; set; }
    public virtual DbSet<Notification> Notifications { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        optionsBuilder.UseSqlServer(config.GetConnectionString("DB"));
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AdditionalService>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__addition__3213E83FAF0C9D62");

            entity.ToTable("additional_services");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.DurationDays).HasColumnName("duration_days");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Price)
                .HasColumnType("decimal(15, 2)")
                .HasColumnName("price");
            entity.Property(e => e.ServiceType)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("service_type");
            // Add these properties to your existing configuration
            entity.Property(e => e.SilverPostsIncluded)
                .HasDefaultValue(0)
                .HasColumnName("silver_posts_included");
            entity.Property(e => e.GoldPostsIncluded)
                .HasDefaultValue(0)
                .HasColumnName("gold_posts_included");
            entity.Property(e => e.DiamondPostsIncluded)
                .HasDefaultValue(0)
                .HasColumnName("diamond_posts_included");

			entity.Property(e => e.PushToTopAvailable)
				.HasDefaultValue(0)
				.HasColumnName("push_to_top_available");

			entity.Property(e => e.AuthenLogoAvailable)
				.HasDefaultValue(0)
				.HasColumnName("authen_logo_available");
		});

        modelBuilder.Entity<Application>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__applicat__3213E83F17E69A8C");

            entity.ToTable("applications");

            entity.HasIndex(e => e.Status, "idx_applications_status");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.EmployerRating).HasColumnName("employer_rating");
            entity.Property(e => e.JobPostId).HasColumnName("job_post_id");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("pending")
                .HasColumnName("status");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.WorkerRating).HasColumnName("worker_rating");

            entity.HasOne(d => d.JobPost).WithMany(p => p.Applications)
                .HasForeignKey(d => d.JobPostId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_applications_job_posts");

            entity.HasOne(d => d.User).WithMany(p => p.Applications)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_applications_users");
        });

        modelBuilder.Entity<CreditTransaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__credit_t__3213E83F89D5B902");

            entity.ToTable("credit_transactions");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(15, 2)")
                .HasColumnName("amount");
            entity.Property(e => e.BalanceAfter)
                .HasColumnType("decimal(15, 2)")
                .HasColumnName("balance_after");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.ReferenceId).HasColumnName("reference_id");
            entity.Property(e => e.ReferenceType)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("reference_type");
            entity.Property(e => e.TransactionType)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("transaction_type");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.CreditTransactions)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_credit_transactions_users");
        });

        modelBuilder.Entity<JobCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__job_cate__3213E83FCA9C1F34");

            entity.ToTable("job_categories");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.ParentId).HasColumnName("parent_id");

            entity.HasOne(d => d.Parent).WithMany(p => p.InverseParent)
                .HasForeignKey(d => d.ParentId)
                .HasConstraintName("FK_job_categories_parent");
        });

        modelBuilder.Entity<JobPost>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__job_post__3213E83F53C3A6C9");

            entity.ToTable("job_posts");

            entity.HasIndex(e => e.PostType, "idx_job_posts_post_type");

            entity.HasIndex(e => e.PriorityLevel, "idx_job_posts_priority_level");

            entity.HasIndex(e => e.PushedToTopUntil, "idx_job_posts_pushed_to_top_until");

            entity.HasIndex(e => e.Status, "idx_job_posts_status");

            entity.HasIndex(e => e.UserId, "idx_job_posts_user_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Benefits).HasColumnName("benefits");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Deadline).HasColumnName("deadline");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.ExperienceLevel)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("experience_level");
            entity.Property(e => e.Image2)
                .HasMaxLength(255)
                .HasColumnName("image2");
            entity.Property(e => e.Image3)
                .HasMaxLength(255)
                .HasColumnName("image3");
            entity.Property(e => e.Image4)
                .HasMaxLength(255)
                .HasColumnName("image4");
            entity.Property(e => e.ImageMain)
                .HasMaxLength(255)
                .HasColumnName("image_main");
            entity.Property(e => e.IsFeatured)
                .HasDefaultValue(false)
                .HasColumnName("is_featured");
            entity.Property(e => e.JobType)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("job_type");
            entity.Property(e => e.Location)
                .HasMaxLength(255)
                .HasColumnName("location");
            entity.Property(e => e.PostType)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("silver")
                .HasColumnName("post_type");
            entity.Property(e => e.PriorityLevel)
                .HasDefaultValue(0)
                .HasColumnName("priority_level");
            entity.Property(e => e.PushedToTopUntil)
                .HasColumnType("datetime")
                .HasColumnName("pushed_to_top_until");
            entity.Property(e => e.Requirements).HasColumnName("requirements");
            entity.Property(e => e.SalaryMax)
                .HasColumnType("decimal(15, 2)")
                .HasColumnName("salary_max");
            entity.Property(e => e.SalaryMin)
                .HasColumnType("decimal(15, 2)")
                .HasColumnName("salary_min");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("draft")
                .HasColumnName("status");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.ViewCount)
                .HasDefaultValue(0)
                .HasColumnName("view_count");

            entity.HasOne(d => d.User).WithMany(p => p.JobPosts)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_job_posts_users");
        });

        modelBuilder.Entity<JobPostCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__job_post__3213E83F339E1BA6");

            entity.ToTable("job_post_categories");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.JobPostId).HasColumnName("job_post_id");

            entity.HasOne(d => d.Category).WithMany(p => p.JobPostCategories)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_job_post_categories_categories");

            entity.HasOne(d => d.JobPost).WithMany(p => p.JobPostCategories)
                .HasForeignKey(d => d.JobPostId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_job_post_categories_job_posts");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__roles__3213E83F596F8211");

            entity.ToTable("roles");

            entity.HasIndex(e => e.Name, "UQ__roles__72E12F1B404E6A29").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("name");
        });

        modelBuilder.Entity<ServiceOrder>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__service___3213E83FA9296A27");

            entity.ToTable("service_orders");

            entity.HasIndex(e => e.Status, "idx_service_orders_status");

            entity.Property(e => e.Id).HasColumnName("id");
            // Add these properties to your existing configuration
            entity.Property(e => e.SilverPostsApplied)
                .HasDefaultValue(0)
                .HasColumnName("silver_posts_applied");
            entity.Property(e => e.GoldPostsApplied)
                .HasDefaultValue(0)
                .HasColumnName("gold_posts_applied");
            entity.Property(e => e.DiamondPostsApplied)
                .HasDefaultValue(0)
                .HasColumnName("diamond_posts_applied");
            entity.Property(e => e.PostCreditsApplied)
                .HasDefaultValue(false)
                .HasColumnName("post_credits_applied");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.EndDate)
                .HasColumnType("datetime")
                .HasColumnName("end_date");
            entity.Property(e => e.ServiceId).HasColumnName("service_id");
            entity.Property(e => e.StartDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("start_date");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("pending")
                .HasColumnName("status");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Service).WithMany(p => p.ServiceOrders)
                .HasForeignKey(d => d.ServiceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_service_orders_services");

            entity.HasOne(d => d.User).WithMany(p => p.ServiceOrders)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_service_orders_users");
        });

        modelBuilder.Entity<Subscription>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__subscrip__3213E83FD26BB75F");

            entity.ToTable("subscriptions");

            entity.HasIndex(e => e.Status, "idx_subscriptions_status");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DiamondPostsRemaining).HasColumnName("diamond_posts_remaining");
            entity.Property(e => e.EndDate)
                .HasColumnType("datetime")
                .HasColumnName("end_date");
            entity.Property(e => e.GoldPostsRemaining).HasColumnName("gold_posts_remaining");
            entity.Property(e => e.PlanId).HasColumnName("plan_id");
            entity.Property(e => e.PushTopRemaining).HasColumnName("push_top_remaining");
            entity.Property(e => e.SilverPostsRemaining).HasColumnName("silver_posts_remaining");
            entity.Property(e => e.StartDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("start_date");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("active")
                .HasColumnName("status");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Plan).WithMany(p => p.Subscriptions)
                .HasForeignKey(d => d.PlanId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_subscriptions_plans");

            entity.HasOne(d => d.User).WithMany(p => p.Subscriptions)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_subscriptions_users");
        });

        modelBuilder.Entity<SubscriptionPlan>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__subscrip__3213E83FB090E19F");

            entity.ToTable("subscription_plans");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.DiamondPosts).HasColumnName("diamond_posts");
            entity.Property(e => e.DurationDays).HasColumnName("duration_days");
            entity.Property(e => e.GoldPosts).HasColumnName("gold_posts");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.MarketingPackage)
                .HasDefaultValue(false)
                .HasColumnName("marketing_package");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.Price)
                .HasColumnType("decimal(15, 2)")
                .HasColumnName("price");
            entity.Property(e => e.PriorityLevel).HasColumnName("priority_level");
            entity.Property(e => e.PushTopTimes).HasColumnName("push_top_times");
            entity.Property(e => e.SilverPosts).HasColumnName("silver_posts");
        });

        modelBuilder.Entity<UserPostCredit>(entity =>
        {
            entity.ToTable("user_post_credits");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
			entity.Property(e => e.PushToTopAvailable)
				.HasDefaultValue(0)
				.HasColumnName("push_to_top_available");
			entity.Property(e => e.AuthenLogoAvailable)
				.HasDefaultValue(0)
				.HasColumnName("authen_logo_available");
			entity.Property(e => e.SilverPostsAvailable)
                .HasDefaultValue(0)
                .HasColumnName("silver_posts_available");
            entity.Property(e => e.GoldPostsAvailable)
                .HasDefaultValue(0)
                .HasColumnName("gold_posts_available");
            entity.Property(e => e.DiamondPostsAvailable)
                .HasDefaultValue(0)
                .HasColumnName("diamond_posts_available");
            entity.Property(e => e.LastUpdated)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("last_updated");

            entity.HasOne(d => d.User).WithOne(p => p.UserPostCredit)
                .HasForeignKey<UserPostCredit>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_user_post_credits_users");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__users__3213E83F2E2ADCD6");

            entity.ToTable("users");

            entity.HasIndex(e => e.RoleId, "idx_users_role_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AuthProvider)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("local")
                .HasColumnName("auth_provider");
            entity.Property(e => e.AuthProviderId)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("auth_provider_id");
            entity.Property(e => e.Avatar)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("avatar");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue(true)
                .HasColumnName("status");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("password");
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.Username)
                .HasMaxLength(255)
                .HasColumnName("username");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_users_roles");
        });

        modelBuilder.Entity<UserCredit>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__user_cre__3213E83F00516FA5");

            entity.ToTable("user_credits");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Balance)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(15, 2)")
                .HasColumnName("balance");
            entity.Property(e => e.LastUpdated)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("last_updated");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.UserCredits)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_user_credits_users");
        });

        modelBuilder.Entity<UserDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__user_det__3213E83F18C0F97A");

            entity.ToTable("user_details");

            entity.HasIndex(e => e.UserId, "idx_user_details_user_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.FirstName)
                .HasMaxLength(100)
                .HasColumnName("firstName");
            entity.Property(e => e.LastName)
                .HasMaxLength(100)
                .HasColumnName("lastName");
            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .HasColumnName("address");
            entity.Property(e => e.Availability).HasColumnName("availability");
            entity.Property(e => e.Bio).HasColumnName("bio");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DesiredLocation)
                .HasMaxLength(255)
                .HasColumnName("desired_location");
            entity.Property(e => e.DesiredPosition)
                .HasMaxLength(255)
                .HasColumnName("desired_position");
            entity.Property(e => e.DesiredSalary)
                .HasColumnType("decimal(15, 2)")
                .HasColumnName("desired_salary");
            entity.Property(e => e.Education).HasColumnName("education");
            entity.Property(e => e.ExperienceYears).HasColumnName("experience_years");
            entity.Property(e => e.Headline)
                .HasMaxLength(255)
                .HasColumnName("headline");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("phoneNumber");
            entity.Property(e => e.Skills).HasColumnName("skills");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.UserDetails)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_user_details_users");
        });

        modelBuilder.Entity<WorkerVisit>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__worker_v__3213E83FEF2C8194");

            entity.ToTable("worker_visits");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.IsFirstView)
                .HasDefaultValue(true)
                .HasColumnName("is_first_view");
            entity.Property(e => e.JobPostId).HasColumnName("job_post_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.VisitTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("visit_time");

            entity.HasOne(d => d.JobPost).WithMany(p => p.WorkerVisits)
                .HasForeignKey(d => d.JobPostId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_worker_visits_job_posts");

            entity.HasOne(d => d.User).WithMany(p => p.WorkerVisits)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_worker_visits_users");
        });

        modelBuilder.Entity<Wishlist>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__wishlist__3213E83F");

            entity.ToTable("wishlists");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.JobPostId).HasColumnName("job_post_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");

            // Create a unique index to prevent duplicate wishlist entries
            entity.HasIndex(e => new { e.UserId, e.JobPostId })
                .IsUnique()
                .HasName("UQ__wishlists__user_job");

            // Configure relationships
            entity.HasOne(d => d.User)
                .WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_wishlists_users");

            entity.HasOne(d => d.JobPost)
                .WithMany()
                .HasForeignKey(d => d.JobPostId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_wishlists_job_posts");
        });

        // New configurations
        modelBuilder.Entity<ApplicationNote>(entity =>
        {
            entity.ToTable("application_notes");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ApplicationId).HasColumnName("application_id");
            entity.Property(e => e.Note).HasColumnName("note");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("getdate()");

            entity.HasOne(d => d.Application)
                .WithMany()
                .HasForeignKey(d => d.ApplicationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.ToTable("notifications");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Title).HasColumnName("title").HasMaxLength(255);
            entity.Property(e => e.Message).HasColumnName("message");
            entity.Property(e => e.Type).HasColumnName("type").HasMaxLength(50);
            entity.Property(e => e.ReferenceId).HasColumnName("reference_id");
            entity.Property(e => e.ReferenceType).HasColumnName("reference_type").HasMaxLength(50);
            entity.Property(e => e.Url).HasColumnName("url").HasMaxLength(255);
            entity.Property(e => e.IsRead).HasColumnName("is_read").HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("getdate()");

            entity.HasOne(d => d.User)
                .WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
