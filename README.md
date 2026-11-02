# NorthwindERP — Backend (ASP.NET Core 8 + Supabase)

API REST del ERP de inventario Northwind (empresa de importación/exportación de alimentos gourmet).
Arquitectura en 3 capas, base de datos PostgreSQL en Supabase, autenticación JWT.

## Arquitectura

```
Petición HTTP
   │
   ▼
NorthwindERP.API            Controllers → reciben el request, validan DTOs, devuelven JSON
   │                        Middlewares → convierten excepciones en respuestas 400/404/409/500
   ▼
NorthwindERP.Core           Services   → reglas de negocio (no saben nada de SQL)
   │                        Interfaces → contratos que la capa de datos debe cumplir
   ▼
NorthwindERP.Infrastructure Repositories → EF Core (tablas) + Dapper (RPC y vistas)
   │
   ▼
Supabase (PostgreSQL)       tablas, vistas, funciones RPC, triggers
```

| Capa | Depende de | Por qué |
|---|---|---|
| Core | nada | La lógica de negocio no cambia si cambias de base de datos |
| Infrastructure | Core | Implementa las interfaces de Core |
| API | Core + Infrastructure | Arma todo (inyección de dependencias) |

**¿Por qué EF Core y Dapper juntos?** EF Core es ideal para tablas (SELECT/UPDATE/DELETE con LINQ).
Las funciones RPC y vistas devuelven columnas definidas en la BD, así que se leen con Dapper
(`SELECT * FROM funcion(...)`) usando **la misma conexión y transacción** de EF Core.

## Requisitos

- [.NET SDK 8](https://dotnet.microsoft.com/download) (`dotnet --version` debe mostrar 8.x o superior)
- Visual Studio 2022 (o VS Code + extensión C# Dev Kit)
- Proyecto de Supabase con las 16 tablas, 2 vistas y las funciones RPC

> Si solo tienes .NET 9/10 instalado, cambia `<TargetFramework>net8.0</TargetFramework>` por `net9.0`/`net10.0`
> en los 4 `.csproj` y sube los paquetes `Microsoft.*`/`Npgsql.*`/`EFCore.NamingConventions` a la misma versión mayor.

## Puesta en marcha

### 1. Restaurar paquetes y compilar
```bash
cd NorthwindERP.Backend
dotnet restore
dotnet build
```

### 2. Cadena de conexión de Supabase
En Supabase: **Connect → Session pooler** (compatible con IPv4). Copia host, usuario y región.

No pongas la contraseña real en `appsettings.json` (se sube a GitHub). Usa *user-secrets*:
```bash
cd src/NorthwindERP.API
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:Supabase" "Host=aws-0-us-east-1.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.abcdefghijk;Password=TU_PASSWORD_REAL;SSL Mode=Require;Include Error Detail=true"
dotnet user-secrets set "Jwt:Key" "una-clave-larga-aleatoria-de-al-menos-32-caracteres!!"
```

### 3. Crear un usuario de prueba con contraseña BCrypt
En el **SQL Editor** de Supabase (pgcrypto ya viene instalado en el esquema `extensions`):
```sql
-- Ver roles existentes
SELECT * FROM roles;

-- Crear (o actualizar) un usuario "admin" con contraseña Admin123*
INSERT INTO useraccounts (employeeid, username, passwordhash, roleid, isactive, mustchangepassword, createdat)
VALUES (1, 'admin', extensions.crypt('Admin123*', extensions.gen_salt('bf')), 1, true, false, now());
```
> `employeeid` es importante: los pedidos y compras se registran a nombre del empleado del usuario logueado.

### 4. Ejecutar
```bash
dotnet run --project src/NorthwindERP.API
```
Abre **http://localhost:5080/swagger**
1. `GET /api/health` → debe decir `"database": "conectada"`.
2. `POST /api/auth/login` → copia el `token`.
3. Botón **Authorize** → pega el token → ya puedes probar todo.

También puedes usar el archivo `src/NorthwindERP.API/NorthwindERP.API.http` (botón "Send Request" en Visual Studio).

### 5. Pruebas unitarias
```bash
dotnet test
```

## Endpoints

| Módulo | Método | Ruta | Implementación |
|---|---|---|---|
| 🔐 Auth | POST | `/api/auth/login` | `useraccounts` + BCrypt → JWT |
| 🔐 Auth | GET | `/api/auth/me` | Datos del token |
| 📦 Inventario | GET | `/api/products?search=&categoryId=` | EF Core (JOIN categories, suppliers) |
| 📦 Inventario | GET | `/api/categories` | EF Core |
| 📦 Inventario | GET | `/api/inventory/stock?warehouseId=&productId=&lowStockOnly=` | `inventorystock` |
| 📦 Inventario | GET | `/api/inventory/kardex?productId=&warehouseId=&type=&from=&to=&take=` | `inventorytransactions` |
| 📦 Inventario | GET | `/api/inventory/warehouses` | `warehouses` |
| 🛒 Órdenes | POST | `/api/orders/with-customer` | INSERT customer + RPC `register_customer_order` (1 transacción) |
| 🛒 Órdenes | GET | `/api/orders/customer/{customerId}` | RPC `get_customer_order_history` |
| 🛒 Órdenes | GET | `/api/orders` | Vista `vw_order_details_extended` |
| 🛒 Órdenes | DELETE | `/api/orders/{id}` | DELETE + ON DELETE CASCADE |
| 👥 Personal | GET | `/api/customers` · `/api/customers/{id}` | EF Core |
| 👥 Personal | PUT | `/api/customers/{id}` | UPDATE dirección/ciudad/CP/país |
| 👥 Personal | GET | `/api/employees` · `/api/shippers` · `/api/suppliers` | EF Core |
| 📥 Compras | GET | `/api/purchase-orders` · `/api/purchase-orders/{id}` | EF Core |
| 📥 Compras | POST | `/api/purchase-orders` | INSERT cabecera + detalles (estado `Pending`) |
| 📥 Compras | PUT | `/api/purchase-orders/{id}/receive` | RPC `receive_purchase_order` |
| 📊 Dashboard | GET | `/api/dashboard/kpis` | LINQ (SUM/COUNT) + vista `vw_customer_sales_summary` |
| 📊 Dashboard | GET | `/api/dashboard/sales-vs-investment` | GROUP BY año/mes |
| — | GET | `/api/health` | Prueba de conexión (sin token) |

Todos excepto `login` y `health` requieren `Authorization: Bearer <token>`.

### Ejemplo: crear pedido con cliente nuevo
```json
POST /api/orders/with-customer
{
  "newCustomer": { "customerName": "Bodega San Martín", "city": "Cusco", "country": "Peru" },
  "shipperId": 1,
  "items": [ { "productId": 1, "quantity": 5 }, { "productId": 2, "quantity": 3 } ]
}
```
Para un cliente existente envía `"customerId": 1` en lugar de `newCustomer`.

### Formato de errores
Todas las respuestas de error usan el mismo formato (ProblemDetails):
```json
{ "status": 400, "title": "Operación rechazada por la base de datos", "detail": "Stock insuficiente para el producto 5" }
```
Un `RAISE EXCEPTION '...'` dentro de tus funciones RPC llega al frontend como **400** con tu mensaje.

## ⚠️ Puntos que debes verificar contra tu BD

| Qué | Dónde se cambia |
|---|---|
| Claves del JSON `p_details` (`product_id`, `quantity`) | `Infrastructure/Repositories/OrderRepository.cs` |
| Nombres de tablas (todo en minúsculas, sin `_`) | `Infrastructure/Data/NorthwindDbContext.cs` |
| Valores de `purchaseorders.status` (`Pending` / `Received`) | `Core/Entities/Purchasing.cs` |
| `customers` no tiene columna `phone` | `Core/Entities/Sales.cs` (instrucciones en el comentario) |

Para ver la definición real de tus funciones y vistas:
```sql
SELECT pg_get_functiondef('register_customer_order'::regproc);
SELECT pg_get_functiondef('get_customer_order_history'::regproc);
SELECT pg_get_functiondef('receive_purchase_order'::regproc);
SELECT pg_get_viewdef('vw_order_details_extended', true);
SELECT pg_get_viewdef('vw_customer_sales_summary', true);
```

Si al crear un cliente o una orden de compra sale *"La columna 'customerid' no acepta valores nulos"*,
la PK no es autoincremental. Arréglalo una vez:
```sql
ALTER TABLE customers ALTER COLUMN customerid ADD GENERATED BY DEFAULT AS IDENTITY;
SELECT setval(pg_get_serial_sequence('customers','customerid'), (SELECT MAX(customerid) FROM customers));
-- igual para purchaseorders.purchaseorderid
```

## Agregar un endpoint nuevo (receta)

1. **Entidad** en `Core/Entities` (si es una tabla nueva) + `DbSet` en `NorthwindDbContext`.
2. **Método en la interfaz** del repositorio (`Core/Interfaces/IRepositories.cs`) y su implementación en `Infrastructure/Repositories`.
3. **Método en el servicio** (`Core/Services`) con las validaciones de negocio.
4. **DTO** en `API/DTOs` + mapeo en `Mappings.cs`.
5. **Acción** en el controlador con `[HttpGet]`, `[HttpPost]`, etc.
6. Si es un servicio/repositorio nuevo, regístralo en `ServiceCollectionExtensions.cs` o `DependencyInjection.cs`.

## Próximos pasos sugeridos
- CRUD de proveedores y ajustes manuales de inventario (mermas, transferencias) del tablero.
- `[Authorize(Roles = "Admin")]` en endpoints sensibles como `DELETE /api/orders/{id}`.
- Deploy: Render / Railway / Azure App Service, con las variables `ConnectionStrings__Supabase` y `Jwt__Key`.
