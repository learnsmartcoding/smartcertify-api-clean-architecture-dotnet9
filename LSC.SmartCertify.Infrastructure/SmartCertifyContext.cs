using System;
using System.Collections.Generic;
using LSC.SmartCertify.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LSC.SmartCertify.Infrastructure;

public partial class SmartCertifyContext : DbContext
{
    public SmartCertifyContext()
    {
    }

    public SmartCertifyContext(DbContextOptions<SmartCertifyContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BannerInfo> BannerInfos { get; set; }

    public virtual DbSet<Choice> Choices { get; set; }

    public virtual DbSet<ContactU> ContactUs { get; set; }

    public virtual DbSet<Course> Courses { get; set; }

    public virtual DbSet<Exam> Exams { get; set; }

    public virtual DbSet<ExamQuestion> ExamQuestions { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Question> Questions { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<SmartApp> SmartApps { get; set; }

    public virtual DbSet<UserActivityLog> UserActivityLogs { get; set; }

    public virtual DbSet<UserNotification> UserNotifications { get; set; }

    public virtual DbSet<UserProfile> UserProfiles { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BannerInfo>(entity =>
        {
            entity.HasKey(e => e.BannerId).HasName("PK_BannerInfo_BannerId");

            entity.Property(e => e.CreatedOn).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<Choice>(entity =>
        {
            entity.HasKey(e => e.ChoiceId).HasName("PK_Choices_ChoiceId");

            entity.HasOne(d => d.Question).WithMany(p => p.Choices)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Choices_QuestionId");
        });

        modelBuilder.Entity<ContactU>(entity =>
        {
            entity.HasKey(e => e.ContactUsId).HasName("PK_ContactUs_ContactUsId");
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(e => e.CourseId).HasName("PK_Courses_CourseId");

            entity.Property(e => e.CreatedOn).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Courses)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Courses_CreatedBy");
        });

        modelBuilder.Entity<Exam>(entity =>
        {
            entity.HasKey(e => e.ExamId).HasName("PK_Exams_ExamId");

            entity.Property(e => e.StartedOn).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Status).HasDefaultValue("In Progress");

            entity.HasOne(d => d.Course).WithMany(p => p.Exams)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Exams_CourseId");

            entity.HasOne(d => d.User).WithMany(p => p.Exams)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Exams_UserId");
        });

        modelBuilder.Entity<ExamQuestion>(entity =>
        {
            entity.HasKey(e => e.ExamQuestionId).HasName("PK_ExamQuestions_ExamQuestionId");

            entity.Property(e => e.ReviewLater).HasDefaultValue(false);

            entity.HasOne(d => d.Exam).WithMany(p => p.ExamQuestions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ExamQuestions_ExamId");

            entity.HasOne(d => d.Question).WithMany(p => p.ExamQuestions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ExamQuestions_QuestionId");

            entity.HasOne(d => d.SelectedChoice).WithMany(p => p.ExamQuestions).HasConstraintName("FK_ExamQuestions_SelectedChoiceId");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("PK_Notification_NotificationId");

            entity.Property(e => e.CreatedOn).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasKey(e => e.QuestionId).HasName("PK_Questions_QuestionId");

            entity.HasOne(d => d.Course).WithMany(p => p.Questions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Questions_CourseId");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK_Roles_RoleId");
        });

        modelBuilder.Entity<SmartApp>(entity =>
        {
            entity.HasKey(e => e.SmartAppId).HasName("PK_SmartApp_SmartAppId");
        });

        modelBuilder.Entity<UserActivityLog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK_UserActivityLog_LogId");

            entity.HasOne(d => d.User).WithMany(p => p.UserActivityLogs).HasConstraintName("FK_UserActivityLog_UserProfile");
        });

        modelBuilder.Entity<UserNotification>(entity =>
        {
            entity.HasKey(e => e.UserNotificationId).HasName("PK_UserNotifications_UserNotificationId");

            entity.Property(e => e.CreatedOn).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Notification).WithMany(p => p.UserNotifications)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserNotifications_NotificationId");

            entity.HasOne(d => d.User).WithMany(p => p.UserNotifications)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserNotifications_UserId");
        });

        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK_UserProfile_UserId");
            //entity.Property(e => e.ProfilePictureUrl).HasMaxLength(500);
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.DisplayName).HasDefaultValue("Guest");
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => e.UserRoleId).HasName("PK_UserRole_UserRoleId");

            entity.HasOne(d => d.Role).WithMany(p => p.UserRoles)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserRole_Roles");

            entity.HasOne(d => d.SmartApp).WithMany(p => p.UserRoles)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserRole_SmartApp");

            entity.HasOne(d => d.User).WithMany(p => p.UserRoles)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserRole_UserProfile");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
