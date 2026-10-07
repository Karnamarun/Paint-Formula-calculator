using Microsoft.EntityFrameworkCore;
using PaintTintCalculator.Domain.Entities;

namespace PaintTintCalculator.Infrastructure.Persistence;

public class PaintTintDbContext : DbContext
{
    public DbSet<Base> Bases => Set<Base>();
    public DbSet<Colorant> Colorants => Set<Colorant>();
    public DbSet<Shade> Shades => Set<Shade>();
    public DbSet<FormulaItem> FormulaItems => Set<FormulaItem>();
    public DbSet<DispenseJob> DispenseJobs => Set<DispenseJob>();
    public DbSet<DispenseJobItem> DispenseJobItems => Set<DispenseJobItem>();

    public PaintTintDbContext(DbContextOptions<PaintTintDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Base Configuration
        modelBuilder.Entity<Base>(builder =>
        {
            builder.ToTable("Bases");
            builder.HasKey(b => b.Id);
            builder.Property(b => b.Name).HasMaxLength(50).IsRequired();
            builder.HasIndex(b => b.Name).IsUnique();
            builder.Property(b => b.MaxTintPercent).HasPrecision(5, 2);
            builder.Property(b => b.PricePerLitre).HasPrecision(10, 2);
        });

        // Colorant Configuration
        modelBuilder.Entity<Colorant>(builder =>
        {
            builder.ToTable("Colorants");
            builder.HasKey(c => c.Id);
            builder.Property(c => c.Code).HasMaxLength(10).IsRequired();
            builder.HasIndex(c => c.Code).IsUnique();
            builder.Property(c => c.Name).HasMaxLength(100).IsRequired();
            builder.Property(c => c.CostPerMl).HasPrecision(10, 4);
        });

        // Shade Configuration
        modelBuilder.Entity<Shade>(builder =>
        {
            builder.ToTable("Shades");
            builder.HasKey(s => s.Id);
            builder.Property(s => s.Code).HasMaxLength(20).IsRequired();
            builder.HasIndex(s => s.Code).IsUnique();
            builder.Property(s => s.Name).HasMaxLength(100).IsRequired();
            builder.Property(s => s.HexColor).HasMaxLength(9).IsRequired();
        });

        // FormulaItem Configuration
        modelBuilder.Entity<FormulaItem>(builder =>
        {
            builder.ToTable("FormulaItems");
            builder.HasKey(f => f.Id);
            builder.Property(f => f.MlPerLitre).HasPrecision(10, 4);

            // Unique constraint on (ShadeId, BaseId, ColorantId)
            builder.HasIndex(f => new { f.ShadeId, f.BaseId, f.ColorantId }).IsUnique();

            builder.HasOne(f => f.Shade)
                .WithMany(s => s.FormulaItems)
                .HasForeignKey(f => f.ShadeId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(f => f.Base)
                .WithMany(b => b.FormulaItems)
                .HasForeignKey(f => f.BaseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(f => f.Colorant)
                .WithMany(c => c.FormulaItems)
                .HasForeignKey(f => f.ColorantId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // DispenseJob Configuration
        modelBuilder.Entity<DispenseJob>(builder =>
        {
            builder.ToTable("DispenseJobs");
            builder.HasKey(j => j.Id);
            builder.Property(j => j.CanSizeLitres).HasPrecision(5, 2);
            builder.Property(j => j.TotalColorantMl).HasPrecision(10, 2);
            builder.Property(j => j.TintPercent).HasPrecision(5, 2);
            builder.Property(j => j.TotalPrice).HasPrecision(10, 2);

            builder.HasOne(j => j.Shade)
                .WithMany(s => s.DispenseJobs)
                .HasForeignKey(j => j.ShadeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(j => j.Base)
                .WithMany(b => b.DispenseJobs)
                .HasForeignKey(j => j.BaseId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // DispenseJobItem Configuration
        modelBuilder.Entity<DispenseJobItem>(builder =>
        {
            builder.ToTable("DispenseJobItems");
            builder.HasKey(ji => ji.Id);
            builder.Property(ji => ji.DispensedMl).HasPrecision(10, 2);
            builder.Property(ji => ji.Cost).HasPrecision(10, 2);

            // DispenseJobItems must cascade delete from DispenseJobs
            builder.HasOne(ji => ji.DispenseJob)
                .WithMany(j => j.Items)
                .HasForeignKey(ji => ji.DispenseJobId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ji => ji.Colorant)
                .WithMany(c => c.DispenseJobItems)
                .HasForeignKey(ji => ji.ColorantId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}

