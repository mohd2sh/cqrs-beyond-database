namespace CdcCqrsDemo.Application.Dtos;

public record ProductDto(
    int Id,
    string Name,
    decimal Price,
    string? Description,
    DateTime UpdatedAt
);


