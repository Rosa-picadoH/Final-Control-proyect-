# Evidencia frente a la guía de evaluación

| Criterio | Evidencia verificable |
|---|---|
| .NET 8 y POO | `FinalControl.csproj`, `Models.cs` y entidades del dominio. |
| DI y desacoplamiento | `Program.cs` registra dependencias; formularios reciben servicios por constructor. |
| UI/UX moderna | MaterialSkin.2, login, panel con indicadores, búsqueda y acciones claras. |
| Login | `AuthenticationService` consulta usuarios activos; demo `admin` / `Admin123*`. |
| Dos mantenimientos | Formularios de Productos y Categorías con nuevo, guardar, búsqueda/selección y validaciones. |
| EF Core Code First | `Data.cs`, `Migrations/` y `Database.Migrate()` en `Program.cs`. |
| Relaciones | Fluent API en `OnModelCreating` y diagrama en `MANUAL_TECNICO.md`. |
| SQL seguro | LINQ/EF Core; no se concatena entrada del usuario para construir SQL. |
| Transacción | `SaleService.CreateAsync` usa `BeginTransactionAsync`, `CommitAsync` y `RollbackAsync`. |
| Concurrencia | Consultas async, reloj con Timer, API async y PDF generado fuera del hilo de UI. |
| API REST | `ExchangeRateService`: HttpClient + System.Text.Json para USD/NIO. |
| Reporte | `ReportService`: inventario PDF desde datos de EF Core mediante QuestPDF. |
| Errores | `Logger` guarda excepciones en `logs/finalcontrol.log`. |
| Release y SQL | `publish-release.ps1` genera carpeta Release y copia el script SQL. |
| GitHub | Debe verificarse en el repositorio real del equipo; no se sustituye por documentación local. |
