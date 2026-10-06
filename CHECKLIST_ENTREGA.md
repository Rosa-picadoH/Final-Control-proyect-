# Checklist final para el sábado

## Antes de publicar

- [ ] Abrir `FinalControl.sln` en Visual Studio 2022.
- [ ] Restaurar paquetes NuGet y compilar en configuración Release.
- [ ] Confirmar que SQL Server LocalDB está disponible.
- [ ] Ejecutar la aplicación desde una base `FinalControlDb` limpia y comprobar que se aplica la migración.
- [ ] Iniciar sesión con `admin` / `Admin123*`.
- [ ] Crear, editar, buscar y desactivar un producto.
- [ ] Crear y eliminar una categoría sin productos; intentar eliminar una con productos para evidenciar la validación.
- [ ] Registrar una venta y confirmar que baja el stock.
- [ ] Generar el PDF y abrirlo.
- [ ] Probar el tipo de cambio con y sin internet.
- [ ] Ejecutar `./publish-release.ps1` y comprobar `Release/FinalControl/FinalControl.exe`.
- [ ] Confirmar que `Release/FinalControlDb.sql` fue copiado.
- [ ] Subir el repositorio real a GitHub y copiar su enlace.

## Orden de demostración

1. Login y panel.
2. Productos y categorías.
3. Código de `Data.cs`: entidades, DbSet y Fluent API.
4. Venta y reducción de stock.
5. Código de `SaleService.CreateAsync`: transacción.
6. API, PDF y log de errores.
7. GitHub, manual y carpeta Release.
