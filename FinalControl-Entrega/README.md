# FinalControl

Aplicación de escritorio para administrar inventario y ventas. Fue construida con C# .NET 8, Windows Forms, Entity Framework Core y SQL Server LocalDB.

## Funcionalidades

- Inicio de sesión con usuarios activos.
- Panel con indicadores de productos, existencias, ventas del día, reloj y tipo de cambio USD/NIO.
- CRUD de categorías y productos, con búsqueda, validaciones y activación/desactivación lógica.
- Registro de ventas con validación de stock y transacción atómica (commit/rollback).
- Reporte PDF de inventario generado desde datos de EF Core.
- Registro estructurado de errores en `logs/finalcontrol.log`.
- Interfaz moderna con MaterialSkin.2 y operaciones asíncronas para datos, API y reporte.

## Requisitos

- Windows 10/11, .NET 8 Desktop Runtime y SQL Server LocalDB.
- Para desarrollar: Visual Studio 2022 con la carga de trabajo **Desarrollo de escritorio con .NET**.

## Ejecución desde Visual Studio

1. Abra `FinalControl.sln`.
2. Restaure paquetes NuGet y compile la solución.
3. Ejecute el proyecto. En el primer inicio se aplican las migraciones y se crean datos de prueba.
4. Inicie sesión con `admin` / `Admin123*`.

## Base de datos y migraciones

El proyecto incluye la migración inicial en `FinalControl/Migrations`. La aplicación ejecuta `Database.Migrate()` al iniciar; por tanto, para una instalación nueva no es necesario ejecutar SQL manualmente.

Para administrar migraciones desde la carpeta `FinalControl`:

```powershell
dotnet ef migrations add NombreDelCambio
dotnet ef database update
```

El script `database/FinalControlDb.sql` se entrega como alternativa para instalación manual en SQL Server y registra la migración inicial.

## Publicación Release

Desde la carpeta raíz ejecute:

```powershell
./publish-release.ps1
```

El ejecutable y sus dependencias se generan en `Release/FinalControl`. El script SQL queda incluido en la carpeta `Release`.

## Evidencia para la defensa

1. Muestre login y panel principal.
2. Cree una categoría o producto; búsquelo y edítelo.
3. Registre una venta y vuelva a Productos para comprobar que disminuyó el stock.
4. Muestre `SaleService.CreateAsync`: contiene la transacción y el rollback.
5. Muestre `Data.cs` y `Migrations`: DbSet, Fluent API y migración inicial.
6. Genere el PDF y señale la carpeta indicada por el mensaje.
7. Si no hay internet, la aplicación muestra un aviso de tipo de cambio no disponible sin bloquearse.

## GitHub

Publique únicamente el historial de cambios reales del equipo. No cree commits con fechas falsas ni presente cambios de última hora como si hubieran sido hechos en fases anteriores.
