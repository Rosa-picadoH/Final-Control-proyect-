using FinalControl.Models;
using Microsoft.EntityFrameworkCore;

namespace FinalControl.Data;

public class FinalControlDbContext(DbContextOptions<FinalControlDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleDetail> SaleDetails => Set<SaleDetail>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<User>()
            .HasIndex(user => user.UserName)
            .IsUnique();

        builder.Entity<Product>()
            .HasIndex(product => product.Code)
            .IsUnique();

        builder.Entity<Product>()
            .Property(product => product.Price)
            .HasColumnType("decimal(18,2)");

        builder.Entity<Sale>()
            .Property(sale => sale.Total)
            .HasColumnType("decimal(18,2)");

        builder.Entity<SaleDetail>()
            .Property(detail => detail.UnitPrice)
            .HasColumnType("decimal(18,2)");

        builder.Entity<Product>()
            .HasOne(product => product.Category)
            .WithMany(category => category.Products)
            .HasForeignKey(product => product.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Sale>()
            .HasOne(sale => sale.User)
            .WithMany()
            .HasForeignKey(sale => sale.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<SaleDetail>()
            .HasOne(detail => detail.Sale)
            .WithMany(sale => sale.Details)
            .HasForeignKey(detail => detail.SaleId);

        builder.Entity<SaleDetail>()
            .HasOne(detail => detail.Product)
            .WithMany()
            .HasForeignKey(detail => detail.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public static class DbSeeder
{
    public static void Seed(FinalControlDbContext db)
    {
        if (db.Users.Any())
        {
            return;
        }

        db.Users.Add(new User
        {
            FullName = "Administrador",
            UserName = "admin",
            PasswordHash = PasswordHasher.Hash("Admin123*")
        });

        var generalCategory = new Category { Name = "General" };
        db.Categories.Add(generalCategory);
        db.Products.AddRange(
            new Product
            {
                Code = "P-001",
                Name = "Teclado",
                Price = 25m,
                Stock = 20,
                Category = generalCategory
            },
            new Product
            {
                Code = "P-002",
                Name = "Mouse",
                Price = 15m,
                Stock = 30,
                Category = generalCategory
            });

        db.SaveChanges();
    }
}
