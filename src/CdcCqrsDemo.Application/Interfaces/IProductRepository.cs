using CdcCqrsDemo.Domain;
using CleanArchitecture.Core.Application.Abstractions.Persistence.Repositories;

namespace CdcCqrsDemo.Application.Interfaces;

public interface IProductRepository : IRepository<Product>
{
    Task<int> AddAsync(Product product, CancellationToken cancellationToken = default);
}


