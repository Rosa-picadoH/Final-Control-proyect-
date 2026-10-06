using FinalControl.Data;
using FinalControl.Models;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Text.Json;

namespace FinalControl.Services;

public sealed class Logger
{
    private readonly string _path = Path.Combine(AppContext.BaseDirectory, "logs", "finalcontrol.log");
    public void Error(Exception exception)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        File.AppendAllText(_path, $"{DateTime.Now:O} | {exception}\n");
    }
}

public sealed class AuthenticationService(FinalControlDbContext db)
{
    public Task<User?> LoginAsync(string userName, string password) =>
        db.Users.FirstOrDefaultAsync(user => user.UserName == userName && user.PasswordHash == DbSeeder.Hash(password) && user.Active);
}

public sealed class ProductService(FinalControlDbContext db)
{
    public Task<List<Product>> GetAsync(bool includeInactive = true) => db.Products.Include(product => product.Category)
        .Where(product => includeInactive || product.Active).OrderBy(product => product.Name).ToListAsync();

    public async Task SaveAsync(Product product)
    {
        if (string.IsNullOrWhiteSpace(product.Code) || product.Code.Trim().Length < 3) throw new InvalidOperationException("El código debe tener al menos 3 caracteres.");
        if (string.IsNullOrWhiteSpace(product.Name) || product.Name.Trim().Length < 3) throw new InvalidOperationException("El nombre debe tener al menos 3 caracteres.");
        if (product.Price < 0 || product.Stock < 0 || product.CategoryId <= 0) throw new InvalidOperationException("Precio, existencias y categoría deben ser válidos.");
        if (product.Id == 0) db.Products.Add(product); else db.Products.Update(product);
        await db.SaveChangesAsync();
    }

    public async Task SetActiveAsync(Product product, bool active) { product.Active = active; await db.SaveChangesAsync(); }
}

public sealed class CategoryService(FinalControlDbContext db)
{
    public Task<List<Category>> GetAsync() => db.Categories.OrderBy(category => category.Name).ToListAsync();
    public async Task SaveAsync(Category category)
    {
        if (string.IsNullOrWhiteSpace(category.Name) || category.Name.Trim().Length < 3) throw new InvalidOperationException("El nombre de la categoría debe tener al menos 3 caracteres.");
        if (category.Id == 0) db.Categories.Add(category); else db.Categories.Update(category);
        await db.SaveChangesAsync();
    }
    public async Task DeleteAsync(Category category)
    {
        if (await db.Products.AnyAsync(product => product.CategoryId == category.Id)) throw new InvalidOperationException("No puede eliminar una categoría que tiene productos asociados.");
        db.Categories.Remove(category); await db.SaveChangesAsync();
    }
}

public record SaleLine(int ProductId, int Quantity);

public sealed class SaleService(FinalControlDbContext db)
{
    public async Task<Sale> CreateAsync(int userId, IReadOnlyCollection<SaleLine> lines)
    {
        if (lines.Count == 0) throw new InvalidOperationException("Agregue al menos un producto a la venta.");
        await using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            var sale = new Sale { UserId = userId };
            foreach (var line in lines)
            {
                if (line.Quantity <= 0) throw new InvalidOperationException("La cantidad debe ser mayor que cero.");
                var product = await db.Products.FindAsync(line.ProductId) ?? throw new InvalidOperationException("Producto no encontrado.");
                if (!product.Active || product.Stock < line.Quantity) throw new InvalidOperationException($"Stock insuficiente para: {product.Name}.");
                product.Stock -= line.Quantity;
                sale.Details.Add(new SaleDetail { ProductId = product.Id, Quantity = line.Quantity, UnitPrice = product.Price });
            }
            sale.Total = sale.Details.Sum(detail => detail.Subtotal);
            db.Sales.Add(sale); await db.SaveChangesAsync(); await transaction.CommitAsync(); return sale;
        }
        catch { await transaction.RollbackAsync(); throw; }
    }
}

public sealed class ExchangeRateService(HttpClient http)
{
    public async Task<string> GetAsync()
    {
        try
        {
            using var response = await http.GetAsync("https://open.er-api.com/v6/latest/USD");
            response.EnsureSuccessStatusCode(); using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            return $"USD/NIO: C$ {document.RootElement.GetProperty("rates").GetProperty("NIO").GetDecimal():N2}";
        }
        catch { return "Tipo de cambio no disponible"; }
    }
}

public sealed class DashboardService(FinalControlDbContext db)
{
    public async Task<(int Products, int Units, int SalesToday)> GetSummaryAsync()
    {
        var today = DateTime.Today;
        return (await db.Products.CountAsync(product => product.Active), await db.Products.Where(product => product.Active).SumAsync(product => (int?)product.Stock) ?? 0, await db.Sales.CountAsync(sale => sale.Date >= today));
    }
}

public sealed class ReportService(FinalControlDbContext db)
{
    public async Task<string> InventoryPdfAsync()
    {
        var products = await db.Products.Include(product => product.Category).Where(product => product.Active).OrderBy(product => product.Name).ToListAsync();
        var directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "FinalControl", "Reportes");
        Directory.CreateDirectory(directory);
        var file = Path.Combine(directory, $"Inventario_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
        QuestPDF.Settings.License = LicenseType.Community;
        await Task.Run(() => Document.Create(container => container.Page(page =>
        {
            page.Margin(35);
            page.Header().Text("FinalControl - Reporte de Inventario").FontSize(20).Bold().FontColor(Colors.Teal.Darken2);
            page.Content().Table(table =>
            {
                table.ColumnsDefinition(columns => { columns.RelativeColumn(); columns.RelativeColumn(3); columns.RelativeColumn(2); columns.RelativeColumn(); columns.RelativeColumn(); });
                table.Header(header => { foreach (var title in new[] { "Código", "Producto", "Categoría", "Stock", "Precio" }) header.Cell().Background(Colors.Teal.Darken2).Padding(6).Text(title).FontColor(Colors.White).Bold(); });
                foreach (var product in products) { table.Cell().Padding(5).Text(product.Code); table.Cell().Padding(5).Text(product.Name); table.Cell().Padding(5).Text(product.Category?.Name ?? "Sin categoría"); table.Cell().Padding(5).Text(product.Stock.ToString()); table.Cell().Padding(5).Text($"C$ {product.Price:N2}"); }
            });
            page.Footer().AlignCenter().Text($"Generado el {DateTime.Now:g} | Productos activos: {products.Count}");
        })).GeneratePdf(file));
        return file;
    }
}
