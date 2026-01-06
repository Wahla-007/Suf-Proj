using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace mess_management.Models;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AspNetUser> AspNetUsers { get; set; }

    public virtual DbSet<MonthlyBill> MonthlyBills { get; set; }

    public virtual DbSet<TeacherAttendance> TeacherAttendances { get; set; }

    public virtual DbSet<WeeklyMenu> WeeklyMenus { get; set; }

    public virtual DbSet<WeeklyPlan> WeeklyPlans { get; set; }
    public virtual DbSet<WeeklyPlanDay> WeeklyPlanDays { get; set; }
    public virtual DbSet<Bill> Bills { get; set; }
    public virtual DbSet<BillLine> BillLines { get; set; }
    public virtual DbSet<Payment> Payments { get; set; }
    public virtual DbSet<ReviewRequest> ReviewRequests { get; set; }
    public virtual DbSet<Setting> Settings { get; set; }
    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

    // OnConfiguring is now handled in Program.cs with SQLite

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Token).IsRequired().HasMaxLength(256);
            entity.Property(e => e.UserId).IsRequired().HasMaxLength(450);
            entity.HasIndex(e => e.Token).IsUnique();
            
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<AspNetUser>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AspNetUs__3213E83F6A26BF7E");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.PasswordHash).HasMaxLength(256);
            entity.Property(e => e.FullName).HasMaxLength(256);
            entity.Property(e => e.JoinedDate).HasColumnType("datetime");
            entity.Property(e => e.IsAdmin).HasDefaultValue(false);
            entity.Property(e => e.IsPasswordChanged).HasDefaultValue(false);
        });

        modelBuilder.Entity<MonthlyBill>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__MonthlyB__3213E83F2AF691CE");

            entity.ToTable("MonthlyBill");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.FoodAmount).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.GeneratedOn)
                .HasDefaultValueSql("datetime('now')")
                .HasColumnType("TEXT");
            entity.Property(e => e.PaidAmount).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.PaidOn).HasColumnType("datetime");
            entity.Property(e => e.PaymentToken).HasMaxLength(256);
            entity.Property(e => e.PreviousDue).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Pending");
            entity.Property(e => e.TeacherId).HasMaxLength(450);
            entity.Property(e => e.TotalDue).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.WaterShare).HasColumnType("decimal(18, 0)");

            entity.HasOne(d => d.Teacher).WithMany(p => p.MonthlyBills)
                .HasForeignKey(d => d.TeacherId)
                .HasConstraintName("FK__MonthlyBi__Teach__412EB0B6");
        });

        modelBuilder.Entity<TeacherAttendance>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TeacherA__3213E83FF8935387");

            entity.ToTable("TeacherAttendance");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.IsVerified).HasDefaultValue(false);
            entity.Property(e => e.MarkedBy)
                .HasMaxLength(256)
                .HasDefaultValue("System");
            entity.Property(e => e.TeacherId).HasMaxLength(450);
            entity.Property(e => e.VerificationNote).HasMaxLength(1000);
            entity.Property(e => e.VerifiedAt).HasColumnType("datetime");
            entity.Property(e => e.DisputeStatus).HasMaxLength(50).HasDefaultValue("None");
            entity.Property(e => e.DisputeReason).HasMaxLength(1000);

            entity.HasOne(d => d.Teacher).WithMany(p => p.TeacherAttendances)
                .HasForeignKey(d => d.TeacherId)
                .HasConstraintName("FK__TeacherAt__Teach__4222D4EF");
        });

        modelBuilder.Entity<WeeklyMenu>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__WeeklyMe__3213E83F7983D1E9");
            entity.ToTable("WeeklyMenu");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BreakfastRate).HasPrecision(18, 2);
            entity.Property(e => e.LunchRate).HasPrecision(18, 2);
            entity.Property(e => e.DinnerRate).HasPrecision(18, 2);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("datetime('now')")
                .HasColumnType("TEXT");
            entity.Property(e => e.CreatedById).HasMaxLength(450);

            entity.HasOne(d => d.CreatedBy).WithMany(p => p.WeeklyMenus)
                .HasForeignKey(d => d.CreatedById)
                .HasConstraintName("FK__WeeklyMen__Creat__4316F928");
        });

        modelBuilder.Entity<WeeklyPlan>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.WeekStart).HasColumnType("date");
            entity.Property(e => e.CreatedAt).HasColumnType("TEXT").HasDefaultValueSql("datetime('now')");
            entity.HasMany(e => e.Days).WithOne(d => d.WeeklyPlan).HasForeignKey(d => d.WeeklyPlanId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<WeeklyPlanDay>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.BreakfastName).HasMaxLength(50).HasDefaultValue("Breakfast");
            entity.Property(e => e.LunchName).HasMaxLength(50).HasDefaultValue("Lunch");
            entity.Property(e => e.DinnerName).HasMaxLength(50).HasDefaultValue("Dinner");
            entity.Property(e => e.BreakfastPrice).HasPrecision(18, 2);
            entity.Property(e => e.LunchPrice).HasPrecision(18, 2);
            entity.Property(e => e.DinnerPrice).HasPrecision(18, 2);
        });

        modelBuilder.Entity<Bill>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TotalMealsAmount).HasPrecision(18, 2);
            entity.Property(e => e.WaterFee).HasPrecision(18, 2);
            entity.Property(e => e.PreviousDue).HasPrecision(18, 2);
            entity.Property(e => e.TotalDue).HasPrecision(18, 2);
            entity.Property(e => e.PaidAmount).HasPrecision(18, 2);
            entity.Property(e => e.GeneratedOn).HasColumnType("TEXT").HasDefaultValueSql("datetime('now')");
            entity.Property(e => e.PaidOn).HasColumnType("datetime");
            entity.HasMany(e => e.Lines).WithOne(l => l.Bill).HasForeignKey(l => l.BillId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.Payments).WithOne(p => p.Bill).HasForeignKey(p => p.BillId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BillLine>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Date).HasColumnType("date");
            entity.Property(e => e.Price).HasPrecision(18, 2);
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PaidOn).HasColumnType("datetime");
            entity.Property(e => e.Amount).HasPrecision(18, 2);
        });

        modelBuilder.Entity<ReviewRequest>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.HasOne(r => r.BillLine).WithOne(l => l.ReviewRequest).HasForeignKey<ReviewRequest>(r => r.BillLineId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Setting>(entity =>
        {
            entity.HasKey(e => e.Key);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
