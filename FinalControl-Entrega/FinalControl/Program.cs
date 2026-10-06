using FinalControl.Data;
using FinalControl.Forms;
using FinalControl.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FinalControl;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        var services = new ServiceCollection();
        const string connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=FinalControlDb;Trusted_Connection=True;TrustServerCertificate=True;";
        services.AddDbContext<FinalControlDbContext>(options => options.UseSqlServer(connectionString));
        services.AddHttpClient<ExchangeRateService>();
        services.AddSingleton<Logger>();
        services.AddScoped<AuthenticationService>();
        services.AddScoped<ProductService>();
        services.AddScoped<CategoryService>();
        services.AddScoped<SaleService>();
        services.AddScoped<ReportService>();
        services.AddScoped<DashboardService>();
        services.AddTransient<LoginForm>();

        using var provider = services.BuildServiceProvider();
        var logger = provider.GetRequiredService<Logger>();
        try
        {
            using var scope = provider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<FinalControlDbContext>();
            db.Database.Migrate();
            DbSeeder.Seed(db);
            Application.Run(scope.ServiceProvider.GetRequiredService<LoginForm>());
        }
        catch (Exception exception)
        {
            logger.Error(exception);
            MessageBox.Show("No fue posible iniciar FinalControl. Revise logs/finalcontrol.log.", "FinalControl - Error de inicio", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
