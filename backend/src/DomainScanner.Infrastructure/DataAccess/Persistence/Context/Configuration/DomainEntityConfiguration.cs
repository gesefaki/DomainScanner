using DomainScanner.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DomainScanner.Infrastructure.DataAccess.Persistence.Context.Configuration;

/// <summary>
/// Entity type configuration for <see cref="DomainEntity"/> 
/// </summary>
internal sealed class DomainEntityConfiguration : BaseEntityConfiguration<DomainEntity>
{
    /// <inheritdoc />
    public override void Configure(EntityTypeBuilder<DomainEntity> builder)
    {
        // BaseEntityConfiguration
        base.Configure(builder);
        
        // Address
        builder.Property(d => d.Address).HasMaxLength(150).IsRequired();
        
        // Navigation
        // User
        builder.HasOne(d => d.User)
            .WithMany(u => u.Domains)
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // CheckResults
        builder.HasMany(d => d.CheckResults)
            .WithOne(c => c.DomainEntity);
        
        
    }
}