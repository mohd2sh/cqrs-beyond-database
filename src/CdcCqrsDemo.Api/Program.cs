using CdcCqrsDemo.Application;
using CdcCqrsDemo.Infrastructure.DependencyInjection;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();

builder.Services.AddInfrastructure(builder.Configuration);

// Add controllers
builder.Services.AddControllers();

// Add health checks
builder.Services.AddHealthChecks()
    .AddCheck("sql-server", () =>
    {
        try
        {
            using var connection = new SqlConnection(builder.Configuration.GetConnectionString("SqlServer"));
            connection.Open();
            return HealthCheckResult.Healthy("SQL Server is healthy");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("SQL Server is unhealthy", ex);
        }
    })
    .AddAsyncCheck("elasticsearch", async () =>
    {
        try
        {
            var elasticsearchUri = builder.Configuration.GetConnectionString("Elasticsearch")
                ?? "http://localhost:9200";
            var client = new Elastic.Clients.Elasticsearch.ElasticsearchClient(
                new Elastic.Clients.Elasticsearch.ElasticsearchClientSettings(new Uri(elasticsearchUri)));
            var response = await client.PingAsync();
            return response.IsValidResponse
                ? HealthCheckResult.Healthy("Elasticsearch is healthy")
                : HealthCheckResult.Unhealthy("Elasticsearch is unhealthy");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Elasticsearch is unhealthy", ex);
        }
    });

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "CDC CQRS Demo API",
        Version = "v1",
        Description = "Demonstrates CQRS pattern with CDC from SQL Server to Elasticsearch"
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();
app.MapHealthChecks("/health");

app.Logger.LogInformation("CDC CQRS Demo API started");
app.Logger.LogInformation("Swagger UI available at: /swagger");
app.Logger.LogInformation("Health checks available at: /health");

app.Run();
