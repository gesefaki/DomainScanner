using DomainScanner.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DomainScanner.Infrastructure.DataAccess.Persistence.Context.Configuration;

/// <summary>
/// Entity type configuration for <see cref="DomainCheckResult"/> 
/// </summary>
internal sealed class DomainCheckResultConfiguration : BaseEntityConfiguration<DomainCheckResult>
{
    /// <inheritdoc />
    public override void Configure(EntityTypeBuilder<DomainCheckResult> builder)
    {
        // BaseEntityConfiguration
        base.Configure(builder);
        
        // Address
        builder.Property(dcr => dcr.Address).IsRequired();
        
        // StatusCode
        builder.Property(dcr => dcr.StatusCode).IsRequired();
        
        // Navigation
        // DomainEntity
        builder.HasOne(dcr => dcr.DomainEntity)
            .WithMany(d => d.CheckResults)
            .HasForeignKey(dcr => dcr.DomainId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}