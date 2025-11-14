using CdcCqrsDemo.Application.Dtos;
using CdcCqrsDemo.Application.Interfaces;
using CleanArchitecture.Core.Application.Abstractions.Messaging;
using Microsoft.Extensions.Logging;

namespace CdcCqrsDemo.Application.Queries.GetAllProducts;

internal class GetAllProductsQueryHandler : IQueryHandler<GetAllProductsQuery, List<ProductDto>>
{
    private readonly IElasticsearchService _elasticsearchService;
    private readonly ILogger<GetAllProductsQueryHandler> _logger;

    public GetAllProductsQueryHandler(
        IElasticsearchService elasticsearchService,
        ILogger<GetAllProductsQueryHandler> logger)
    {
        _elasticsearchService = elasticsearchService;
        _logger = logger;
    }

    public async Task<List<ProductDto>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Querying all products from Elasticsearch");

        var products = await _elasticsearchService.GetAllProductsAsync(cancellationToken);

        _logger.LogInformation("Retrieved {Count} products from Elasticsearch", products.Count);

        return products;
    }
}

