using FinalControl.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace FinalControl.Data;
public class FinalControlDbContext(DbContextOptions<FinalControlDbContext> options) : DbContext(options)
{
 public DbSet<User> Users => Set<User>(); public DbSet<Category> Categories => Set<Category>(); public DbSet<Product> Products => Set<Product>(); public DbSet<Sale> Sales => Set<Sale>(); public DbSet<SaleDetail> SaleDetails => Set<SaleDetail>();
 protected override void OnModelCreating(ModelBuilder b) { b.Entity<User>().HasIndex(x=>x.UserName).IsUnique(); b.Entity<Product>().HasIndex(x=>x.Code).IsUnique(); b.Entity<Product>().Property(x=>x.Price).HasColumnType("decimal(18,2)"); b.Entity<Sale>().Property(x=>x.Total).HasColumnType("decimal(18,2)"); b.Entity<SaleDetail>().Property(x=>x.UnitPrice).HasColumnType("decimal(18,2)"); b.Entity<Product>().HasOne(x=>x.Category).WithMany(x=>x.Products).HasForeignKey(x=>x.CategoryId).OnDelete(DeleteBehavior.Restrict); b.Entity<Sale>().HasOne(x=>x.User).WithMany().HasForeignKey(x=>x.UserId).OnDelete(DeleteBehavior.Restrict); b.Entity<SaleDetail>().HasOne(x=>x.Sale).WithMany(x=>x.Details).HasForeignKey(x=>x.SaleId); b.Entity<SaleDetail>().HasOne(x=>x.Product).WithMany().HasForeignKey(x=>x.ProductId).OnDelete(DeleteBehavior.Restrict); }
}
public static class DbSeeder { public static void Seed(FinalControlDbContext db) { if (db.Users.Any()) return; db.Users.Add(new User { FullName="Administrador", UserName="admin", PasswordHash=Hash("Admin123*") }); var c=new Category{Name="General"}; db.Categories.Add(c); db.Products.AddRange(new Product{Code="P-001",Name="Teclado",Price=25m,Stock=20,Category=c},new Product{Code="P-002",Name="Mouse",Price=15m,Stock=30,Category=c}); db.SaveChanges(); } public static string Hash(string text)=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))); }
