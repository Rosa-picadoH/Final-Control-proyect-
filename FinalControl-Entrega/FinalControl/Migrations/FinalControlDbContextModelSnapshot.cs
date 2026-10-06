using System;
using FinalControl.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace FinalControl.Migrations;

[DbContext(typeof(FinalControlDbContext))]
partial class FinalControlDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("ProductVersion", "8.0.8").HasAnnotation("Relational:MaxIdentifierLength", 128).UseIdentityColumns();
        modelBuilder.Entity("FinalControl.Models.Category", b =>
        {
            b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int").UseIdentityColumn(); b.Property<string>("Name").IsRequired().HasColumnType("nvarchar(max)"); b.HasKey("Id"); b.ToTable("Categories");
        });
        modelBuilder.Entity("FinalControl.Models.Product", b =>
        {
            b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int").UseIdentityColumn(); b.Property<bool>("Active").HasColumnType("bit"); b.Property<int>("CategoryId").HasColumnType("int"); b.Property<string>("Code").IsRequired().HasColumnType("nvarchar(450)"); b.Property<string>("Name").IsRequired().HasColumnType("nvarchar(max)"); b.Property<decimal>("Price").HasColumnType("decimal(18,2)"); b.Property<int>("Stock").HasColumnType("int"); b.HasKey("Id"); b.HasIndex("CategoryId"); b.HasIndex("Code").IsUnique(); b.ToTable("Products");
        });
        modelBuilder.Entity("FinalControl.Models.Sale", b =>
        {
            b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int").UseIdentityColumn(); b.Property<DateTime>("Date").HasColumnType("datetime2"); b.Property<decimal>("Total").HasColumnType("decimal(18,2)"); b.Property<int>("UserId").HasColumnType("int"); b.HasKey("Id"); b.HasIndex("UserId"); b.ToTable("Sales");
        });
        modelBuilder.Entity("FinalControl.Models.SaleDetail", b =>
        {
            b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int").UseIdentityColumn(); b.Property<int>("ProductId").HasColumnType("int"); b.Property<int>("Quantity").HasColumnType("int"); b.Property<int>("SaleId").HasColumnType("int"); b.Property<decimal>("UnitPrice").HasColumnType("decimal(18,2)"); b.HasKey("Id"); b.HasIndex("ProductId"); b.HasIndex("SaleId"); b.ToTable("SaleDetails");
        });
        modelBuilder.Entity("FinalControl.Models.User", b =>
        {
            b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int").UseIdentityColumn(); b.Property<bool>("Active").HasColumnType("bit"); b.Property<string>("FullName").IsRequired().HasColumnType("nvarchar(max)"); b.Property<string>("PasswordHash").IsRequired().HasColumnType("nvarchar(max)"); b.Property<string>("UserName").IsRequired().HasColumnType("nvarchar(450)"); b.HasKey("Id"); b.HasIndex("UserName").IsUnique(); b.ToTable("Users");
        });
        modelBuilder.Entity("FinalControl.Models.Product", b => { b.HasOne("FinalControl.Models.Category", "Category").WithMany("Products").HasForeignKey("CategoryId").OnDelete(DeleteBehavior.Restrict).IsRequired(); b.Navigation("Category"); });
        modelBuilder.Entity("FinalControl.Models.Sale", b => { b.HasOne("FinalControl.Models.User", "User").WithMany().HasForeignKey("UserId").OnDelete(DeleteBehavior.Restrict).IsRequired(); b.Navigation("User"); });
        modelBuilder.Entity("FinalControl.Models.SaleDetail", b => { b.HasOne("FinalControl.Models.Product", "Product").WithMany().HasForeignKey("ProductId").OnDelete(DeleteBehavior.Restrict).IsRequired(); b.HasOne("FinalControl.Models.Sale", "Sale").WithMany("Details").HasForeignKey("SaleId").OnDelete(DeleteBehavior.Cascade).IsRequired(); b.Navigation("Product"); b.Navigation("Sale"); });
        modelBuilder.Entity("FinalControl.Models.Category", b => b.Navigation("Products"));
        modelBuilder.Entity("FinalControl.Models.Sale", b => b.Navigation("Details"));
    }
}
