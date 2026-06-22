using DomainScanner.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DomainScanner.Infrastructure.DataAccess.Persistence.Context.Configuration;

internal class UserConfiguration : BaseEntityConfiguration<User>
{
    public override void Configure(EntityTypeBuilder<User> builder)
    {
        // BaseEntityConfiguration
        base.Configure(builder);
        
        // Username
        builder.Property(u => u.Username).HasMaxLength(150).IsRequired();
        
        // PasswordHash
        builder.Property(u => u.PasswordHash).IsRequired();
        
        // Email
        builder.Property(u => u.Email).IsRequired();
        
        // Navigation
        // Domains
        builder.HasMany(u => u.Domains)
            .WithOne(d => d.User);
        
    }
}