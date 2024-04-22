using System.ComponentModel.DataAnnotations;

namespace cw6.DTOs;

public record ReplaceAnimalRequest(
    [Required] [MaxLength(200)] string Name,
    [MaxLength(200)] string Description,
    [Required] [MaxLength(200)] string Category,
    [Required] [MaxLength(200)] string Area
    );

public record ReplaceAnimalResponse(int IdAnimal, string Name, string Description, string Category, string Area)
{
    public ReplaceAnimalResponse(int idAnimal, ReplaceAnimalRequest request) : this(idAnimal, request.Name,
        request.Description, request.Category, request.Area) {}
}