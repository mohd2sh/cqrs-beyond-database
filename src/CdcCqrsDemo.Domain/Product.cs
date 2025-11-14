using CleanArchitecture.Core.Domain.Abstractions;

namespace CdcCqrsDemo.Domain;

public class Product : AggregateRoot<int>
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? Description { get; set; }
    public DateTime UpdatedAt { get; set; }
}


