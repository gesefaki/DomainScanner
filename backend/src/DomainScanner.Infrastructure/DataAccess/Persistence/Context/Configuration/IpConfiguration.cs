using DomainScanner.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DomainScanner.Infrastructure.DataAccess.Persistence.Context.Configuration;

internal class IpConfiguration : BaseEntityConfiguration<Ip>
{
    public override void Configure(EntityTypeBuilder<Ip> builder)
    {
        
    }
}