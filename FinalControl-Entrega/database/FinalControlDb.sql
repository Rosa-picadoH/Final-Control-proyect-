/* FinalControl - instalación manual para SQL Server LocalDB / SQL Server.
   Úselo solamente en una base FinalControlDb nueva. La aplicación crea la base
   automáticamente mediante EF Core Migrations en una instalación normal. */
IF DB_ID('FinalControlDb') IS NULL CREATE DATABASE FinalControlDb;
GO
USE FinalControlDb;
GO
IF OBJECT_ID('__EFMigrationsHistory', 'U') IS NULL
    CREATE TABLE __EFMigrationsHistory (MigrationId nvarchar(150) NOT NULL PRIMARY KEY, ProductVersion nvarchar(32) NOT NULL);
GO
CREATE TABLE Categories (Id INT IDENTITY PRIMARY KEY, Name NVARCHAR(MAX) NOT NULL);
CREATE TABLE Users (Id INT IDENTITY PRIMARY KEY, FullName NVARCHAR(MAX) NOT NULL, UserName NVARCHAR(450) NOT NULL, PasswordHash NVARCHAR(MAX) NOT NULL, Active BIT NOT NULL);
CREATE UNIQUE INDEX IX_Users_UserName ON Users(UserName);
CREATE TABLE Products (Id INT IDENTITY PRIMARY KEY, Code NVARCHAR(450) NOT NULL, Name NVARCHAR(MAX) NOT NULL, Price DECIMAL(18,2) NOT NULL, Stock INT NOT NULL, CategoryId INT NOT NULL, Active BIT NOT NULL, CONSTRAINT FK_Products_Categories_CategoryId FOREIGN KEY(CategoryId) REFERENCES Categories(Id));
CREATE UNIQUE INDEX IX_Products_Code ON Products(Code);
CREATE INDEX IX_Products_CategoryId ON Products(CategoryId);
CREATE TABLE Sales (Id INT IDENTITY PRIMARY KEY, [Date] DATETIME2 NOT NULL, UserId INT NOT NULL, Total DECIMAL(18,2) NOT NULL, CONSTRAINT FK_Sales_Users_UserId FOREIGN KEY(UserId) REFERENCES Users(Id));
CREATE INDEX IX_Sales_UserId ON Sales(UserId);
CREATE TABLE SaleDetails (Id INT IDENTITY PRIMARY KEY, SaleId INT NOT NULL, ProductId INT NOT NULL, Quantity INT NOT NULL, UnitPrice DECIMAL(18,2) NOT NULL, CONSTRAINT FK_SaleDetails_Sales_SaleId FOREIGN KEY(SaleId) REFERENCES Sales(Id) ON DELETE CASCADE, CONSTRAINT FK_SaleDetails_Products_ProductId FOREIGN KEY(ProductId) REFERENCES Products(Id));
CREATE INDEX IX_SaleDetails_SaleId ON SaleDetails(SaleId);
CREATE INDEX IX_SaleDetails_ProductId ON SaleDetails(ProductId);
GO
INSERT INTO Users(FullName, UserName, PasswordHash, Active) VALUES ('Administrador', 'admin', '0A5BC3E342432F1BAD92FFD51B785343EC72906CDBA6A26131060B008E786656', 1);
INSERT INTO Categories(Name) VALUES ('General');
INSERT INTO Products(Code, Name, Price, Stock, CategoryId, Active) VALUES ('P-001', 'Teclado', 25.00, 20, 1, 1), ('P-002', 'Mouse', 15.00, 30, 1, 1);
INSERT INTO __EFMigrationsHistory(MigrationId, ProductVersion) VALUES ('202609010001_InitialCreate', '8.0.8');
GO
