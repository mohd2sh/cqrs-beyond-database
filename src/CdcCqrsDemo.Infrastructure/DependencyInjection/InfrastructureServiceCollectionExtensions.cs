using CdcCqrsDemo.Application.Interfaces;
using CdcCqrsDemo.Infrastructure.Elasticsearch;
using CdcCqrsDemo.Infrastructure.Persistence;
using CdcCqrsDemo.Infrastructure.Persistence.Repositories;
using CleanArchitecture.Core.Infrastructure;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Serialization;
using Elastic.Transport;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json.Serialization;

namespace CdcCqrsDemo.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {

        services.AddCoreInfrastructure();
        // Add EF Core
        var sqlConnectionString = configuration.GetConnectionString("SqlServer")
            ?? throw new InvalidOperationException("SqlServer connection string is not configured");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(sqlConnectionString));

        services.AddScoped<IProductRepository, ProductRepository>();

        // Add Elasticsearch
        var elasticsearchUri = configuration.GetConnectionString("Elasticsearch");

        if (string.IsNullOrEmpty(elasticsearchUri))
        {
            throw new InvalidOperationException("Elasticsearch connection string is not configured");
        }

        services.AddSingleton(sp =>
        {
            var nodePool = new SingleNodePool(new Uri(elasticsearchUri));

            var settings = new ElasticsearchClientSettings(nodePool, sourceSerializer: (defaultSerializer, settings) => new DefaultSourceSerializer(settings, options =>
            {
                options.PropertyNameCaseInsensitive = true;
                options.PropertyNamingPolicy = null;
                options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            }));

            settings.DisableDirectStreaming();

            return new ElasticsearchClient(settings);
        });

        // Add Elasticsearch Service
        services.AddScoped<IElasticsearchService, ElasticsearchService>();

        return services;
    }
}


