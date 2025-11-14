using CdcCqrsDemo.Application.Dtos;
using CleanArchitecture.Core.Application.Abstractions.Persistence.Repositories;

namespace CdcCqrsDemo.Application.Interfaces;

public interface IElasticsearchService : IReadRepository
{
    Task<List<ProductDto>> GetAllProductsAsync(CancellationToken cancellationToken = default);
}


