# Modelo de datos de la feature

## Entidades existentes reutilizadas

### `Property`

La entidad ya existe en el dominio y se reutiliza para la consulta paginada:

- `Id`: identificador único de la propiedad.
- `Title`: nombre de la propiedad.
- `Description`: descripción breve de la propiedad.
- `Address`: ubicación o dirección.
- `Price`: precio ofertado.
- `Status`: estado operativo (`Available`, `Rented`, `Maintenance`, etc.).
- `BedroomCount`, `BathroomCount`, `AreaSquareMeters`: datos de la vivienda.
- `ImageUrl`: URL pública de la imagen asociada, persistida desde el seeding.
- `CreatedAt`, `UpdatedAt`: metadatos de auditoría.

No se introduce una nueva entidad persistente ni un cambio del esquema de base de datos.

## Request de consulta

### `PropertyListQuery`

```csharp
public sealed record PropertyListQuery
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 6;
}
```

Reglas de validación:
- `Page` debe ser un entero positivo (`> 0`).
- `PageSize` debe ser un entero positivo y estar en rango `[1, 100]`.
- Si el valor no es un entero válido o cae fuera del rango, el endpoint devuelve `400 Bad Request` con `ValidationProblemDetails`.
- Si se envían parámetros faltantes, se aplican los valores por defecto `1` y `6` independientemente.

## Respuestas del catálogo

### `PropertySummaryResponse`

```csharp
public sealed record PropertySummaryResponse
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public string Status { get; init; } = string.Empty;
    public int BedroomCount { get; init; }
    public int BathroomCount { get; init; }
    public decimal AreaSquareMeters { get; init; }
    public string ImageUrl { get; init; } = string.Empty;
}
```

### `PaginatedPropertiesResponse`

```csharp
public sealed record PaginatedPropertiesResponse
{
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalItems { get; init; }
    public int TotalPages { get; init; }
    public bool HasNext { get; init; }
    public bool HasPrevious { get; init; }
    public IReadOnlyList<PropertySummaryResponse> Items { get; init; } = Array.Empty<PropertySummaryResponse>();
}
```

Reglas de metadatos:
- `TotalPages = Math.Ceiling(totalItems / (double)pageSize)` con un mínimo de `0` si no hay elementos.
- `HasNext = page < totalPages`.
- `HasPrevious = page > 1`.
- Si la página solicitada supera la última disponible, el `Items` debe ser vacío, pero los metadatos deben seguir siendo coherentes con el total.

## Orden de resultados

La consulta debe aplicarse en este orden:
1. `Title` ascendente.
2. `Id` ascendente como desempate.

Esto garantiza estabilidad entre páginas y evita resultados inconsistentes cuando varias propiedades comparten título.

## Reglas de seguridad y validación del contrato

- La respuesta no debe devolver directamente la entidad `Property` ni propiedades internas adicionales.
- `imageUrl` debe construirse con el esquema y host actual de la solicitud, por ejemplo `https://localhost:5001/assets/properties/imagen.jpg`.
- La respuesta debe ser segura para consumo desde Blazor y no incluir rutas locales del sistema de archivos ni secretos de infraestructura.
