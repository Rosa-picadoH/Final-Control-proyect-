# Cambios de revisión

Fecha: 6 de octubre de 2026

## Seguridad

- Se añadió `FinalControl/PasswordHasher.cs` para guardar contraseñas nuevas mediante PBKDF2-SHA256 con sal aleatoria y comparación en tiempo constante.
- `AuthenticationService` todavía acepta el hash SHA-256 de las bases existentes. Tras una autenticación correcta, sustituye ese hash por uno PBKDF2 sin requerir cambio de esquema.
- El hash de la cuenta demo también se genera con una sal nueva.

## Inventario y ventas

- El descuento de stock ahora es una actualización condicional en SQL: solo se aplica cuando el producto sigue activo y tiene existencias suficientes.
- Las líneas repetidas de un producto se combinan antes de procesar la venta.
- La cabecera, sus detalles y los descuentos continúan protegidos por una única transacción; cualquier error revierte la operación completa.
- Las consultas de productos y categorías para mostrar datos usan `AsNoTracking`, y los servicios validan entradas y categorías existentes.

## Operación y mantenimiento

- El registro de errores se guarda bajo `%LOCALAPPDATA%\FinalControl\logs`, que es escribible cuando el ejecutable está en una carpeta protegida.
- Las pantallas muestran la ruta del registro ante errores técnicos y capturan fallos durante la carga inicial.
- El cliente del tipo de cambio vence después de ocho segundos. Los PDF incluyen milisegundos en su nombre para reducir colisiones.
- El usuario conectado aparece en el título de las ventanas de Productos y Categorías.
- Se actualizó el README, el manual técnico y la evidencia para describir el comportamiento corregido y las mejoras recomendadas para una siguiente versión.

## Interfaz y experiencia de uso

- Se aplicó una identidad visual común con navegación lateral oscura, superficies claras, jerarquía tipográfica y botones con estados al pasar el cursor.
- El login ahora tiene una composición de marca y formulario, y permite mostrar u ocultar la contraseña.
- El panel principal presenta tarjetas clicables, accesos rápidos, fecha/hora, tipo de cambio y actualización desde la pantalla.
- Productos, categorías y ventas recibieron títulos orientativos, tablas con filas alternadas y controles más claros. Categorías ahora tiene búsqueda instantánea; el stock bajo se destaca y el carrito informa artículos, unidades y total.
- Confirmar una venta con el carrito vacío presenta una indicación antes de llamar al servicio.
- Se ampliaron tamaños mínimos y se ajustaron las filas de controles para reducir recortes en ventanas más pequeñas.
- El rediseño se implementó con Windows Forms/MaterialSkin.2 ya presentes; no se añadió una dependencia ni se cambió el esquema de base de datos.

## Base de datos

No se cambió el esquema ni se añadió una migración. Las bases existentes y los scripts de instalación continúan siendo utilizables; los hashes SHA-256 antiguos se actualizan después de un inicio de sesión válido.

El ZIP no contiene la carpeta `Release` de la versión anterior: esos ejecutables no incluirían las correcciones de esta revisión. Se puede volver a generar con `publish-release.ps1` después de restaurar las dependencias NuGet.
