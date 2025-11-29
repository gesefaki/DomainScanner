using System.ComponentModel.DataAnnotations;

namespace DomainScanner.Core.DTO;

public class UpdateDomainDto
{
    [Required, MaxLength(256)]
    public string Name { get; init; }
    public bool? IsAvailable { get; init; }

    public UpdateDomainDto(string name, bool? isAvailable)
    {
        Name = name;
        IsAvailable = isAvailable;
    }
}