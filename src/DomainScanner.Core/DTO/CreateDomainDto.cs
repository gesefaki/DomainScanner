using System.ComponentModel.DataAnnotations;

namespace DomainScanner.Core.DTO;

public class CreateDomainDto
{
    [Required, MaxLength(256)]
    public string Name { get; set; }

    public CreateDomainDto(string name)
    {
        if (!name.StartsWith("http://"))
        {
            name = "http://" + name;
        }
        Name = name;
    }
}