# FinalControl

Aplicación de escritorio para administrar inventario y ventas, construida con C# .NET 8, Windows Forms, Entity Framework Core y SQL Server LocalDB.

## Funcionalidades

- Inicio de sesión con usuarios activos y contraseñas protegidas con PBKDF2-SHA256.
- Actualización automática de contraseñas SHA-256 antiguas al iniciar sesión correctamente.
- Panel con indicadores de productos, existencias, ventas del día, reloj y tipo de cambio USD/NIO.
- Mantenimiento de categorías y productos, búsqueda, validaciones y activación lógica.
- Registro de ventas transaccional con descuento atómico de existencias para evitar ventas concurrentes por encima del stock.
- Reporte PDF de inventario generado desde datos de EF Core.
- Registro de errores en `%LOCALAPPDATA%\FinalControl\logs\finalcontrol.log`.
- Interfaz renovada con panel lateral, tarjetas interactivas, accesos rápidos, búsqueda en productos y categorías, indicadores de stock bajo y carrito con total visible.
- Acceso con panel visual dividido y opción para mostrar u ocultar la contraseña.
- Interfaz MaterialSkin.2 y operaciones asíncronas para datos, API y reportes.
- Espera máxima de ocho segundos para la consulta del tipo de cambio.

## Acceso de demostración

En una base nueva, se crea `admin` / `Admin123*` para la demostración académica. No use esa contraseña ni la cuenta de demostración en una instalación con datos reales.

## Requisitos

- Windows 10/11, .NET 8 Desktop Runtime y SQL Server LocalDB.
- Para desarrollar: Visual Studio 2022 con la carga de trabajo **Desarrollo de escritorio con .NET**.

## Ejecución desde Visual Studio

1. Abra `FinalControl.sln`.
2. Restaure paquetes NuGet y compile la solución.
3. Ejecute el proyecto. En el primer inicio se aplican las migraciones y se crean datos de demostración.
4. Inicie sesión con las credenciales de demostración indicadas arriba.

## Base de datos y migraciones

El proyecto usa la base `FinalControlDbFinal` en `(localdb)\MSSQLLocalDB`. Incluye una migración inicial en `FinalControl/Migrations` y aplica `Database.Migrate()` al iniciar. Para una instalación nueva no es necesario ejecutar SQL manualmente.

Para administrar migraciones desde la carpeta `FinalControl`:

```powershell
dotnet ef migrations add NombreDelCambio
dotnet ef database update
```

El script `database/FinalControlDb.sql` se entrega como alternativa para instalación manual en SQL Server y registra la migración inicial.

Si una base existente conserva hashes SHA-256 de versiones anteriores, el siguiente inicio de sesión correcto los cambia a PBKDF2-SHA256 automáticamente. No se requiere modificar el esquema.

Si ya ejecutó una versión anterior que usaba `EnsureCreated()`, ejecute una sola vez `database/RegistrarMigracionBaseExistente.sql` en esa base antes de abrir esta versión. El script registra la migración sin borrar datos.

## Registro de errores

La aplicación escribe fecha y detalle técnico en `%LOCALAPPDATA%\FinalControl\logs\finalcontrol.log`. Esta ubicación permite guardar el registro sin permisos de escritura en la carpeta de instalación.

## Publicación Release

Desde la carpeta raíz ejecute:

```powershell
./publish-release.ps1
```

El ejecutable y sus dependencias se generan en `Release/FinalControl`. El script SQL queda incluido en la carpeta `Release`. El ZIP de revisión no incluye el ejecutable anterior porque no se pudo regenerar sin restaurar los paquetes del proyecto.

## Evidencia para la defensa

1. Muestre login y panel principal.
2. Cree una categoría o producto; búsquelo y edítelo.
3. Registre una venta y vuelva a Productos para comprobar que disminuyó el stock.
4. Muestre `SaleService.CreateAsync`: contiene la transacción y el descuento condicional de stock.
5. Muestre `PasswordHasher.cs` y `Data.cs`: hash con sal y datos semilla.
6. Muestre `Data.cs` y `Migrations`: DbSet, Fluent API y migración inicial.
7. Genere el PDF y señale la carpeta indicada por el mensaje.
8. Si no hay internet, el tipo de cambio muestra un aviso y deja de esperar como máximo después de ocho segundos.

## Documentación

Consulte `MANUAL_TECNICO.md` para arquitectura y reglas de negocio, y `CAMBIOS_REVISION.md` para el resumen de las mejoras aplicadas.
