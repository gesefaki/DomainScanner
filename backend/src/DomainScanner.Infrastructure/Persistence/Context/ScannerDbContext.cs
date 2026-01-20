using DomainScanner.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DomainScanner.Infrastructure.Persistence.Context;

public class ScannerDbContext : DbContext
{
    public DbSet<DomainEntity> Domains { get; set; } 
    public DbSet<DomainCheckResult> CheckResults { get; set; }
    public DbSet<User> Users { get; set; }
    
    public ScannerDbContext(DbContextOptions<ScannerDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(user =>
        {
            user.HasKey(u => u.Id);

            user.HasIndex((u => u.Username))
                .IsUnique();

            user.Property(u => u.Username)
                .IsRequired()
                .HasMaxLength(100);
            
            user.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(100);
            
            user.HasIndex(u => u.Email)
                .IsUnique();

            user.Property(u => u.CreatedAt)
                .HasDefaultValueSql("NOW()");

            user.Property(u => u.UpdatedAt)
                .IsRequired(false);

            user.HasMany(u => u.Domains)
                .WithOne(d => d.User)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            user.HasMany(u => u.Ips)
                .WithOne(i => i.User)
                .HasForeignKey(i => i.UserId)
                .OnDelete(DeleteBehavior.Cascade);

        });

        modelBuilder.Entity<DomainEntity>(domain =>
        {
            domain.HasKey(d => d.Id);

            domain.Property(d => d.Address)
                .IsRequired();

            domain.Property(d => d.IsAvailable)
                .IsRequired(false);

            domain.Property(d => d.CreatedAt)
                .IsRequired();

            domain.Property(d => d.UpdatedAt)
                .IsRequired(false);

            domain.HasOne(d => d.User)
                .WithMany(u => u.Domains)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            domain.HasMany(d => d.Ips)
                .WithOne(i => i.Domain)
                .HasForeignKey(i => i.DomainId)
                .OnDelete(DeleteBehavior.Cascade);
            
            domain.HasMany(d => d.CheckResults)
                .WithOne(c => c.DomainEntity)
                .HasForeignKey(c => c.DomainId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DomainCheckResult>(check =>
        {
            check.HasKey(c => c.Id);
            
            check.Property(c => c.Address)
                .IsRequired();
            
            check.Property(c => c.StatusCode)
                .IsRequired();
            
            check.Property(c => c.IsAvailable)
                .IsRequired();
            
            check.Property(c => c.CreatedAt)
                .IsRequired();
            
            check.HasOne(c => c.DomainEntity)
                .WithMany(d => d.CheckResults)
                .HasForeignKey(c => c.DomainId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Ip>(ip =>
            {
                ip.HasKey(i => i.Id);

                ip.Property(i => i.Address)
                    .IsRequired();

                ip.Property(i => i.IsAvailable)
                    .IsRequired(false);

                ip.Property(i => i.CreatedAt)
                    .IsRequired();

                ip.Property(i => i.UpdatedAt)
                    .IsRequired(false);

                ip.HasOne(i => i.User)
                    .WithMany(u => u.Ips)
                    .HasForeignKey(i => i.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            }
        );
    }
}