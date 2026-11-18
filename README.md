# Franchise Manager

Language / Idioma: [English](#english) · [Español](#español)

---

<a id="english"></a>

# 🇬🇧 English

Fullstack application to manage **franchises**, their **branches** and the **products** of each branch, including stock updates.

- **Backend:** ASP.NET Core Web API (.NET 8) + Entity Framework Core + SQL Server
- **Frontend:** React + Vite + React Router

## Features

- Create and list franchises
- Create and list branches of a franchise
- Create and list products of a branch
- Update the stock of a product
- Stock report per franchise: totals per branch and the product with the most stock
- Input validation on both API and UI, with clear error messages (no stack traces)
- Loading and error states in the UI
- Unit tests for controllers and validation rules (xUnit)
- One-command setup with Docker Compose (SQL Server + API + frontend)

## Prerequisites

| Tool | Version | Notes |
|---|---|---|
| [.NET SDK](https://dotnet.microsoft.com/download/dotnet/8.0) | 8.0.x | Pinned in `backend/global.json` |
| [Node.js](https://nodejs.org/) | 20.19+ or 22.12+ | Required by Vite 8 |
| SQL Server LocalDB | Any recent version | Included with Visual Studio or SQL Server Express (Windows) |
| EF Core CLI | 8.0.x | `dotnet tool install --global dotnet-ef --version 8.0.*` |

> Not on Windows? Any SQL Server instance works — just update the connection string in `backend/Franchise.Api/appsettings.json`, or use [Docker](#run-with-docker-alternative).

## Getting started

### 1. Clone the repository

```bash
git clone https://github.com/ItzCavi/franchise-app.git
cd franchise-app
```

### 2. Run the backend

```bash
cd backend/Franchise.Api
dotnet restore
dotnet ef database update
dotnet run
```

- API: http://localhost:5104
- Swagger UI: http://localhost:5104/swagger

`dotnet ef database update` creates the `FranchiseDb` database in LocalDB and applies all migrations.

### 3. Run the frontend

In a second terminal:

```bash
cd frontend
npm install
npm run dev
```

- App: http://localhost:5173

The API URL is configured in `frontend/.env.development` (`VITE_API_URL`). Change it if your API runs on a different port.

## Run with Docker (alternative)

Runs SQL Server, the API and the frontend with a single command. Only [Docker Desktop](https://www.docker.com/products/docker-desktop/) is required — no .NET, Node.js or SQL Server installation needed.

```bash
docker compose up --build
```

- App: http://localhost:5173
- Swagger UI: http://localhost:5104/swagger

The first run takes a few minutes while images are downloaded. The API waits until SQL Server is healthy and applies the EF Core migrations automatically on startup.

| Service | Image | Port | Description |
|---|---|---|---|
| `db` | `mcr.microsoft.com/mssql/server:2022-latest` | internal | SQL Server, data persisted in the `mssql-data` volume |
| `api` | built from `backend/Dockerfile` | `5104` | ASP.NET Core API (multi-stage build: SDK → runtime) |
| `web` | built from `frontend/Dockerfile` | `5173` | React app built with Vite and served by nginx |

To stop: `Ctrl + C`, then `docker compose down` (add `-v` to also delete the database data).

> Stop any local `dotnet run` / `npm run dev` first — they use the same ports. The Docker database is separate from LocalDB and starts empty.

## Database migrations (EF Core)

Run these commands from `backend/Franchise.Api`.

| Action | Command |
|---|---|
| Apply migrations / create database | `dotnet ef database update` |
| Create a new migration after changing a model | `dotnet ef migrations add <MigrationName>` |
| Remove the last migration (if not applied) | `dotnet ef migrations remove` |

Connection string (`appsettings.json`):

```
Server=(localdb)\MSSQLLocalDB;Database=FranchiseDb;Trusted_Connection=True;TrustServerCertificate=True
```

## Running tests

```bash
cd backend
dotnet test
```

Unit tests (xUnit) cover controller logic — success cases, `404` and `409` responses — and DTO validation rules. They use the EF Core InMemory provider, so each test gets its own isolated database.

| Test class | What it covers |
|---|---|
| `FranchisesControllerTests` | Create (trims name), duplicate name → `409`, list ordered by name, not found → `404` |
| `BranchesControllerTests` | Missing franchise → `404`, same name allowed in different franchises, duplicate in same franchise → `409`, filter by franchise |
| `ProductsControllerTests` | Create, missing branch → `404`, stock update, missing product → `404` |
| `ReportsControllerTests` | Totals per branch and franchise, top product per branch, branch without products, missing franchise → `404` |
| `DtoValidationTests` | Required/empty names, max length, negative stock, missing stock, zero stock is valid |

> Limitation: the InMemory provider does not enforce database constraints (unique indexes, check constraints). Those rules are covered by the API-level checks tested here and by the real SQL Server schema.

## API endpoints

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/franchises` | List franchises |
| GET | `/api/franchises/{id}` | Get a franchise |
| POST | `/api/franchises` | Create a franchise |
| GET | `/api/franchises/{franchiseId}/branches` | List branches of a franchise |
| POST | `/api/franchises/{franchiseId}/branches` | Create a branch |
| GET | `/api/branches/{id}` | Get a branch |
| GET | `/api/branches/{branchId}/products` | List products of a branch |
| POST | `/api/branches/{branchId}/products` | Create a product |
| GET | `/api/products/{id}` | Get a product |
| PATCH | `/api/products/{id}/stock` | Update product stock |
| GET | `/api/franchises/{franchiseId}/report` | Stock report of a franchise (totals per branch, top product) |

### Response codes

| Code | Meaning |
|---|---|
| `200` | OK |
| `201` | Created (includes `Location` header) |
| `400` | Validation error |
| `404` | Resource not found |
| `409` | Duplicate name |
| `500` | Unexpected error (generic message, no stack trace) |

All errors use the standard [ProblemDetails](https://datatracker.ietf.org/doc/html/rfc9457) JSON format.

## Architecture

```
franchise-app/
├── docker-compose.yml              # SQL Server + API + frontend
├── backend/
│   ├── Dockerfile                  # Multi-stage build of the API
│   ├── global.json                 # Pins .NET SDK 8
│   ├── Franchise.sln               # Solution (API + tests)
│   ├── Franchise.Api.Tests/        # xUnit tests (controllers, DTO validation)
│   └── Franchise.Api/
│       ├── Controllers/            # HTTP endpoints (one per resource)
│       ├── Data/AppDbContext.cs    # EF Core context, relationships and constraints
│       ├── Dtos/                   # Request/response contracts with validation
│       ├── Migrations/             # EF Core migrations
│       ├── Models/                 # Domain entities
│       └── Program.cs              # DI, CORS, Swagger, error handling
└── frontend/
    ├── Dockerfile                  # Vite build served by nginx
    ├── nginx.conf                  # SPA fallback for React Router
    └── src/
        ├── api/                    # HTTP client and calls per resource
        ├── components/             # Reusable UI components
        ├── pages/                  # One component per screen
        └── App.jsx                 # Routes
```

### Backend

- **Controllers + EF Core:** controllers use `AppDbContext` directly through dependency injection. For an application of this size, an extra repository/service layer would add code without adding value; EF Core's `DbContext` already acts as a unit of work. If business rules grew, the next step would be moving logic into a service layer.
- **DTOs** separate the API contract from the database entities: they avoid circular references in JSON and prevent clients from setting fields like `Id`.
- **Validation in two layers:**
  - API: Data Annotations (`[Required]`, `[StringLength]`, `[Range]`) validated automatically by `[ApiController]`.
  - Database: unique indexes (no duplicate names per parent), a check constraint (`Stock >= 0`) and cascade delete.
- **Error handling:** `AddProblemDetails` + `UseExceptionHandler` return consistent JSON errors without exposing stack traces.
- **RESTful nested routes** express ownership (`/franchises/{id}/branches`). `PATCH` is used for stock because only one field changes.
- **CORS** only allows the frontend origin (`http://localhost:5173`).
- **Migrations on startup** are applied only when `ApplyMigrationsOnStartup=true` (enabled in Docker). `EnableRetryOnFailure` makes the connection resilient while SQL Server starts.
- **Stock report** loads the franchise with its branches and products in a single query (`Include`/`ThenInclude`, `AsNoTracking`) and aggregates in memory. For large data volumes, the aggregation would be moved to SQL with a projection.

### Frontend

- **`api/client.js`** centralizes `fetch`, reads ProblemDetails errors (including validation errors) and handles connection failures.
- **Pages** manage loading, error and empty states; forms validate input before calling the API and disable buttons while saving.
- **React Router** handles navigation: Franchises → Branches → Products, plus a stock report page per franchise.

### Data model

```
Franchise (Id, Name)
   └── 1:N ── Branch (Id, FranchiseId, Name)
                 └── 1:N ── Product (Id, BranchId, Name, Stock)
```

## Git workflow

- `main` only receives changes through Pull Requests.
- Each module was developed in its own branch (`feature/backend-setup`, `feature/franchise-module`, `feature/branch-module`, `feature/product-module`, `feature/frontend-setup`, `feature/branches-products-pages`, `docs/readme`, `feature/unit-tests`, `feature/stock-report`, `feature/docker`).
- Commits follow [Conventional Commits](https://www.conventionalcommits.org/) (`feat:`, `fix:`, `chore:`, `docs:`, `test:`, `build:`).

## Notes

- **.NET 8** reached end of support on November 10, 2026. It is used here because the technical test requires it; upgrading to a supported version would mainly require changing `TargetFramework` and package versions.
- `npm audit` reports a vulnerability in `picomatch`, a transitive dependency of Vite used only by the dev tooling. The suggested `npm audit fix --force` downgrades Vite to an older major version, so it was not applied.
- The SQL Server password in `docker-compose.yml` is for **local development only**. In a real deployment it would come from environment variables or a secrets manager.

---

<a id="español"></a>

# 🇪🇸 Español

Aplicación fullstack para gestionar **franquicias**, sus **sucursales** y los **productos** de cada sucursal, incluyendo la actualización de stock.

- **Backend:** ASP.NET Core Web API (.NET 8) + Entity Framework Core + SQL Server
- **Frontend:** React + Vite + React Router

## Funcionalidades

- Crear y listar franquicias
- Crear y listar sucursales de una franquicia
- Crear y listar productos de una sucursal
- Actualizar el stock de un producto
- Reporte de stock por franquicia: totales por sucursal y el producto con más stock
- Validación de datos en la API y en la interfaz, con mensajes de error claros (sin stack traces)
- Estados de carga y de error en la interfaz
- Pruebas unitarias de los controladores y de las reglas de validación (xUnit)
- Ejecución con un solo comando mediante Docker Compose (SQL Server + API + frontend)

## Requisitos previos

| Herramienta | Versión | Notas |
|---|---|---|
| [.NET SDK](https://dotnet.microsoft.com/download/dotnet/8.0) | 8.0.x | Fijada en `backend/global.json` |
| [Node.js](https://nodejs.org/) | 20.19+ o 22.12+ | Requerido por Vite 8 |
| SQL Server LocalDB | Cualquier versión reciente | Incluido con Visual Studio o SQL Server Express (Windows) |
| EF Core CLI | 8.0.x | `dotnet tool install --global dotnet-ef --version 8.0.*` |

> ¿No usas Windows? Funciona con cualquier instancia de SQL Server; solo actualiza la cadena de conexión en `backend/Franchise.Api/appsettings.json`, o usa [Docker](#ejecutar-con-docker-alternativa).

## Cómo ejecutar el proyecto

### 1. Clonar el repositorio

```bash
git clone https://github.com/ItzCavi/franchise-app.git
cd franchise-app
```

### 2. Ejecutar el backend

```bash
cd backend/Franchise.Api
dotnet restore
dotnet ef database update
dotnet run
```

- API: http://localhost:5104
- Swagger UI: http://localhost:5104/swagger

`dotnet ef database update` crea la base de datos `FranchiseDb` en LocalDB y aplica todas las migraciones.

### 3. Ejecutar el frontend

En una segunda terminal:

```bash
cd frontend
npm install
npm run dev
```

- Aplicación: http://localhost:5173

La URL de la API se configura en `frontend/.env.development` (`VITE_API_URL`). Cámbiala si tu API corre en otro puerto.

## Ejecutar con Docker (alternativa)

Levanta SQL Server, la API y el frontend con un solo comando. Solo se necesita [Docker Desktop](https://www.docker.com/products/docker-desktop/); no hace falta instalar .NET, Node.js ni SQL Server.

```bash
docker compose up --build
```

- Aplicación: http://localhost:5173
- Swagger UI: http://localhost:5104/swagger

La primera ejecución tarda unos minutos mientras se descargan las imágenes. La API espera a que SQL Server esté listo (healthcheck) y aplica las migraciones de EF Core automáticamente al arrancar.

| Servicio | Imagen | Puerto | Descripción |
|---|---|---|---|
| `db` | `mcr.microsoft.com/mssql/server:2022-latest` | interno | SQL Server, datos persistidos en el volumen `mssql-data` |
| `api` | construida desde `backend/Dockerfile` | `5104` | API ASP.NET Core (build multi-etapa: SDK → runtime) |
| `web` | construida desde `frontend/Dockerfile` | `5173` | App React compilada con Vite y servida con nginx |

Para detener: `Ctrl + C` y luego `docker compose down` (añade `-v` para borrar también los datos de la base de datos).

> Detén antes cualquier `dotnet run` / `npm run dev` local, porque usan los mismos puertos. La base de datos de Docker es independiente de LocalDB y empieza vacía.

## Migraciones de base de datos (EF Core)

Ejecuta estos comandos desde `backend/Franchise.Api`.

| Acción | Comando |
|---|---|
| Aplicar migraciones / crear la base de datos | `dotnet ef database update` |
| Crear una migración nueva tras cambiar un modelo | `dotnet ef migrations add <NombreMigracion>` |
| Eliminar la última migración (si no se aplicó) | `dotnet ef migrations remove` |

Cadena de conexión (`appsettings.json`):

```
Server=(localdb)\MSSQLLocalDB;Database=FranchiseDb;Trusted_Connection=True;TrustServerCertificate=True
```

## Ejecutar las pruebas

```bash
cd backend
dotnet test
```

Las pruebas unitarias (xUnit) cubren la lógica de los controladores (casos exitosos y respuestas `404` y `409`) y las reglas de validación de los DTOs. Usan el proveedor InMemory de EF Core, así que cada prueba tiene su propia base de datos aislada.

| Clase de prueba | Qué cubre |
|---|---|
| `FranchisesControllerTests` | Crear (recorta espacios del nombre), nombre duplicado → `409`, lista ordenada por nombre, no encontrada → `404` |
| `BranchesControllerTests` | Franquicia inexistente → `404`, mismo nombre permitido en franquicias distintas, duplicado en la misma franquicia → `409`, filtro por franquicia |
| `ProductsControllerTests` | Crear, sucursal inexistente → `404`, actualizar stock, producto inexistente → `404` |
| `ReportsControllerTests` | Totales por sucursal y por franquicia, producto con más stock por sucursal, sucursal sin productos, franquicia inexistente → `404` |
| `DtoValidationTests` | Nombres obligatorios/vacíos, longitud máxima, stock negativo, stock ausente, stock 0 válido |

> Limitación: el proveedor InMemory no aplica las restricciones de la base de datos (índices únicos, restricciones `CHECK`). Esas reglas están cubiertas por las validaciones de la API que se prueban aquí y por el esquema real de SQL Server.

## Endpoints de la API

| Método | Endpoint | Descripción |
|---|---|---|
| GET | `/api/franchises` | Listar franquicias |
| GET | `/api/franchises/{id}` | Obtener una franquicia |
| POST | `/api/franchises` | Crear una franquicia |
| GET | `/api/franchises/{franchiseId}/branches` | Listar sucursales de una franquicia |
| POST | `/api/franchises/{franchiseId}/branches` | Crear una sucursal |
| GET | `/api/branches/{id}` | Obtener una sucursal |
| GET | `/api/branches/{branchId}/products` | Listar productos de una sucursal |
| POST | `/api/branches/{branchId}/products` | Crear un producto |
| GET | `/api/products/{id}` | Obtener un producto |
| PATCH | `/api/products/{id}/stock` | Actualizar el stock de un producto |
| GET | `/api/franchises/{franchiseId}/report` | Reporte de stock de una franquicia (totales por sucursal, producto con más stock) |

### Códigos de respuesta

| Código | Significado |
|---|---|
| `200` | OK |
| `201` | Creado (incluye el header `Location`) |
| `400` | Error de validación |
| `404` | Recurso no encontrado |
| `409` | Nombre duplicado |
| `500` | Error inesperado (mensaje genérico, sin stack trace) |

Todos los errores usan el formato JSON estándar [ProblemDetails](https://datatracker.ietf.org/doc/html/rfc9457).

## Arquitectura

```
franchise-app/
├── docker-compose.yml              # SQL Server + API + frontend
├── backend/
│   ├── Dockerfile                  # Build multi-etapa de la API
│   ├── global.json                 # Fija el SDK de .NET 8
│   ├── Franchise.sln               # Solución (API + pruebas)
│   ├── Franchise.Api.Tests/        # Pruebas xUnit (controladores, validación de DTOs)
│   └── Franchise.Api/
│       ├── Controllers/            # Endpoints HTTP (uno por recurso)
│       ├── Data/AppDbContext.cs    # Contexto de EF Core, relaciones y restricciones
│       ├── Dtos/                   # Contratos de entrada/salida con validación
│       ├── Migrations/             # Migraciones de EF Core
│       ├── Models/                 # Entidades del dominio
│       └── Program.cs              # Inyección de dependencias, CORS, Swagger, manejo de errores
└── frontend/
    ├── Dockerfile                  # Build de Vite servido con nginx
    ├── nginx.conf                  # Redirección a index.html para React Router
    └── src/
        ├── api/                    # Cliente HTTP y llamadas por recurso
        ├── components/             # Componentes reutilizables
        ├── pages/                  # Un componente por pantalla
        └── App.jsx                 # Rutas
```

### Backend

- **Controllers + EF Core:** los controladores usan `AppDbContext` directamente mediante inyección de dependencias. En una aplicación de este tamaño, una capa extra de repositorios o servicios añadiría código sin aportar valor; el `DbContext` de EF Core ya actúa como unidad de trabajo. Si las reglas de negocio crecieran, el siguiente paso sería mover la lógica a una capa de servicios.
- **Los DTOs** separan el contrato de la API de las entidades de la base de datos: evitan referencias circulares al serializar a JSON e impiden que el cliente asigne campos como `Id`.
- **Validación en dos capas:**
  - API: Data Annotations (`[Required]`, `[StringLength]`, `[Range]`), validadas automáticamente por `[ApiController]`.
  - Base de datos: índices únicos (sin nombres duplicados dentro del mismo padre), una restricción `CHECK` (`Stock >= 0`) y borrado en cascada.
- **Manejo de errores:** `AddProblemDetails` + `UseExceptionHandler` devuelven errores JSON consistentes sin exponer stack traces.
- **Rutas REST anidadas** que expresan pertenencia (`/franchises/{id}/branches`). Se usa `PATCH` para el stock porque solo cambia un campo.
- **CORS** solo permite el origen del frontend (`http://localhost:5173`).
- **Las migraciones al arrancar** solo se aplican si `ApplyMigrationsOnStartup=true` (activado en Docker). `EnableRetryOnFailure` hace la conexión resiliente mientras SQL Server arranca.
- **El reporte de stock** carga la franquicia con sus sucursales y productos en una sola consulta (`Include`/`ThenInclude`, `AsNoTracking`) y calcula los totales en memoria. Con grandes volúmenes de datos, la agregación se trasladaría a SQL mediante una proyección.

### Frontend

- **`api/client.js`** centraliza `fetch`, interpreta los errores ProblemDetails (incluidos los de validación) y maneja los fallos de conexión.
- **Las páginas** manejan los estados de carga, error y lista vacía; los formularios validan los datos antes de llamar a la API y desactivan los botones mientras guardan.
- **React Router** gestiona la navegación: Franquicias → Sucursales → Productos, además de una pantalla de reporte de stock por franquicia.

### Modelo de datos

```
Franchise (Id, Name)
   └── 1:N ── Branch (Id, FranchiseId, Name)
                 └── 1:N ── Product (Id, BranchId, Name, Stock)
```

## Flujo de trabajo con Git

- `main` solo recibe cambios mediante Pull Requests.
- Cada módulo se desarrolló en su propia rama (`feature/backend-setup`, `feature/franchise-module`, `feature/branch-module`, `feature/product-module`, `feature/frontend-setup`, `feature/branches-products-pages`, `docs/readme`, `feature/unit-tests`, `feature/stock-report`, `feature/docker`).
- Los commits siguen [Conventional Commits](https://www.conventionalcommits.org/) (`feat:`, `fix:`, `chore:`, `docs:`, `test:`, `build:`).

## Notas

- **.NET 8** dejó de tener soporte oficial el 10 de noviembre de 2026. Se usa porque la prueba técnica lo exige; actualizar a una versión con soporte requeriría principalmente cambiar el `TargetFramework` y las versiones de los paquetes.
- `npm audit` reporta una vulnerabilidad en `picomatch`, una dependencia indirecta de Vite que solo usan las herramientas de desarrollo. El comando sugerido `npm audit fix --force` degrada Vite a una versión mayor anterior, por lo que no se aplicó.
- La contraseña de SQL Server en `docker-compose.yml` es **solo para desarrollo local**. En un despliegue real vendría de variables de entorno o de un gestor de secretos.
