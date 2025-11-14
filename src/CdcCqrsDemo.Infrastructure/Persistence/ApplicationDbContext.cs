using CdcCqrsDemo.Domain;
using Microsoft.EntityFrameworkCore;

namespace CdcCqrsDemo.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("Products");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("Id").ValueGeneratedOnAdd();
            entity.Property(e => e.Name).HasColumnName("Name").IsRequired().HasMaxLength(200);
            entity.Property(e => e.Price).HasColumnName("Price").HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(e => e.Description).HasColumnName("Description");
            entity.Property(e => e.UpdatedAt).HasColumnName("UpdatedAt").IsRequired();
        });

        base.OnModelCreating(modelBuilder);
    }
}


