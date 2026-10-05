# Skill: Configuración de Entidades EF Core

## Cuándo usar este skill

Usar este skill cada vez que una spec agregue, modifique o elimine una entidad persistente del dominio.

Este skill aplica cuando se trabaja con:

- Entidades de dominio.
- `AppDbContext`.
- `DbSet<T>`.
- Configuraciones EF Core mediante `IEntityTypeConfiguration<T>`.
- Relaciones, índices, restricciones y conversiones.

## Regla principal

Cada entidad persistente DEBE tener su propia clase de configuración EF Core.

Está prohibido configurar entidades inline dentro de `OnModelCreating`.

## Ubicación obligatoria

Las configuraciones DEBEN vivir en:

```text
app/backend/src/RealtorApi/Infrastructure/Persistence/Configurations/
```

## Convención de nombres

Cada archivo de configuración DEBE usar esta convención:

```text
<NombreEntidad>Configuration.cs
```

Ejemplos:

```text
PropertyConfiguration.cs
TenantConfiguration.cs
LeaseConfiguration.cs
AuditLogConfiguration.cs
```

## Registro obligatorio en `AppDbContext`

`AppDbContext.OnModelCreating` DEBE usar únicamente:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
}
```

No se permite agregar configuraciones manuales como:

```csharp
modelBuilder.Entity<Tenant>(...);
```

No se permite registrar configuraciones una por una si pueden descubrirse por assembly.

## Flujo obligatorio al agregar una entidad

Cuando una spec agregue una entidad persistente, seguir este orden:

1. Confirmar que existe una tarea en `tasks.md` que justifica la entidad.
2. Crear la entidad en la carpeta de dominio correspondiente.
3. Registrar `DbSet<T>` en `AppDbContext` cuando la entidad sea agregada como aggregate root o requiera consulta directa.
4. Crear `<Entidad>Configuration.cs` dentro de `Infrastructure/Persistence/Configurations/`.
5. Definir tabla, clave primaria, propiedades requeridas, longitudes máximas, relaciones, índices y conversiones.
6. Ejecutar revisión de consistencia del modelo antes de generar la migración.
7. Dejar pendiente la generación de migración para el skill `.skills/dotnet/ef-core-migrations/SKILL.md`.

## Ejemplo de configuración

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealtorApi.Domain.Properties;

namespace RealtorApi.Infrastructure.Persistence.Configurations;

public sealed class PropertyConfiguration : IEntityTypeConfiguration<Property>
{
    public void Configure(EntityTypeBuilder<Property> builder)
    {
        builder.ToTable("properties");

        builder.HasKey(property => property.Id);

        builder.Property(property => property.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(property => property.CreatedAt)
            .IsRequired();
    }
}
```

## Reglas de calidad

Toda configuración DEBE declarar explícitamente:

- Nombre de tabla.
- Clave primaria.
- Propiedades requeridas.
- Longitudes máximas para strings persistentes.
- Relaciones entre entidades.
- Índices requeridos por criterios de búsqueda o unicidad.
- Conversiones de value objects o enums cuando corresponda.

## Prohibiciones

Está prohibido:

- Configurar entidades inline en `OnModelCreating`.
- Crear carpetas globales `Repositories` para justificar persistencia.
- Mezclar entidades de dominio con requests o responses.
- Crear configuraciones no trazadas a `tasks.md`.
- Crear entidades persistentes sin plan de migración.

## Checklist de revisión

Antes de terminar la tarea, validar:

- [ ] La entidad existe en la carpeta de dominio correcta.
- [ ] `DbSet<T>` fue agregado si corresponde.
- [ ] Existe `<Entidad>Configuration.cs`.
- [ ] `OnModelCreating` usa `ApplyConfigurationsFromAssembly`.
- [ ] No hay configuración inline.
- [ ] La configuración tiene tabla, clave, propiedades requeridas y relaciones.
- [ ] La tarea correspondiente en `tasks.md` sigue pendiente hasta completar migración y validación.
