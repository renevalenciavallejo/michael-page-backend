-- ============================================================
--  michael_page_setup.sql
--  Base de datos: Azure SQL Database
--  Descripción : Creación de tablas, usuario y permisos CRUD
-- ============================================================

-- ============================================================
--  1. SCHEMA propio para aislar las tablas del proyecto
-- ============================================================
IF NOT EXISTS (
    SELECT 1 FROM sys.schemas WHERE name = 'michael_page'
)
BEGIN
    EXEC('CREATE SCHEMA michael_page');
END
GO


-- ============================================================
--  2. TABLA: michael_page.Users
-- ============================================================
IF OBJECT_ID('michael_page.Users', 'U') IS NULL
BEGIN
    CREATE TABLE michael_page.Users (
        Id          INT             NOT NULL IDENTITY(1,1),
        Name        NVARCHAR(100)   NOT NULL,
        Email       NVARCHAR(150)   NOT NULL,
        CreatedAt   DATETIME2(0)    NOT NULL CONSTRAINT DF_mpUsers_CreatedAt DEFAULT GETUTCDATE(),

        CONSTRAINT PK_mpUsers        PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT UQ_mpUsers_Email  UNIQUE (Email)
    );
END
GO


-- ============================================================
--  3. TABLA: michael_page.Tasks
-- ============================================================
IF OBJECT_ID('michael_page.Tasks', 'U') IS NULL
BEGIN
    CREATE TABLE michael_page.Tasks (
        Id              INT             NOT NULL IDENTITY(1,1),
        Title           NVARCHAR(200)   NOT NULL,
        Status          NVARCHAR(20)    NOT NULL
                            CONSTRAINT DF_mpTasks_Status  DEFAULT 'Pending'
                            CONSTRAINT CK_mpTasks_Status  CHECK (Status IN ('Pending', 'InProgress', 'Done')),
        UserId          INT             NOT NULL,
        CreatedAt       DATETIME2(0)    NOT NULL CONSTRAINT DF_mpTasks_CreatedAt DEFAULT GETUTCDATE(),

        -- Columna JSON para información adicional (prioridad, etiquetas, metadatos)
        AdditionalInfo  NVARCHAR(MAX)   NULL
                            CONSTRAINT CK_mpTasks_Json CHECK (
                                AdditionalInfo IS NULL OR ISJSON(AdditionalInfo) = 1
                            ),

        CONSTRAINT PK_mpTasks         PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_mpTasks_Users   FOREIGN KEY (UserId)
                                      REFERENCES michael_page.Users (Id)
                                      ON DELETE NO ACTION
                                      ON UPDATE NO ACTION
    );
END
GO


-- ============================================================
--  4. ÍNDICES
-- ============================================================

-- Acelera JOIN y filtros por usuario
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_mpTasks_UserId'
      AND object_id = OBJECT_ID('michael_page.Tasks')
)
    CREATE NONCLUSTERED INDEX IX_mpTasks_UserId
        ON michael_page.Tasks (UserId);
GO

-- Acelera filtros por estado
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_mpTasks_Status'
      AND object_id = OBJECT_ID('michael_page.Tasks')
)
    CREATE NONCLUSTERED INDEX IX_mpTasks_Status
        ON michael_page.Tasks (Status)
        INCLUDE (Id, Title, UserId, CreatedAt);
GO

-- Acelera ORDER BY fecha de creación
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_mpTasks_CreatedAt'
      AND object_id = OBJECT_ID('michael_page.Tasks')
)
    CREATE NONCLUSTERED INDEX IX_mpTasks_CreatedAt
        ON michael_page.Tasks (CreatedAt DESC);
GO


-- ============================================================
--  5. USUARIO DE BASE DE DATOS
--
--  En Azure SQL Database los logins se crean en la BD master.
--  Ejecuta el bloque de "LOGIN" conectado a master,
--  y el resto conectado a tu base de datos de aplicación.
-- ============================================================

-- >> Ejecutar en master <<
-- CREATE LOGIN michael_page_app
--     WITH PASSWORD = 'Tr0n@Secure#2026!';
-- GO

-- >> Ejecutar en la BD de la aplicación <<
IF NOT EXISTS (
    SELECT 1 FROM sys.database_principals WHERE name = 'michael_page_app'
)
BEGIN
    CREATE USER michael_page_app
        FOR LOGIN michael_page_app;   -- Vincula con el login creado en master
END
GO


-- ============================================================
--  6. PERMISOS: solo CRUD sobre las tablas del proyecto
--
--  Se conceden permisos explícitos tabla a tabla.
--  El usuario NO tiene acceso a ningún otro objeto de la BD.
-- ============================================================

-- Tabla Users
GRANT SELECT, INSERT, UPDATE, DELETE
    ON michael_page.Users
    TO michael_page_app;
GO

-- Tabla Tasks
GRANT SELECT, INSERT, UPDATE, DELETE
    ON michael_page.Tasks
    TO michael_page_app;
GO

-- Sin acceso al resto del schema dbo ni a otros schemas
-- (Azure SQL niega todo por defecto; no se necesita DENY explícito
--  salvo que el usuario sea miembro de un rol más permisivo)


-- ============================================================
--  7. CONSULTAS DE EJEMPLO — JSON nativo de SQL Server
-- ============================================================

-- Ejemplo de inserción con AdditionalInfo en JSON
/*
INSERT INTO michael_page.Tasks (Title, Status, UserId, AdditionalInfo)
VALUES (
    'Diseñar mockup de dashboard',
    'Pending',
    1,
    N'{
        "priority": "high",
        "estimatedEndDate": "2024-08-30",
        "tags": ["design", "ux"],
        "metadata": { "storyPoints": 5 }
    }'
);
*/

-- 7.1 Tareas por usuario, filtrando por estado, ordenadas por fecha
SELECT
    t.Id,
    t.Title,
    t.Status,
    t.CreatedAt,
    u.Name   AS AssignedTo,
    u.Email
FROM michael_page.Tasks  t
INNER JOIN michael_page.Users u ON u.Id = t.UserId
WHERE t.UserId = 1              -- parámetro: @UserId
  AND t.Status  = 'Pending'     -- parámetro: @Status
ORDER BY t.CreatedAt DESC;
GO

-- 7.2 Leer un campo del JSON con JSON_VALUE
SELECT
    Id,
    Title,
    JSON_VALUE(AdditionalInfo, '$.priority')            AS Priority,
    JSON_VALUE(AdditionalInfo, '$.estimatedEndDate')    AS EstimatedEnd
FROM michael_page.Tasks
WHERE AdditionalInfo IS NOT NULL;
GO

-- 7.3 Filtrar por valor dentro del JSON
SELECT
    Id,
    Title,
    Status
FROM michael_page.Tasks
WHERE JSON_VALUE(AdditionalInfo, '$.priority') = 'high';
GO

-- 7.4 Leer arreglo de etiquetas con JSON_QUERY
SELECT
    Id,
    Title,
    JSON_QUERY(AdditionalInfo, '$.tags') AS Tags
FROM michael_page.Tasks
WHERE AdditionalInfo IS NOT NULL;
GO

-- 7.5 Descomponer el JSON en filas con OPENJSON
SELECT
    t.Id,
    t.Title,
    tag.value AS Tag
FROM michael_page.Tasks t
CROSS APPLY OPENJSON(t.AdditionalInfo, '$.tags') AS tag
WHERE t.AdditionalInfo IS NOT NULL;
GO

-- 7.6 (Opcional) Actualizar un campo específico dentro del JSON
UPDATE michael_page.Tasks
SET AdditionalInfo = JSON_MODIFY(AdditionalInfo, '$.priority', 'medium')
WHERE Id = 1;
GO


-- ============================================================
--  8. VERIFICACIÓN
-- ============================================================
SELECT
    s.name  AS [Schema],
    t.name  AS [Table],
    p.permission_name,
    p.state_desc
FROM sys.database_permissions p
INNER JOIN sys.objects           t ON t.object_id  = p.major_id
INNER JOIN sys.schemas           s ON s.schema_id  = t.schema_id
INNER JOIN sys.database_principals dp ON dp.principal_id = p.grantee_principal_id
WHERE dp.name = 'michael_page_app'
ORDER BY t.name, p.permission_name;
GO
