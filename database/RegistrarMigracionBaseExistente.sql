/* Ejecute este script SOLO si ya tenía FinalControlDb creada con la versión
   anterior que usaba EnsureCreated(). No elimina datos; registra el esquema
   existente como la migración inicial antes de abrir la nueva aplicación. */
USE FinalControlDb;
GO
IF OBJECT_ID('__EFMigrationsHistory', 'U') IS NULL
    CREATE TABLE __EFMigrationsHistory (MigrationId nvarchar(150) NOT NULL PRIMARY KEY, ProductVersion nvarchar(32) NOT NULL);
GO
IF NOT EXISTS (SELECT 1 FROM __EFMigrationsHistory WHERE MigrationId = '202609010001_InitialCreate')
    INSERT INTO __EFMigrationsHistory(MigrationId, ProductVersion) VALUES ('202609010001_InitialCreate', '8.0.8');
GO
