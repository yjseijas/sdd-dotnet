---
name: vertical-slice-mapping
description: Define mappings explícitos por slice y selecciona la estrategia correcta para objetos, colecciones, consultas EF Core y streaming.
---

# Skill: Vertical Slice Mapping

## Objetivo

Mantener mappings explícitos, simples y localizados, seleccionando la estrategia según el comportamiento real del caso de uso.

## Cuándo usar

Usar cuando una tarea necesita transformar:

- request a dominio o entidad
- entidad a response
- entidad a evento
- colecciones en memoria
- consultas EF Core a DTOs
- flujos diferidos o streaming

## Decisión de estrategia

```text
Objeto individual
    ↓
Extension Block

Colección ya materializada
    ↓
Select + ToList

Consulta EF Core de lectura
    ↓
Select projection + ToListAsync

Streaming real
    ↓
IAsyncEnumerable<T> o yield return
```

## Mapping individual

Preferir C# Extension Blocks cuando mejoren claridad.

```csharp
internal static class CreatePropertyMappings
{
    extension(CreatePropertyRequest request)
    {
        internal Property ToEntity()
            => new(request.Name, request.Address);
    }
}
```

## Colecciones materializadas

```csharp
var response = properties
    .Select(x => x.ToSummaryResponse())
    .ToList();
```

Usar cuando la colección ya está en memoria y el resultado completo se necesita inmediatamente.

## Proyección EF Core

Preferir proyección directa para consultas de lectura:

```csharp
var response = await context.Properties
    .AsNoTracking()
    .Select(x => new PropertySummaryResponse(
        x.Id,
        x.Name,
        x.Status))
    .ToListAsync(cancellationToken);
```

No cargar entidades completas si solo se necesita una proyección.

## Streaming

Usar `IAsyncEnumerable<T>` o `yield return` solo cuando exista una necesidad real de streaming, procesamiento incremental o evaluación diferida.

## Ubicación

Mapping específico:

```text
<Slice>.Mapping.cs
```

Mapping realmente compartido:

```text
Shared/Mapping/
```

No mover a Shared anticipadamente.

## Prohibiciones

- lógica de negocio en mapping
- consultas a base de datos dentro de mapping
- validación dentro de mapping
- side effects dentro de mapping
- logging operacional dentro de mapping
- introducir AutoMapper, Mapster o Mapperly sin justificación aprobada

## Checklist

- [ ] El mapping es explícito.
- [ ] La estrategia elegida corresponde al escenario.
- [ ] No contiene lógica de negocio.
- [ ] Está localizado en el slice cuando es específico.
- [ ] Solo está en Shared si existe reutilización real.
- [ ] Las queries de lectura proyectan directamente cuando corresponde.
