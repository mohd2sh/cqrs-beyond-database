# CQRS Beyond the Database: Infrastructure-Level Architecture with CDC and Event Streaming

**Achieve true read/write separation using Change Data Capture (CDC), Debezium, Kafka, and Elasticsearch. A .NET demonstration of infrastructure-level CQRS that enables independent scaling without application-level event publishing.**

---

## What is This?

This repository demonstrates how to implement **Command Query Responsibility Segregation (CQRS)** at the infrastructure level using **Change Data Capture (CDC)** and **event streaming**. Unlike traditional CQRS implementations that only separate commands and queries in code, this approach achieves physical separation at the database layer.

**Core Technologies:**
- **.NET 8** - Application framework with CQRS patterns
- **SQL Server** - Write database with CDC enabled
- **Debezium** - CDC connector framework
- **Apache Kafka** - Event streaming platform
- **Elasticsearch** - Read-optimized database

## Architecture Overview

![High-Level Design](HighLevelDesign.png)

The architecture achieves read/write separation through an automated pipeline:

1. **Commands** write to SQL Server (transactional database)
2. **CDC** captures all database changes automatically
3. **Debezium** reads CDC logs and publishes to Kafka
4. **Kafka** streams change events reliably
5. **Elasticsearch Sink Connector** indexes data in Elasticsearch
6. **Queries** read from Elasticsearch (optimized for search)

This demonstrates **eventual consistency**: changes written to SQL Server appear in Elasticsearch within small delay through the CDC pipeline, without requiring application code modifications.

## Key Concepts

### CQRS (Command Query Responsibility Segregation)
Complete separation of read and write models. Commands write to SQL Server, queries read from Elasticsearch - each optimized for its purpose.

### Change Data Capture (CDC)
Database-level change capture eliminates the need for application-level event publishing. All database modifications are captured automatically, including direct SQL operations.

### Event Streaming
Changes flow through Kafka as a durable, scalable event stream, enabling reliable data replication and downstream processing.

### Eventual Consistency
Acceptable delay between write and read operations enables independent optimization and scaling.

## Why This Matters

**Traditional CQRS Limitations:**
- Commands and queries point to the same database
- Read and write operations compete for resources
- No scalability or performance benefits
- Pattern exists only in code, not infrastructure

**Infrastructure-Level CQRS Benefits:**
- **True Separation**: Read and write databases are physically independent
- **Independent Scaling**: Each side scales based on its workload
- **Performance Optimization**: SQL Server for transactions, Elasticsearch for queries
- **Zero Application Impact**: No code changes required for event capture
- **Complete Coverage**: All database changes captured automatically

## Quick Start

Get the complete stack running in minutes:

```bash
# 1. Start all infrastructure services
docker-compose up -d

# 2. Run the .NET API
cd src/CdcCqrsDemo.Api
dotnet run
```

**Access Points:**
- **API Swagger**: https://localhost:7000/swagger
- **Debezium UI**: http://localhost:8080
- **Kibana**: http://localhost:5601

See the [full README](../README.md) for detailed setup instructions and architecture documentation.

## See It In Action

The repository includes screenshots demonstrating the CDC pipeline:

**Step 1: Initial State**
![Initial Products](Scenario-1-GetAllProducts.png)

**Step 2: Create Product**
![Create Product](Scenario-2-CreateNewProduct.png)

**Step 3: Verify CDC Sync**
![Verify New Product](Scenario-3-GetAllProductsAndVerify.png)

This demonstrates eventual consistency: the product is written to SQL Server immediately, then appears in Elasticsearch after the CDC pipeline processes the change.

## Technology Stack

- **Application**: .NET 8 with CQRS pattern implementation
- **Write Database**: SQL Server 2019 with CDC enabled
- **CDC Framework**: Debezium SQL Server connector
- **Event Streaming**: Apache Kafka
- **Read Database**: Elasticsearch 8.10
- **Orchestration**: Docker Compose (demo only)

**Note**: While this demonstration uses SQL Server, the architecture supports any database with CDC capabilities (PostgreSQL, MySQL, MongoDB, Oracle).

## Production Considerations

This repository uses Docker Compose for demonstration purposes only. Production deployments require:

- **Infrastructure as Code**: Terraform, Pulumi, or CloudFormation
- **Container Orchestration**: Kubernetes or managed services
- **Secrets Management**: Proper credential handling
- **Monitoring**: Comprehensive logging, metrics, and tracing
- **High Availability**: Multi-region deployments and disaster recovery

See the [README](../README.md) for detailed production considerations.

## Related Resources

**Blog Post**: [Moving CQRS Beyond the Database: System Design with Debezium, Kafka, and Elasticsearch](https://medium.com/@mohd2sh/moving-cqrs-beyond-the-database-system-design-with-debezium-kafka-and-elasticsearch-2e2b425a5fcb)

**Application Architecture Template**: [CleanArchitecture-DDD-CQRS](https://github.com/mohd2sh/CleanArchitecture-DDD-CQRS) - This project uses NuGet packages from this repository for CQRS abstractions, DDD patterns, and repository interfaces.

## Repository

**GitHub**: [View Repository](https://github.com/mohd2sh/cqrs-beyond-database)

The repository includes:
- Complete .NET 8 implementation
- Docker Compose infrastructure setup
- Database initialization scripts
- Connector configurations
- Comprehensive documentation
- Working examples and screenshots

## When to Use This Approach

This architecture is appropriate when:

- System faces high read volumes or complex queries
- Need for full-text search, analytics, or flexible filtering
- Team is invested in Kafka or event-driven integration
- Eventual consistency is acceptable for performance gains
- Independent scaling of read and write workloads is required

For smaller systems with straightforward queries, the additional infrastructure may not justify the complexity. But for large, data-intensive applications, this approach transforms CQRS from a code pattern into a real architectural boundary.

---

**CQRS was never meant to be a naming convention—it was meant to decouple read and write concerns in both design and infrastructure.**

Using Debezium and Kafka to replicate data changes from a transactional database into Elasticsearch provides that separation in a practical, maintainable way for .NET systems.

---

*This is a demonstration project for educational and design exploration purposes.*

