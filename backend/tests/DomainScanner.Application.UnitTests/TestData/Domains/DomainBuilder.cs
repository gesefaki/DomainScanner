using DomainScanner.Domain.Entities;

namespace DomainScanner.Application.UnitTests.TestData.Domains;

/// <summary>
/// Builder for creating <see cref="DomainEntity"/> instances in tests.
/// By default, returns valid domains with random IDs and default values.
/// </summary>
public sealed class DomainBuilder
{
    private Guid _id = Guid.NewGuid();
    private string _address = "https://example.com/";
    private Guid _userId = Guid.NewGuid();
    private bool _isActive = true;

    /// <summary>
    /// Sets the domain ID.
    /// </summary>
    /// <param name="id">The domain identifier.</param>
    /// <returns>The current builder instance.</returns>
    public DomainBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    /// <summary>
    /// Sets the domain address.
    /// </summary>
    /// <param name="address">The domain URL.</param>
    /// <returns>The current builder instance.</returns>
    public DomainBuilder WithAddress(string address)
    {
        _address = address;
        return this;
    }
    
    /// <summary>
    /// Sets the user ID associated with the domain.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>The current builder instance.</returns>
    public DomainBuilder WithUserId(Guid userId)
    {
        _userId = userId;
        return this;
    }
    
    /// <summary>
    /// Sets the domain as active.
    /// </summary>
    /// <returns>The current builder instance.</returns>
    public DomainBuilder Active()
    {
        _isActive = true;
        return this;
    }
    
    /// <summary>
    /// Sets the domain as inactive.
    /// </summary>
    /// <returns>The current builder instance.</returns>
    public DomainBuilder Inactive()
    {
        _isActive = false;
        return this;
    }
    
    /// <summary>
    /// Builds and returns a <see cref="DomainEntity"/> with the configured properties.
    /// </summary>
    /// <returns>A new <see cref="DomainEntity"/> instance.</returns>
    public DomainEntity Build()
    {
        return new DomainEntity()
        {
            Id = _id,
            Address = _address,
            UserId = _userId,
            IsActive = _isActive
        };
    }
    
    /// <summary>
    /// Builds and returns a <see cref="List{DomainEntity}"/> with the configured properties.
    /// </summary>
    /// <param name="count">Length of range.</param>
    /// <returns>The collection of <see cref="DomainEntity"/>.</returns>
    public List<DomainEntity> BuildRange(int count)
    {
        var result = new List<DomainEntity>();
        
        for (var i = 0; i < count; i++)
        {
            var domain = Build();
            result.Add(domain);
        }

        return result;
    }
}