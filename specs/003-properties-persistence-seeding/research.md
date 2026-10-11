# Investigación: persistencia y seeding de propiedades

## Decisión: conversión de estado como texto en base de datos

**Rationale:** El dominio requiere representar los estados `Available`, `Rented` y `Maintenance` de forma estable para consultas y para cumplir con los requisitos de persistencia. Guardarlos como texto con conversión EF Core evita errores de numeración y mantiene legibilidad en PostgreSQL.

**Alternativas consideradas:**
- Persistir el enum sin conversión: se descarta porque la base de datos no ofrece garantiza de lectura legible ni compatibilidad con cambios del enum.
- Guardar el valor como entero: se descarta por ser menos explícito y más frágil para semántica de dominio.

## Decisión: arranque de la aplicación con `app.MigrateAsync()`

**Rationale:** La especificación exige que el seeding se dispare en cada arranque, aun si no hay migraciones pendientes. Llamar siempre a `MigrateAsync()` garantiza que `UseAsyncSeeding()` se ejecute y que la base permanezca consistente.

**Alternativas consideradas:**
- Condicionar la migración a `GetPendingMigrationsAsync()`: se descarta porque podría omitir el seeding cuando la base ya está al día.
- Ejecutar el seed manualmente desde `Program.cs`: se descarta porque duplica la lógica y rompe el flujo centralizado de EF Core.

## Decisión: definición del seeder a partir de un manifiesto de assets

**Rationale:** Los datos y los archivos de soporte deben resolverse desde un solo origen y apuntar a rutas públicas servidas por la aplicación. El manifiesto permite centralizar la relación entre archivos externos, rutas de publicación y el valor `ImageUrl` persistido.

**Alternativas consideradas:**
- Hardcodear rutas locales del repositorio: se descarta porque rompe la portabilidad y la publicación en runtime.
- Serializar los assets directamente desde código: se descarta porque duplica el origen de verdad y complicaría futuras extensiones.

## Decisión: entidad y configuración estructurada por dominio y persistencia

**Rationale:** La base de dato y el dominio deben organizarse por feature y mantenerla separada de los endpoints. El modelo concuerda con la arquitectura de vertical slice y con el uso de `IEntityTypeConfiguration<T>` para el mapping.

**Alternativas consideradas:**
- Configurar entidades inline en `OnModelCreating()`: se descarta porque contradice la política del proyecto.
- Añadir capas de repositorios o servicios globales antes del dominio: se descarta por prematuro y fuera del alcance.

## Evidencia del repositorio

- El proyecto base ya usa ASP.NET Core Minimal APIs y .NET 11 en la raíz del repositorio.
- Hay un conjunto de seed-data previamente preparado en `support/seed-data/` con JSON y activos de imagen.
- La solución requiere una migración única, `UseSeeding()`/`UseAsyncSeeding()`, un `DatabaseSeeder` idempotente y un punto de publicación para assets de imagen.
