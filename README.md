# Michael Page API

API REST en ASP.NET Core para la gestión de **usuarios** y **tareas** asignadas a esos usuarios. Expone endpoints CRUD, valida las entradas, persiste en SQL Server / Azure SQL Database y registra logs estructurados con Serilog.

---

## 1. Arquitectura

La solución sigue una arquitectura en capas inspirada en **Clean Architecture**, donde las dependencias apuntan siempre hacia el dominio (`Core`). Esto permite aislar las reglas de negocio de los detalles de infraestructura (HTTP, base de datos, serialización, etc.).

```
┌────────────────────────────────────────────────────────┐
│                   MichaelPage.API                      │  ← Capa de presentación (HTTP)
│  Controllers · Filters · DI · Serilog · Swagger · CORS │
└───────────────────────────┬────────────────────────────┘
                            │ depende de
┌───────────────────────────▼────────────────────────────┐
│               MichaelPage.Application                  │  ← Casos de uso / orquestación
│    Services · DTOs · AutoMapper · FluentValidation     │
└───────────────────────────┬────────────────────────────┘
                            │ depende de
┌───────────────────────────▼────────────────────────────┐
│                  MichaelPage.Core                      │  ← Dominio
│      Entities · Enums · Interfaces de repositorio      │
└───────────────────────────▲────────────────────────────┘
                            │ implementa
┌───────────────────────────┴────────────────────────────┐
│               MichaelPage.Infrastructure               │  ← Persistencia
│    Repositorios (Dapper / SQL) · Script de BD (.sql)   │
└────────────────────────────────────────────────────────┘

┌────────────────────────────────────────────────────────┐
│                  MichaelPage.Common                    │  ← Transversal
│      Result<T> · Settings · Modelos compartidos        │
└────────────────────────────────────────────────────────┘
```

### Proyectos de la solución

| Proyecto | Responsabilidad |
| --- | --- |
| `MichaelPage.API` | Host web, controladores (`TasksController`, `UsersController`), pipeline HTTP, Swagger, CORS, Serilog, filtro global de excepciones y configuración de DI. |
| `MichaelPage.Application` | Casos de uso (`TaskService`, `UserService`), DTOs de entrada/salida, perfiles de AutoMapper y validadores de FluentValidation. |
| `MichaelPage.Core` | Dominio puro: entidades (`User`, `Task`, `TaskAdditionalInfo`), enum `TaskStatus` e interfaces de repositorio. |
| `MichaelPage.Infrastructure` | Implementación de repositorios y script SQL de creación de la base de datos. |
| `MichaelPage.Common` | Tipos compartidos: `Result` / `Result<T>` para el retorno unificado y `SqlServerSettings` para la cadena de conexión. |

### Decisiones técnicas relevantes

- **Patrón Result**: los servicios y controladores devuelven `Result` / `Result<T>` para normalizar respuestas de éxito y error sin lanzar excepciones para errores esperables.
- **AutoMapper** entre entidades y DTOs.
- **FluentValidation** aplicada automáticamente al model binding (`AddFluentValidationAutoValidation`).
- **Filtro global de excepciones** (`HttpGlobalExceptionFilter`) para capturar errores no controlados.
- **Serilog** con sinks a consola y a archivo (`./logs/info-*.txt`, `./logs/error-*.txt`) con rotación diaria.
- **CORS** preconfigurado para `http://localhost:4200` (frontend Angular).
- **Swagger** habilitado en entorno de desarrollo o cuando `IsSwaggerEnabled=true`.
- **JSON nativo de SQL Server** en la columna `AdditionalInfo` de `Tasks` para almacenar metadatos extensibles (prioridad, tags, etc.) sin romper el esquema.

---

## 2. Reglas de negocio

### Usuarios
- El **email es único**. Si ya existe, la creación falla con `"Email already in use."`.
- El `Id` es autogenerado (`IDENTITY`).
- `CreatedAt` se asigna por defecto con `GETUTCDATE()`.

### Tareas
- Toda tarea debe estar **asignada a un usuario existente**. Si el `UserId` no existe, la creación falla con `"Assigned user not found."`.
- Al crearse, la tarea se inicializa siempre con estado **`Pending`**, ignorando cualquier estado que envíe el cliente.
- El estado se restringe a tres valores (enum `TaskStatus` y `CHECK` en BD):
  - `Pending`
  - `InProgress`
  - `Done`
- **Transiciones de estado permitidas** (validado en `TaskService.UpdateTaskStatus`):
  - `Pending → InProgress` ✅
  - `InProgress → Done` ✅
  - `Pending → Done` ❌ (bloqueado: una tarea no puede completarse sin pasar por `InProgress`)
- `AdditionalInfo` es opcional; si se envía, se serializa como JSON y la BD valida que sea JSON válido (`ISJSON(AdditionalInfo) = 1`).
- El listado de tareas puede filtrarse por `status` mediante query string.
- Al devolver tareas, se hidrata el usuario asignado (`AssignedUser`) en el DTO.

### Validaciones de entrada
- `CreateUserDto` y `CreateTaskDto` se validan con FluentValidation (`CreateUserValidation`, `CreateTaskValidation`).
- Los errores de validación devuelven HTTP 400 envueltos en `Result.Fail(...)`.

---

## 3. Base de datos

### Ubicación del script

El script completo de creación de la base de datos se encuentra en:

```
MichaelPage.Infrastructure/Database/michael_page_setup.sql
```

Contiene:

1. Creación del **schema** `michael_page`.
2. Tabla `michael_page.Users` (con `UNIQUE` en `Email`).
3. Tabla `michael_page.Tasks` (con FK a `Users`, `CHECK` de estado y `CHECK` de JSON válido en `AdditionalInfo`).
4. Índices para acelerar filtros por `UserId`, `Status` y orden por `CreatedAt`.
5. Creación del **usuario de aplicación** `michael_page_app` y asignación de permisos CRUD **solo** sobre las tablas del proyecto.
6. Consultas de ejemplo con funciones JSON nativas (`JSON_VALUE`, `JSON_QUERY`, `OPENJSON`, `JSON_MODIFY`).
7. Consulta de verificación de permisos.

> En Azure SQL Database, el `LOGIN` debe crearse conectado a la BD `master`; el resto del script se ejecuta sobre la base de datos de la aplicación. El bloque `CREATE LOGIN` está comentado en el script y debe ejecutarse manualmente en `master`.

---

## 4. Configuración de la cadena de conexión

La cadena de conexión se enlaza a la clase `SqlServerSettings` desde `ConfigureSqlServerSettings` (`MichaelPage.API/Settings/ServicesConfigurationExtension.cs`).

Hay **dos formas** de configurarla:

### Opción A (recomendada): .NET User Secrets

Desde la carpeta del proyecto `MichaelPage.API`, inicialice y defina el secreto:

```bash
cd MichaelPage.API
dotnet user-secrets init
dotnet user-secrets set "SqlServerSettings:ConnectionString" "Data Source=server-name;Initial Catalog=database-name;persist security info=True;user id=michael_page_app;password=Tr0n@Secure#2026!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
```

El archivo `secrets.json` resultante debe respetar la siguiente estructura, que es la que `configuration.Bind(nameof(sqlServerSettings), sqlServerSettings)` espera:

```json
{
  "SqlServerSettings": {
    "ConnectionString": "Data Source=server-name;Initial Catalog=database-name;persist security info=True;user id=michael_page_app;password=Tr0n@Secure#2026!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  }
}
```

### Opción B: asignación en código (solo para pruebas locales)

En `MichaelPage.API/Settings/ServicesConfigurationExtension.cs`, dentro del método `ConfigureSqlServerSettings`, **comente** la línea:

```csharp
configuration.Bind(nameof(sqlServerSettings), sqlServerSettings);
```

y **descomente** la línea siguiente asignando la cadena directamente:

```csharp
sqlServerSettings.ConnectionString = "Data Source=server-name;Initial Catalog=database-name;persist security info=True;user id=michael_page_app;password=Tr0n@Secure#2026!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
```

> ⚠️ No comitear credenciales reales al repositorio. Use User Secrets o variables de entorno en entornos productivos.

---

## 5. Pasos para ejecutar la aplicación

### Requisitos previos
- .NET SDK 8.0+
- SQL Server local o acceso a una instancia de Azure SQL Database
- Herramienta para ejecutar SQL (SSMS, Azure Data Studio, `sqlcmd`, etc.)

### Paso a paso

1. **Clonar el repositorio** y ubicarse en la raíz (`michael-page/`).

2. **Crear la base de datos**:
   - Crear una BD vacía en SQL Server / Azure SQL.
   - (Solo Azure SQL) Conectado a `master`, ejecutar el bloque `CREATE LOGIN` comentado en el script.
   - Conectado a la BD de la aplicación, ejecutar `MichaelPage.Infrastructure/Database/michael_page_setup.sql`.

3. **Configurar la cadena de conexión** siguiendo la Opción A o B del apartado anterior.

4. **Restaurar dependencias y compilar**:
   ```bash
   dotnet restore
   dotnet build
   ```

5. **Ejecutar la API**:
   ```bash
   dotnet run --project MichaelPage.API
   ```

6. **Acceder a Swagger** (en desarrollo o con `IsSwaggerEnabled=true`):
   ```
   https://localhost:<puerto>/swagger
   ```

7. **Logs**: se generan en `MichaelPage.API/logs/` (`info-YYYYMMDD.txt`, `error-YYYYMMDD.txt`).

---

## 6. Endpoints principales

Base: `api/v1`

### Usuarios (`/users`)
| Método | Ruta | Descripción |
| --- | --- | --- |
| GET | `/users` | Listar usuarios |
| GET | `/users/{id}` | Obtener usuario por Id |
| POST | `/users` | Crear usuario |

### Tareas (`/tasks`)
| Método | Ruta | Descripción |
| --- | --- | --- |
| GET | `/tasks?status={Pending\|InProgress\|Done}` | Listar tareas (filtro opcional por estado) |
| GET | `/tasks/{id}` | Obtener tarea por Id (incluye usuario asignado) |
| POST | `/tasks` | Crear tarea (se fuerza estado `Pending`) |
| PATCH | `/tasks/{id}/status?status={...}` | Actualizar estado respetando las transiciones permitidas |
