using DomainScanner.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DomainScanner.Infrastructure.DataAccess.Persistence.Context.Configuration;

internal abstract class BaseEntityConfiguration<TEntity> : IEntityTypeConfiguration<TEntity> where TEntity : BaseEntity
{
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        // Id
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd().IsRequired();
        
        // CreatedAt
        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("NOW()")
            .IsRequired();
        
        // UpdatedAt
        builder.Property(e => e.UpdatedAt).IsRequired(false);
        
        // IsActive
        builder.Property(e => e.IsActive).IsRequired();
    }
}