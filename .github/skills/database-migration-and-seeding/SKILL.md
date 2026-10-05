# Skill: Migración y Seed Automático de Base de Datos

## Cuándo usar este skill

Usar este skill cuando la aplicación deba aplicar migraciones pendientes y ejecutar seeds idempotentes al iniciar, usando el mecanismo de seeding nativo de EF Core: `UseSeeding()` y `UseAsyncSeeding()`.

Este skill NO genera archivos de migración. Los archivos de migración se generan mediante:

```text
.skills/ef-core-migrations/SKILL.md
```

## Regla principal

La spec genera entidades, configuraciones y archivos de migración.

La aplicación aplica las migraciones pendientes al iniciar. El seed NO se invoca manualmente: se configura mediante `UseSeeding()` / `UseAsyncSeeding()` en las opciones del `DbContext`, y EF Core lo ejecuta automáticamente al finalizar `MigrateAsync()`, `Migrate()`, `EnsureCreatedAsync()` o `EnsureCreated()`.

```text
La spec genera los archivos.
La app ejecuta los archivos pendientes.
EF Core dispara el seed automáticamente al final.
```

## Ubicación recomendada

Crear la extensión en:

```text
app/backend/src/RealtorApi/Infrastructure/Persistence/MigrationExtensions.cs
```

Crear el seeder en:

```text
app/backend/src/RealtorApi/Infrastructure/Persistence/DatabaseSeeder.cs
```

El seeder centraliza la lógica de carga de datos y es invocado únicamente desde los delegados `UseSeeding()` y `UseAsyncSeeding()` configurados en el registro del `DbContext`.

## Implementación obligatoria del seeder

El seeder DEBE exponer una versión síncrona y una asíncrona con la misma lógica. EF Core invoca la versión síncrona (`UseSeeding`) desde `Migrate()`, `EnsureCreated()` y las herramientas de línea de comandos (`dotnet ef database update`), y la versión asíncrona (`UseAsyncSeeding`) desde `MigrateAsync()` y `EnsureCreatedAsync()`.

```csharp
using Microsoft.EntityFrameworkCore;
using RealtorApi.Domain.AuditLogs;

namespace RealtorApi.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    public static void Seed(AppDbContext context)
    {
        if (!context.AuditLogs.Any())
        {
            context.AuditLogs.Add(new AuditLog
            {
                Message = "Sistema iniciado",
                CreatedAt = DateTime.UtcNow
            });

            context.SaveChanges();
        }
    }

    public static async Task SeedAsync(AppDbContext context, CancellationToken cancellationToken = default)
    {
        if (!await context.AuditLogs.AnyAsync(cancellationToken))
        {
            context.AuditLogs.Add(new AuditLog
            {
                Message = "Sistema iniciado",
                CreatedAt = DateTime.UtcNow
            });

            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
```

## Registro obligatorio del DbContext con seeding

El seeding se configura en `DbContextOptionsBuilder` al registrar el `DbContext`. DEBEN configurarse AMBOS delegados:

```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options
        .UseNpgsql(builder.Configuration.GetConnectionString("Default"))
        .UseSeeding((context, _) =>
        {
            DatabaseSeeder.Seed((AppDbContext)context);
        })
        .UseAsyncSeeding(async (context, _, cancellationToken) =>
        {
            await DatabaseSeeder.SeedAsync((AppDbContext)context, cancellationToken);
        }));
```

Notas:

- El segundo parámetro del delegado es un `bool` que indica si EF Core creó o modificó el esquema durante la operación. Como todo seed DEBE ser idempotente, este parámetro se ignora (`_`) y la verificación de existencia se hace siempre dentro del seeder.
- Ambos delegados DEBEN delegar en `DatabaseSeeder` para no duplicar lógica.

## Implementación obligatoria de la extensión

La extensión solo aplica migraciones. El seed se dispara automáticamente al finalizar la operación porque está configurado en las opciones del `DbContext`.

```csharp
using Microsoft.EntityFrameworkCore;
using RealtorApi.Infrastructure.Persistence;

namespace RealtorApi.Infrastructure.Persistence;

public static class MigrationExtensions
{
    public static async Task MigrateAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (context.Database.IsRelational())
        {
            // MigrateAsync ejecuta UseAsyncSeeding automáticamente al finalizar.
            await context.Database.MigrateAsync();
        }
        else
        {
            // EnsureCreatedAsync también ejecuta UseAsyncSeeding automáticamente.
            await context.Database.EnsureCreatedAsync();
        }
    }
}
```

## Registro en `Program.cs`

La aplicación DEBE llamar la extensión antes de `app.Run()`:

```csharp
await app.MigrateAsync();

app.Run();
```

## Consideraciones específicas de EF Core 11

- El snapshot del modelo ahora registra el ID de la última migración. Si dos ramas generan migraciones divergentes, el merge producirá un conflicto en el snapshot. Esto es intencional: el conflicto DEBE resolverse descartando uno de los árboles de migraciones y generando una migración unificada, nunca editando el snapshot a mano para "silenciar" el conflicto.
- Existe un nuevo `dotnet ef database update --add` que genera y aplica una migración en un solo paso, compilándola en runtime. Este comando está PROHIBIDO en este flujo (ver Prohibiciones), porque viola la regla de que la spec genera los archivos de migración y estos deben ser revisados antes de aplicarse.
- Si al ejecutar `Migrate()` / `MigrateAsync()` aparece `OldMigrationVersionWarning`, significa que el snapshot fue generado por una versión mayor anterior de EF Core; la acción correcta es regenerar el snapshot, no crear una migración redundante.

## Reglas para bases de datos relacionales

Para PostgreSQL, SQL Server, MySQL u otra base relacional, usar:

```csharp
await context.Database.MigrateAsync();
```

Está prohibido usar `EnsureCreatedAsync()` para inicializar una base de datos relacional que usa migraciones.

## Reglas para proveedores no relacionales o pruebas

Si el proveedor no soporta migraciones o se trata de un proveedor de pruebas, se permite:

```csharp
await context.Database.EnsureCreatedAsync();
```

Este fallback solo aplica cuando:

- `context.Database.IsRelational()` es `false`.
- El proveedor no soporta migraciones.
- La spec o el plan justifican el uso de proveedor de pruebas.

En ambos casos el seed configurado con `UseSeeding()` / `UseAsyncSeeding()` se ejecuta automáticamente, por lo que la estrategia de seed es la misma sin importar el proveedor.

## Reglas de seed

Todo seed DEBE ser idempotente.

Un seed idempotente puede ejecutarse varias veces sin duplicar datos ni dejar la base en estado inconsistente. Esto es obligatorio porque EF Core invoca el seeding cada vez que se ejecuta `MigrateAsync()` / `EnsureCreatedAsync()`, es decir, en cada arranque de la aplicación.

Ejemplo correcto:

```csharp
if (!await context.AuditLogs.AnyAsync(cancellationToken))
{
    context.AuditLogs.Add(new AuditLog
    {
        Message = "Sistema iniciado",
        CreatedAt = DateTime.UtcNow
    });

    await context.SaveChangesAsync(cancellationToken);
}
```

Ejemplo incorrecto:

```csharp
context.AuditLogs.Add(new AuditLog
{
    Message = "Sistema iniciado",
    CreatedAt = DateTime.UtcNow
});

await context.SaveChangesAsync(cancellationToken);
```

Además:

- La versión síncrona (`Seed`) y la asíncrona (`SeedAsync`) DEBEN mantener la misma lógica y mantenerse sincronizadas.
- El delegado asíncrono DEBE propagar el `CancellationToken` a todas las operaciones de EF Core.

## Prohibiciones

Está prohibido:

- Crear tablas manualmente en la base de datos.
- Ejecutar scripts SQL no trazados.
- Duplicar lógica de migración en varios lugares.
- Crear múltiples métodos de arranque para aplicar migraciones.
- Ejecutar seeds no idempotentes.
- Usar `EnsureCreatedAsync()` en bases relacionales con migraciones.
- Hacer que la aplicación genere archivos de migración automáticamente.
- Usar `dotnet ef database update --add` (EF Core 11) para generar y aplicar migraciones en un solo paso sin revisión.
- Resolver conflictos de merge del snapshot del modelo editándolo manualmente en lugar de unificar las migraciones divergentes.
- Invocar el seeder manualmente después de `MigrateAsync()` o en cualquier otro punto del arranque; el seed solo se ejecuta a través de `UseSeeding()` / `UseAsyncSeeding()`.
- Configurar solo uno de los dos delegados de seeding; ambos son obligatorios.
- Usar `HasData()` (model seeding) para datos que requieren lógica condicional o valores dinámicos como `DateTime.UtcNow`; para esos casos usar exclusivamente este mecanismo.

## Relación con `tasks.md`

Cuando una spec requiera cambios persistentes, `tasks.md` debe incluir tareas similares a:

```md
- [ ] Crear entidades persistentes requeridas por la spec.
- [ ] Crear configuraciones EF Core para las entidades nuevas.
- [ ] Generar una migración EF Core única para el cambio funcional de la spec.
- [ ] Revisar el contenido de la migración generada.
- [ ] Actualizar el seeder idempotente (`DatabaseSeeder.Seed` y `DatabaseSeeder.SeedAsync`) si la spec requiere datos iniciales.
- [ ] Verificar que el registro del `DbContext` configura `UseSeeding()` y `UseAsyncSeeding()`.
- [ ] Verificar que `MigrateAsync` aplica las migraciones al iniciar la aplicación y que el seed se ejecuta automáticamente.
```

La tarea de aplicar la migración manualmente con `dotnet ef database update` NO debe ser obligatoria en `tasks.md`, porque la aplicación lo hace al iniciar. Si se llegara a ejecutar manualmente, el seeding síncrono (`UseSeeding`) se ejecutará también, por lo que la idempotencia sigue siendo obligatoria.

## Checklist de revisión

Antes de marcar completada una spec con cambios persistentes, validar:

- [ ] Existe la migración generada.
- [ ] La migración fue revisada.
- [ ] `MigrateAsync` (extensión) existe y solo aplica migraciones, sin llamadas manuales al seeder.
- [ ] `Program.cs` llama `await app.MigrateAsync();` antes de `app.Run()`.
- [ ] El registro del `DbContext` configura `UseSeeding()` y `UseAsyncSeeding()`, ambos delegando en `DatabaseSeeder`.
- [ ] `DatabaseSeeder.Seed` y `DatabaseSeeder.SeedAsync` tienen la misma lógica y son idempotentes.
- [ ] El `CancellationToken` se propaga en la versión asíncrona.
- [ ] No se usó `dotnet ef database update` como requisito obligatorio de finalización.
- [ ] No hay scripts SQL manuales no trazados.
- [ ] No se usa `EnsureCreatedAsync()` para bases relacionales con migraciones.
