namespace DomainScanner.Domain.Entities;

public class DomainEntity
{
    // Main Properties
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Address { get; set; } = string.Empty;
    public bool? IsAvailable { get; set; }
    public DateTime CreatedAt { get; }
    public DateTime? UpdatedAt {  get; set; } // Only for scheduler
     
    // Navigation Properties
    public Guid UserId { get; set; } // FK
    public User? User { get; set; } // Navigation Property
    
    public virtual ICollection<DomainCheckResult> CheckResults { get; set; } =  new List<DomainCheckResult>();

    public Uri? AddressToUri()
    {
        try
        {
            Uri.TryCreate(this.Address, UriKind.Absolute, out var uri);
            return uri;
        }
        catch (UriFormatException)
        {
            return null;
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }
}