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
    private readonly object _sync = new();
    private readonly string _path = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "FinalControl",
        "logs",
        "finalcontrol.log");

    public string FilePath => _path;

    public void Error(Exception exception)
    {
        try
        {
            lock (_sync)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
                File.AppendAllText(_path, $"{DateTimeOffset.Now:O} | {exception}{Environment.NewLine}");
            }
        }
        catch (Exception writeException) when (writeException is IOException
            or UnauthorizedAccessException
            or System.Security.SecurityException)
        {
            // The UI still reports the original error if the log directory is unavailable.
        }
    }
}

public sealed class AuthenticationService(FinalControlDbContext db)
{
    public async Task<User?> LoginAsync(string userName, string password)
    {
        var user = await db.Users.SingleOrDefaultAsync(
            candidate => candidate.UserName == userName && candidate.Active);
        if (user is null)
        {
            return null;
        }

        var verification = await Task.Run(() => PasswordHasher.Verify(user.PasswordHash, password));
        if (!verification.IsValid)
        {
            return null;
        }

        if (verification.NeedsUpgrade)
        {
            user.PasswordHash = await Task.Run(() => PasswordHasher.Hash(password));
            await db.SaveChangesAsync();
        }

        return user;
    }
}

public sealed class ProductService(FinalControlDbContext db)
{
    public Task<List<Product>> GetAsync(bool includeInactive = true) => db.Products.AsNoTracking().Include(product => product.Category)
        .Where(product => includeInactive || product.Active).OrderBy(product => product.Name).ToListAsync();

    public async Task SaveAsync(Product product)
    {
        if (string.IsNullOrWhiteSpace(product.Code)) throw new InvalidOperationException("El código es obligatorio.");
        if (string.IsNullOrWhiteSpace(product.Name)) throw new InvalidOperationException("El nombre es obligatorio.");
        product.Code = product.Code.Trim();
        product.Name = product.Name.Trim();
        if (product.Code.Length < 3) throw new InvalidOperationException("El código debe tener al menos 3 caracteres.");
        if (product.Name.Length < 3) throw new InvalidOperationException("El nombre debe tener al menos 3 caracteres.");
        if (product.Price < 0 || product.Stock < 0 || product.CategoryId <= 0) throw new InvalidOperationException("Precio, existencias y categoría deben ser válidos.");
        if (!await db.Categories.AnyAsync(category => category.Id == product.CategoryId)) throw new InvalidOperationException("Seleccione una categoría válida.");
        product.Category = null;
        if (product.Id == 0)
        {
            db.Products.Add(product);
        }
        else
        {
            db.Products.Attach(product);
            db.Entry(product).State = EntityState.Modified;
        }

        await db.SaveChangesAsync();
    }

    public async Task SetActiveAsync(Product product, bool active)
    {
        var storedProduct = await db.Products.SingleOrDefaultAsync(candidate => candidate.Id == product.Id)
            ?? throw new InvalidOperationException("El producto ya no existe.");
        storedProduct.Active = active;
        await db.SaveChangesAsync();
    }
}

public sealed class CategoryService(FinalControlDbContext db)
{
    public Task<List<Category>> GetAsync() => db.Categories.AsNoTracking().OrderBy(category => category.Name).ToListAsync();
    public async Task SaveAsync(Category category)
    {
        if (string.IsNullOrWhiteSpace(category.Name)) throw new InvalidOperationException("El nombre de la categoría es obligatorio.");
        category.Name = category.Name.Trim();
        if (category.Name.Length < 3) throw new InvalidOperationException("El nombre de la categoría debe tener al menos 3 caracteres.");
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
        ArgumentNullException.ThrowIfNull(lines);
        if (userId <= 0) throw new InvalidOperationException("El usuario de la venta no es válido.");
        if (lines.Count == 0) throw new InvalidOperationException("Agregue al menos un producto a la venta.");

        if (lines.Any(line => line.ProductId <= 0 || line.Quantity <= 0))
        {
            throw new InvalidOperationException("Cada producto debe tener un identificador y una cantidad válidos.");
        }

        var requestedLines = lines
            .GroupBy(line => line.ProductId)
            .Select(group => new SaleLine(
                group.Key,
                group.Aggregate(0, (total, line) => checked(total + line.Quantity))))
            .ToArray();

        await using var transaction = await db.Database.BeginTransactionAsync();
        var sale = new Sale { UserId = userId };
        try
        {
            foreach (var line in requestedLines)
            {
                var product = await db.Products.AsNoTracking()
                    .SingleOrDefaultAsync(candidate => candidate.Id == line.ProductId)
                    ?? throw new InvalidOperationException("Producto no encontrado.");

                if (!product.Active)
                {
                    throw new InvalidOperationException($"El producto {product.Name} está inactivo.");
                }

                var affectedRows = await db.Products
                    .Where(candidate => candidate.Id == product.Id
                        && candidate.Active
                        && candidate.Stock >= line.Quantity)
                    .ExecuteUpdateAsync(update => update
                        .SetProperty(candidate => candidate.Stock, candidate => candidate.Stock - line.Quantity));

                if (affectedRows == 0)
                {
                    throw new InvalidOperationException(
                        $"No hay existencias suficientes o el producto dejó de estar activo: {product.Name}.");
                }

                sale.Details.Add(new SaleDetail { ProductId = product.Id, Quantity = line.Quantity, UnitPrice = product.Price });
            }

            sale.Total = sale.Details.Sum(detail => detail.Subtotal);
            db.Sales.Add(sale);
            await db.SaveChangesAsync();
            await transaction.CommitAsync();
            return sale;
        }
        catch
        {
            try
            {
                await transaction.RollbackAsync();
            }
            catch
            {
                // Preserve the original sale error if the connection also rejects rollback.
            }

            foreach (var detail in sale.Details)
            {
                db.Entry(detail).State = EntityState.Detached;
            }

            db.Entry(sale).State = EntityState.Detached;
            throw;
        }
    }
}

public sealed class ExchangeRateService(HttpClient http)
{
    public async Task<string> GetAsync()
    {
        try
        {
            using var response = await http.GetAsync("https://open.er-api.com/v6/latest/USD");
            response.EnsureSuccessStatusCode();
            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
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
        var file = Path.Combine(directory, $"Inventario_{DateTime.Now:yyyyMMdd_HHmmssfff}.pdf");
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
