namespace FinalControl.Models;

public class User { public int Id { get; set; } public string FullName { get; set; } = ""; public string UserName { get; set; } = ""; public string PasswordHash { get; set; } = ""; public bool Active { get; set; } = true; }
public class Category { public int Id { get; set; } public string Name { get; set; } = ""; public List<Product> Products { get; set; } = []; }
public class Product { public int Id { get; set; } public string Code { get; set; } = ""; public string Name { get; set; } = ""; public decimal Price { get; set; } public int Stock { get; set; } public int CategoryId { get; set; } public Category? Category { get; set; } public bool Active { get; set; } = true; }
public class Sale { public int Id { get; set; } public DateTime Date { get; set; } = DateTime.Now; public int UserId { get; set; } public User? User { get; set; } public decimal Total { get; set; } public List<SaleDetail> Details { get; set; } = []; }
public class SaleDetail { public int Id { get; set; } public int SaleId { get; set; } public Sale? Sale { get; set; } public int ProductId { get; set; } public Product? Product { get; set; } public int Quantity { get; set; } public decimal UnitPrice { get; set; } public decimal Subtotal => Quantity * UnitPrice; }
