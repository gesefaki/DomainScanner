using System.ComponentModel.DataAnnotations;

namespace DomainScanner.Core.DTO;

public class UpdateDomainDto
{
    [Required, MaxLength(256)]
    public string Name { get; init; }
    public bool? IsAvailable { get; init; }

    public UpdateDomainDto(string name, bool? isAvailable)
    {
        if (!name.StartsWith("http://") && !name.StartsWith("https://"))
        {
            name = "https://" + name;
        }
        
        Name = name;
        IsAvailable = isAvailable;
    }
}