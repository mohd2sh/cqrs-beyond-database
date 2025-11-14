using CdcCqrsDemo.Application.Commands.CreateProduct;
using CdcCqrsDemo.Application.Queries.GetAllProducts;
using CleanArchitecture.Core.Application.Abstractions.Messaging;
using Microsoft.AspNetCore.Mvc;

namespace CdcCqrsDemo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IMediator mediator, ILogger<ProductsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all products from Elasticsearch (read model)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting all products");
        var products = await _mediator.Send(new GetAllProductsQuery(), cancellationToken);
        return Ok(products);
    }

    /// <summary>
    /// Create a new product (writes to SQL Server)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating product {ProductName}", request.Name);

        var command = new CreateProductCommand(
            request.Name,
            request.Price,
            request.Description);

        var productId = await _mediator.Send(command, cancellationToken);

        return CreatedAtAction(
            nameof(GetAll),
            null,
            new { id = productId, message = "Product created successfully. It will appear in queries after CDC sync." });
    }
}

public record CreateProductRequest(string Name, decimal Price, string? Description);
