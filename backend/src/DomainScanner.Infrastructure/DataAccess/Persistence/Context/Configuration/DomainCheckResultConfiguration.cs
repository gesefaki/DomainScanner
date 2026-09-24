using DomainScanner.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DomainScanner.Infrastructure.DataAccess.Persistence.Context.Configuration;

/// <summary>Maps stored domain checks, including their HTTP-specific fields.</summary>
internal sealed class DomainCheckResultConfiguration : BaseEntityConfiguration<DomainCheckResult>
{
    /// <inheritdoc />
    public override void Configure(EntityTypeBuilder<DomainCheckResult> builder)
    {
        base.Configure(builder);

        builder.Property(check => check.Kind).HasMaxLength(32).IsRequired();
        builder.Property(check => check.Outcome).HasMaxLength(16).IsRequired();
        builder.Property(check => check.RequestedAddress).IsRequired();
        builder.Property(check => check.FinalAddress).IsRequired(false);
        builder.Property(check => check.StatusCode).IsRequired(false);
        builder.Property(check => check.ErrorCode).HasMaxLength(64);
        builder.Property(check => check.Redirects).HasColumnType("text[]");

        builder.HasOne(check => check.DomainEntity)
            .WithMany(domain => domain.CheckResults)
            .HasForeignKey(check => check.DomainId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(check => new
        {
            check.DomainId,
            check.CreatedAt,
            check.Id
        });
    }
}
