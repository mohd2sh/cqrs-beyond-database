using CleanArchitecture.Core.Application.Abstractions.Messaging;

namespace CdcCqrsDemo.Application.Commands.CreateProduct;

public record CreateProductCommand(
    string Name,
    decimal Price,
    string? Description
) : ICommand<int>;

