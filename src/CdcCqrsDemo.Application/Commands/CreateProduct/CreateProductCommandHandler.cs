using CdcCqrsDemo.Application.Interfaces;
using CdcCqrsDemo.Domain;
using CleanArchitecture.Core.Application.Abstractions.Messaging;
using Microsoft.Extensions.Logging;

namespace CdcCqrsDemo.Application.Commands.CreateProduct;

public class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, int>
{
    private readonly IProductRepository _productRepository;
    private readonly ILogger<CreateProductCommandHandler> _logger;

    public CreateProductCommandHandler(
        IProductRepository productRepository,
        ILogger<CreateProductCommandHandler> logger)
    {
        _productRepository = productRepository;
        _logger = logger;
    }

    public async Task<int> Handle(CreateProductCommand request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating product: {ProductName}", request.Name);

        var product = new Product
        {
            Name = request.Name,
            Price = request.Price,
            Description = request.Description,
            UpdatedAt = DateTime.UtcNow
        };

        var productId = await _productRepository.AddAsync(product, cancellationToken);

        _logger.LogInformation("Product created with Id: {ProductId}", productId);

        return productId;
    }
}

