using CdcCqrsDemo.Application.Dtos;
using CdcCqrsDemo.Application.Interfaces;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Microsoft.Extensions.Logging;

namespace CdcCqrsDemo.Infrastructure.Elasticsearch;

public class ElasticsearchService : IElasticsearchService
{
    private readonly ElasticsearchClient _elasticsearchClient;
    private readonly ILogger<ElasticsearchService> _logger;

    public ElasticsearchService(
        ElasticsearchClient elasticsearchClient,
        ILogger<ElasticsearchService> logger)
    {
        _elasticsearchClient = elasticsearchClient;
        _logger = logger;
    }

    public async Task<List<ProductDto>> GetAllProductsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Querying all products from Elasticsearch");

        try
        {
            var response = await _elasticsearchClient.SearchAsync<ElasticsearchProduct>(
                s => s
                    .Indices("products")
                    .Size(100)
                    .Query(q => q.MatchAll(new MatchAllQuery())),
                cancellationToken);

            if (response.IsValidResponse)
            {
                var products = response.Documents
                    .Where(doc => doc != null)
                    .Select(doc => MapToDto(doc!))
                    .ToList();

                _logger.LogInformation("Retrieved {Count} products from Elasticsearch", products.Count);
                return products;
            }

            _logger.LogWarning("Failed to retrieve products from Elasticsearch");
            return new List<ProductDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error querying products from Elasticsearch");
            throw;
        }
    }

    private static ProductDto MapToDto(ElasticsearchProduct product)
    {
        DateTime updatedAt = GetUpdatedAt(product);

        // For demo only Handle Price
        decimal price = 0;
        if (product.Price is string priceStr)
        {
            // Try to parse as decimal first
            if (decimal.TryParse(priceStr, out var parsedPrice))
            {
                price = parsedPrice;
            }
        }
        else if (product.Price is decimal dec)
        {
            price = dec;
        }

        return new ProductDto(
            product.Id,
            product.Name ?? string.Empty,
            price,
            product.Description,
            updatedAt
        );
    }

    private static DateTime GetUpdatedAt(ElasticsearchProduct product)
    {
        if (product.UpdatedAt is double ticks && ticks > 0)
        {
            // Convert ticks to DateTime
            return new DateTime((long)ticks, DateTimeKind.Utc);
        }

        if (product.UpdatedAt is DateTime datetime)
        {
            return datetime;
        }

        return DateTime.UtcNow;
    }
}

/// <summary>
/// Elasticsearch document model for products. Uses object types to handle varying data formats from CDC pipeline.
/// </summary>
internal class ElasticsearchProduct
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public object? Price { get; set; }
    public string? Description { get; set; }
    public object? UpdatedAt { get; set; }
}

