using System.ComponentModel.DataAnnotations;

namespace DomainScanner.Core.DTO;

public class ResponseDomainDto
{
    public int Id { get; init; }
    [Required, MaxLength(256)]
    public string Name { get; init; }
    public bool? IsAvailable { get; init; }

    public ResponseDomainDto(int id, string name, bool? isAvailable)
    {
        Id = id;
        Name = name;
        IsAvailable = isAvailable;
    }
}