# Instrucciones de base de datos y persistencia

Estas instrucciones aplican a todo cambio relacionado con:

- EF Core.
- Npgsql.
- PostgreSQL.
- entidades persistentes.
- `AppDbContext`.
- configuraciones `IEntityTypeConfiguration<T>`.
- migraciones EF Core.
- seeders.
- inicialización de la base de datos.

El proyecto usa:

- EF Core.
- Npgsql.
- PostgreSQL.
- configuraciones separadas mediante `IEntityTypeConfiguration<T>`.
- migraciones EF Core versionadas.
- `UseSeeding()` y `UseAsyncSeeding()` para datos iniciales.
- seeds idempotentes.
- aplicación automática de migraciones pendientes al iniciar la aplicación.

Estas instrucciones definen las reglas obligatorias de persistencia.

Los procedimientos operativos detallados DEBEN seguir los skills oficiales del proyecto.

---

## Relación con Spec-Driven Development

Todo cambio de persistencia DEBE originarse en una spec aprobada.

Está prohibido:

- crear entidades persistentes no descritas en `spec.md`
- modificar el modelo de datos fuera del alcance de la spec vigente
- generar migraciones sin tarea correspondiente en `tasks.md`
- agregar o modificar seeds sin justificación en la spec
- modificar manualmente el esquema de la base de datos para evitar el flujo definido
- crear archivos de migración durante el arranque de la aplicación

Antes de realizar cualquier cambio de persistencia, el agente DEBE:

1. Leer `spec.md`.
2. Leer `plan.md`.
3. Leer `tasks.md`.
4. Identificar la tarea específica relacionada con persistencia.
5. Determinar qué skill corresponde aplicar.
6. Implementar únicamente los cambios definidos por la spec.
7. Marcar la tarea como `[X]` solo cuando haya sido completada y verificada.

---

## Skills obligatorios

Todo trabajo de persistencia DEBE seguir uno o más de los siguientes skills según el tipo de cambio:

```text
.github/skills/ef-core-entity-configuration/SKILL.md
.github/skills/ef-core-migrations/SKILL.md
.github/skills/database-migration-and-seeding/SKILL.md
```

Cada skill tiene una responsabilidad específica.

### `ef-core-entity-configuration`

Usar cuando una spec:

- agrega una entidad persistente
- modifica una entidad persistente
- agrega o modifica propiedades persistentes
- crea o modifica relaciones
- agrega índices
- agrega restricciones
- agrega conversiones de tipos
- requiere registrar un `DbSet<T>`

### `ef-core-migrations`

Usar cuando una spec modifica el modelo persistente y se necesita generar una migración EF Core versionada.

Este skill se ejecuta después de completar:

- entidades
- propiedades persistentes
- relaciones
- índices
- restricciones
- configuraciones `IEntityTypeConfiguration<T>`
- registros necesarios en `AppDbContext`

### `database-migration-and-seeding`

Usar para implementar y mantener el mecanismo centralizado que:

- aplica migraciones pendientes al iniciar la aplicación
- configura `UseSeeding()`
- configura `UseAsyncSeeding()`
- ejecuta seeds idempotentes mediante EF Core
- usa `MigrateAsync()` para proveedores relacionales
- usa `EnsureCreatedAsync()` únicamente como fallback controlado para proveedores no relacionales o de pruebas

---

## Flujo obligatorio de persistencia

Cuando una spec modifica el modelo persistente, el flujo esperado es:

```text
spec.md
    ↓
plan.md
    ↓
tasks.md
    ↓
Entidades o cambios de modelo
    ↓
DbSet<T> cuando corresponda
    ↓
IEntityTypeConfiguration<T>
    ↓
Migración EF Core versionada
    ↓
Inicio de la aplicación
    ↓
app.MigrateAsync()
    ↓
Database.MigrateAsync()
    ↓
EF Core evalúa y aplica 0..N migraciones pendientes
    ↓
UseAsyncSeeding()
    ↓
DatabaseSeeder.SeedAsync()
    ↓
Base de datos actualizada
```

El orden DEBE respetarse.

Está prohibido generar una migración antes de completar y validar el modelo persistente correspondiente.

---

## Configuración de entidades

Cada entidad persistente DEBE tener su propia clase de configuración EF Core.

Cada configuración DEBE implementar:

```csharp
IEntityTypeConfiguration<T>
```

Las configuraciones DEBEN vivir en:

```text
app/backend/src/RealtorApi/Infrastructure/Persistence/Configurations/
```

La convención obligatoria de nombres es:

```text
<NombreEntidad>Configuration.cs
```

El procedimiento detallado DEBE seguir:

```text
.github/skills/ef-core-entity-configuration/SKILL.md
```

---

## AppDbContext

`AppDbContext.OnModelCreating` DEBE descubrir las configuraciones automáticamente:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    modelBuilder.ApplyConfigurationsFromAssembly(
        typeof(AppDbContext).Assembly);
}
```

Está prohibido configurar entidades inline dentro de `OnModelCreating`.

---

## DbSet

Registrar `DbSet<T>` cuando corresponda según el modelo y los casos de uso.

Ejemplo:

```csharp
public DbSet<Property> Properties => Set<Property>();
```

No agregar `DbSet<T>` automáticamente para todas las entidades sin evaluar si corresponde.

---

## Migraciones EF Core

Toda modificación del modelo persistente que requiera cambio de esquema DEBE generar una migración EF Core versionada.

La generación DEBE seguir:

```text
.github/skills/ef-core-migrations/SKILL.md
```

La tarea correspondiente DEBE existir en `tasks.md`.

El proyecto DEBE compilar antes de generar la migración.

---

## Una migración por cambio funcional coherente

Está prohibido crear automáticamente una migración por cada entidad.

Ejemplo incorrecto:

```text
AddProperty
AddTenant
AddLease
```

Ejemplo correcto:

```text
AddPropertyManagementEntities
```

La migración representa un cambio funcional coherente, no necesariamente una sola entidad.

---

## Validaciones antes de generar una migración

Antes de generar cualquier migración, validar:

- [ ] Existe `spec.md`.
- [ ] Existe `plan.md`.
- [ ] Existe `tasks.md`.
- [ ] Existe una tarea explícita para la migración.
- [ ] Todas las entidades requeridas por la spec fueron creadas.
- [ ] Todos los cambios persistentes fueron completados.
- [ ] Los `DbSet<T>` necesarios fueron registrados.
- [ ] Las configuraciones `IEntityTypeConfiguration<T>` fueron creadas.
- [ ] Las relaciones fueron configuradas.
- [ ] Los índices fueron configurados.
- [ ] Las restricciones fueron configuradas.
- [ ] El proyecto compila.

Si alguna condición no se cumple, la generación de la migración DEBE detenerse.

---

## Generación de migraciones

Ejemplo:

```powershell
dotnet ef migrations add AddPropertyManagementEntities `
  --project app/backend/src/RealtorApi `
  --startup-project app/backend/src/RealtorApi `
  --output-dir Infrastructure/Persistence/Migrations
```

El nombre DEBE representar claramente el cambio funcional.

---

## Revisión obligatoria de migraciones

Toda migración generada DEBE revisarse antes de marcar su tarea como completada.

Revisar como mínimo:

- `Up()`
- `Down()`
- snapshot del modelo
- tablas
- columnas
- tipos de datos
- nulabilidad
- primary keys
- foreign keys
- índices
- restricciones
- renombramientos
- operaciones destructivas

Si existe riesgo de pérdida de datos, el agente DEBE detenerse y pedir confirmación explícita.

---

## Separación entre generación y ejecución de migraciones

### Generación

El skill:

```text
.github/skills/ef-core-migrations/SKILL.md
```

genera y revisa archivos de migración versionados.

### Ejecución

El skill:

```text
.github/skills/database-migration-and-seeding/SKILL.md
```

define cómo la aplicación aplica migraciones y ejecuta el seeding nativo de EF Core durante el arranque.

---

## Aplicación automática de migraciones

La aplicación DEBE centralizar la ejecución mediante:

```csharp
await app.MigrateAsync();
```

La implementación detallada DEBE seguir:

```text
.github/skills/database-migration-and-seeding/SKILL.md
```

Ejemplo:

```csharp
public static class MigrationExtensions
{
    public static async Task MigrateAsync(
        this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var context = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        if (context.Database.IsRelational())
        {
            await context.Database.MigrateAsync();
        }
        else
        {
            await context.Database.EnsureCreatedAsync();
        }
    }
}
```

La extensión NO DEBE invocar manualmente el seeder.

---

## Ejecución de seed sin migraciones pendientes

La aplicación DEBE invocar siempre:

```csharp
await app.MigrateAsync();
```

durante el arranque, incluso cuando se estime que no existen migraciones pendientes.

La ausencia de migraciones pendientes NO implica que el flujo de inicialización deba omitirse.

Para proveedores relacionales, la extensión ejecuta:

```csharp
await context.Database.MigrateAsync();
```

EF Core evalúa las migraciones pendientes y, al finalizar la operación, ejecuta el delegado configurado mediante `UseAsyncSeeding()` aunque ninguna migración nueva haya sido aplicada.

El flujo válido es:

```text
Aplicación arranca
    ↓
app.MigrateAsync()
    ↓
Database.MigrateAsync()
    ↓
EF Core evalúa 0..N migraciones pendientes
    ↓
UseAsyncSeeding()
    ↓
DatabaseSeeder.SeedAsync()
    ↓
Base de datos lista
```

El seeder DEBE ser idempotente porque será evaluado en cada ejecución del flujo de inicialización.

Está prohibido condicionar la llamada a `MigrateAsync()` únicamente a la existencia de migraciones pendientes.

Ejemplo prohibido:

```csharp
var pendingMigrations =
    await context.Database.GetPendingMigrationsAsync();

if (pendingMigrations.Any())
{
    await context.Database.MigrateAsync();
}
```

Este patrón puede impedir que `UseAsyncSeeding()` se ejecute cuando no existen migraciones pendientes.

El patrón correcto es:

```csharp
await context.Database.MigrateAsync();
```

Está prohibido invocar manualmente:

```csharp
await DatabaseSeeder.SeedAsync(context);
```

como fallback cuando no existan migraciones pendientes.

El seed DEBE ejecutarse exclusivamente mediante:

```text
UseSeeding()
UseAsyncSeeding()
```

---

## Registro obligatorio del seeding

El `DbContext` DEBE configurar ambos mecanismos:

```csharp
UseSeeding()
```

y:

```csharp
UseAsyncSeeding()
```

Ejemplo:

```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options
        .UseNpgsql(
            builder.Configuration.GetConnectionString("Default"))
        .UseSeeding((context, _) =>
        {
            DatabaseSeeder.Seed((AppDbContext)context);
        })
        .UseAsyncSeeding(
            async (context, _, cancellationToken) =>
            {
                await DatabaseSeeder.SeedAsync(
                    (AppDbContext)context,
                    cancellationToken);
            }));
```

Ambos delegados DEBEN delegar en `DatabaseSeeder`.

Está prohibido duplicar la lógica de seed dentro de los delegados.

---

## DatabaseSeeder

El seeder DEBE vivir en:

```text
app/backend/src/RealtorApi/Infrastructure/Persistence/DatabaseSeeder.cs
```

Debe exponer:

```csharp
Seed(AppDbContext context)
```

y:

```csharp
SeedAsync(
    AppDbContext context,
    CancellationToken cancellationToken = default)
```

Las versiones síncrona y asíncrona DEBEN:

- implementar la misma lógica
- mantenerse sincronizadas
- ser idempotentes
- evitar duplicación de datos

La versión asíncrona DEBE propagar el `CancellationToken`.

---

## Registro en Program.cs

La aplicación DEBE llamar:

```csharp
await app.MigrateAsync();

app.Run();
```

`Program.cs` NO DEBE contener lógica de migración ni lógica de seed.

---

## Proveedores relacionales

Para PostgreSQL y cualquier proveedor relacional que use migraciones:

```csharp
await context.Database.MigrateAsync();
```

Está prohibido usar `EnsureCreatedAsync()` para una base de datos relacional que utiliza migraciones.

---

## Proveedores no relacionales o pruebas

Se permite:

```csharp
await context.Database.EnsureCreatedAsync();
```

únicamente cuando:

- `context.Database.IsRelational()` es `false`
- el proveedor no soporta migraciones
- la spec o `plan.md` justifican el proveedor utilizado

---

## Regla sobre `dotnet ef database update`

Está prohibido usar:

```text
dotnet ef database update
```

como paso obligatorio de cierre de cada spec.

La aplicación aplica normalmente las migraciones pendientes al iniciar mediante:

```csharp
await app.MigrateAsync();
```

---

## Reglas específicas para EF Core 11

### Conflictos en el snapshot

Si dos ramas generan árboles de migraciones divergentes y se produce un conflicto en el snapshot, está prohibido editarlo manualmente para silenciar el conflicto.

El conflicto DEBE resolverse unificando correctamente las migraciones.

### `dotnet ef database update --add`

Está prohibido usar:

```text
dotnet ef database update --add
```

dentro del flujo del proyecto.

Las migraciones DEBEN generarse explícitamente, almacenarse, revisarse y aplicarse mediante el flujo aprobado.

### `OldMigrationVersionWarning`

Si aparece `OldMigrationVersionWarning`, el agente DEBE investigar la versión del snapshot y regenerarlo correctamente.

Está prohibido crear una migración redundante únicamente para silenciar la advertencia.

---

## HasData

Está prohibido usar `HasData()` para datos que requieran:

- lógica condicional
- valores dinámicos
- `DateTime.UtcNow`
- consultas previas
- decisiones basadas en existencia
- integración con otros datos

Para estos casos usar:

```text
DatabaseSeeder
+
UseSeeding()
+
UseAsyncSeeding()
```

---

## Responsabilidades de MigrationExtensions

`MigrationExtensions` DEBE:

- crear el scope de servicios
- resolver `AppDbContext`
- detectar si el proveedor es relacional
- ejecutar `MigrateAsync()` para proveedores relacionales
- ejecutar `EnsureCreatedAsync()` únicamente como fallback controlado

`MigrationExtensions` NO DEBE:

- invocar `DatabaseSeeder` manualmente
- comprobar migraciones pendientes para decidir si llamar o no a `MigrateAsync()`
- crear migraciones
- generar archivos de código
- crear tablas manualmente
- modificar el modelo EF Core
- contener datos de seed
- contener lógica de negocio

---

## Responsabilidades de DatabaseSeeder

`DatabaseSeeder` DEBE:

- contener la lógica de seed
- ofrecer versión síncrona
- ofrecer versión asíncrona
- mantener ambas versiones funcionalmente equivalentes
- ejecutar seeds idempotentes
- respetar dependencias entre datos iniciales
- evitar duplicación de datos
- propagar `CancellationToken` en operaciones asíncronas

`DatabaseSeeder` NO DEBE:

- ejecutar migraciones
- modificar el esquema
- crear archivos de migración
- implementar lógica de negocio
- ser invocado manualmente desde `MigrationExtensions`

---

## SQL manual

Está prohibido crear tablas manualmente en PostgreSQL para evitar EF Core migrations.

Está prohibido ejecutar scripts SQL no trazados a una tarea en `tasks.md`.

---

## Operaciones destructivas

Ante cualquier cambio destructivo, el agente DEBE detenerse.

Ejemplos:

- eliminación de tablas
- eliminación de columnas
- cambios de tipo con riesgo de pérdida de datos
- reducción de longitud de columnas
- cambios de nulabilidad incompatibles con datos existentes
- eliminación de relaciones
- eliminación de índices críticos

El agente DEBE solicitar confirmación explícita antes de continuar.

---

## Relación con tasks.md

Cuando una spec requiera cambios persistentes, `tasks.md` debe incluir tareas equivalentes a:

```md
- [ ] Crear las entidades persistentes requeridas por la spec.
- [ ] Crear las configuraciones EF Core correspondientes.
- [ ] Registrar los DbSet<T> necesarios.
- [ ] Generar una migración EF Core única para el cambio funcional de la spec.
- [ ] Revisar el contenido de la migración generada.
- [ ] Actualizar DatabaseSeeder.Seed y DatabaseSeeder.SeedAsync si la spec requiere datos iniciales.
- [ ] Verificar que UseSeeding y UseAsyncSeeding estén configurados.
- [ ] Verificar que app.MigrateAsync() se ejecuta siempre al iniciar la aplicación.
- [ ] Verificar que EF Core evalúa 0..N migraciones pendientes.
- [ ] Verificar que EF Core ejecuta automáticamente el seed aunque no existan migraciones pendientes.
```

No agregar como tarea obligatoria:

```text
Ejecutar dotnet ef database update
```

---

## Prohibiciones

Está prohibido:

- Crear tablas manualmente en PostgreSQL.
- Crear entidades persistentes fuera de una spec aprobada.
- Modificar el esquema sin migración cuando corresponda.
- Usar scripts SQL no trazados.
- Configurar entidades inline en `OnModelCreating`.
- Crear migraciones sin tarea en `tasks.md`.
- Generar migraciones antes de completar el modelo persistente.
- Crear automáticamente una migración por cada entidad de una misma spec.
- Ejecutar `dotnet ef database update` como paso obligatorio de cierre de una spec.
- Usar `dotnet ef database update --add`.
- Generar archivos de migración durante el arranque.
- Usar `EnsureCreatedAsync()` en bases relacionales con migraciones.
- Crear seeds no idempotentes.
- Insertar datos duplicados durante el arranque.
- Invocar el seeder manualmente después de `MigrateAsync()`.
- Invocar el seeder manualmente desde `MigrationExtensions`.
- Condicionar `MigrateAsync()` a la existencia de migraciones pendientes.
- Usar `GetPendingMigrationsAsync()` para omitir `MigrateAsync()`.
- Configurar solo `UseSeeding()` o solo `UseAsyncSeeding()`.
- Duplicar lógica de seed dentro de los delegados.
- Usar `HasData()` para datos dinámicos o condicionales.
- Resolver conflictos de snapshot editándolo manualmente para ocultar el conflicto.
- Marcar una tarea como `[X]` sin revisar la migración generada.

---

## Checklist antes de finalizar una tarea de persistencia

### Entidades y configuraciones

- [ ] El cambio está descrito en `spec.md`.
- [ ] Existe una tarea correspondiente en `tasks.md`.
- [ ] La entidad está ubicada correctamente.
- [ ] `DbSet<T>` fue registrado si corresponde.
- [ ] Existe `<Entidad>Configuration.cs`.
- [ ] La configuración implementa `IEntityTypeConfiguration<T>`.
- [ ] `OnModelCreating` usa `ApplyConfigurationsFromAssembly`.
- [ ] No existe configuración inline.

### Migraciones

- [ ] La migración tiene nombre descriptivo.
- [ ] La migración representa un cambio funcional coherente.
- [ ] No existen migraciones innecesarias por entidad.
- [ ] `Up()` fue revisado.
- [ ] `Down()` fue revisado.
- [ ] El snapshot fue revisado.
- [ ] Las tablas y columnas fueron verificadas.
- [ ] Las relaciones fueron verificadas.
- [ ] Los índices fueron verificados.
- [ ] No existen operaciones destructivas sin aprobación.

### Registro de seeding

- [ ] El `DbContext` configura `UseSeeding()`.
- [ ] El `DbContext` configura `UseAsyncSeeding()`.
- [ ] Ambos delegados delegan en `DatabaseSeeder`.
- [ ] No existe lógica duplicada dentro de los delegados.

### DatabaseSeeder

- [ ] Existe `DatabaseSeeder.Seed`.
- [ ] Existe `DatabaseSeeder.SeedAsync`.
- [ ] Ambas versiones implementan la misma lógica.
- [ ] Los seeds son idempotentes.
- [ ] El `CancellationToken` se propaga correctamente.
- [ ] No existen inserciones duplicadas después de múltiples ejecuciones.

### Ejecución automática

- [ ] Existe la extensión `MigrateAsync`.
- [ ] `MigrationExtensions` solo aplica migraciones o ejecuta el fallback autorizado.
- [ ] `MigrationExtensions` no invoca manualmente el seeder.
- [ ] `Program.cs` llama `await app.MigrateAsync();` antes de `app.Run()`.
- [ ] La llamada a `MigrateAsync()` no está condicionada por `GetPendingMigrationsAsync()`.
- [ ] PostgreSQL usa `MigrateAsync()`.
- [ ] `EnsureCreatedAsync()` solo se usa como fallback no relacional o de pruebas.
- [ ] La aplicación inicia correctamente.
- [ ] EF Core evalúa y aplica 0..N migraciones pendientes.
- [ ] EF Core ejecuta automáticamente `UseAsyncSeeding()` incluso cuando no hay migraciones pendientes.
- [ ] La base de datos queda correctamente inicializada en cada arranque.

Solo después de completar las validaciones correspondientes se puede marcar la tarea como `[X]`.
