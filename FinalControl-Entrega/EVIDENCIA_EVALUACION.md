# Evidencia frente a la guía de evaluación

| Criterio | Evidencia verificable |
|---|---|
| .NET 8 y POO | `FinalControl.csproj`, `Models.cs` y entidades del dominio. |
| DI y desacoplamiento | `Program.cs` registra dependencias; formularios reciben servicios por constructor. |
| UI/UX moderna | Login con panel visual y mostrar contraseña; navegación lateral, tarjetas clicables, accesos rápidos, búsqueda al escribir, tablas con filas alternadas y resaltado de stock bajo; carrito con total y contador. |
| Login | `AuthenticationService` consulta usuarios activos; `PasswordHasher.cs` protege claves nuevas y actualiza hashes SHA-256 antiguos al iniciar sesión correctamente; demo `admin` / `Admin123*`. |
| Dos mantenimientos | Formularios de Productos y Categorías con nuevo, guardar, búsqueda/selección y validaciones. |
| EF Core Code First | `Data.cs`, `Migrations/` y `Database.Migrate()` en `Program.cs`. |
| Relaciones | Fluent API en `OnModelCreating` y diagrama en `MANUAL_TECNICO.md`. |
| SQL seguro | LINQ/EF Core; no se concatena entrada del usuario para construir SQL. |
| Transacción | `SaleService.CreateAsync` usa `BeginTransactionAsync`, `CommitAsync`, `RollbackAsync` y descuento condicional de stock para evitar sobreventa concurrente. |
| Concurrencia | Consultas async sin seguimiento para datos de lectura, reloj con Timer, API async con espera máxima y PDF generado fuera del hilo de UI. |
| API REST | `ExchangeRateService`: HttpClient + System.Text.Json para USD/NIO. |
| Reporte | `ReportService`: inventario PDF desde datos de EF Core mediante QuestPDF. |
| Errores | `Logger` guarda excepciones en `%LOCALAPPDATA%\FinalControl\logs\finalcontrol.log`. |
| Release y SQL | `publish-release.ps1` genera carpeta Release y copia el script SQL. |
| GitHub | Debe verificarse en el repositorio real del equipo; no se sustituye por documentación local. |
