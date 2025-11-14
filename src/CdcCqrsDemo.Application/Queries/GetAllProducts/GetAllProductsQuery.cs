using CdcCqrsDemo.Application.Dtos;
using CleanArchitecture.Core.Application.Abstractions.Messaging;

namespace CdcCqrsDemo.Application.Queries.GetAllProducts;

public record GetAllProductsQuery : IQuery<List<ProductDto>>;

