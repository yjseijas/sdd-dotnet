# Modelo de datos de lectura: Listado paginado de propiedades

**Fecha**: 2026-10-13  
**Especificación**: [spec.md](./spec.md)

Este caso de uso es de solo lectura. Reutiliza la entidad persistente `Property`; no agrega ni cambia tablas, columnas, relaciones, seeds o migraciones.

## Request de listado

Parámetros de consulta:

| Campo | Tipo de entrada | Predeterminado | Restricciones |
|------|------------------|----------------|---------------|
| `page` | Entero positivo | `1` | Mayor que cero |
| `pageSize` | Entero | `6` | Entre `1` y `100`, inclusive |

Los valores no numéricos se informan como errores de validación HTTP 400. Cada valor ausente toma su valor predeterminado independientemente.

## Elemento `items`

Cada elemento expone un contrato de respuesta, nunca la entidad EF:

| Campo JSON | Tipo | Origen o regla |
|-----------|------|----------------|
| `id` | UUID | `Property.Id` |
| `title` | Cadena | `Property.Title` |
| `description` | Cadena | `Property.Description` |
| `address` | Cadena | `Property.Address` |
| `price` | Número decimal | `Property.Price` |
| `status` | Cadena | Nombre de `PropertyStatus`: `Available`, `Rented` o `Maintenance` |
| `bedroomCount` | Entero | `Property.BedroomCount` |
| `bathroomCount` | Entero | `Property.BathroomCount` |
| `areaSquareMeters` | Número decimal | `Property.AreaSquareMeters` |
| `imageUrl` | URI absoluta | Esquema y host de la solicitud, ruta `/assets/properties/{fileName}` |

La lectura proyecta los campos requeridos con `AsNoTracking`. El orden es `title` ascendente y, para títulos iguales, `id` ascendente.

## Sobre paginado

El objeto de respuesta contiene `items` junto a estos metadatos:

| Campo | Tipo | Regla |
|------|------|-------|
| `page` | Entero | Página solicitada |
| `pageSize` | Entero | Tamaño de página solicitado |
| `totalItems` | Entero de 64 bits | Número total de propiedades, antes de paginar |
| `totalPages` | Entero de 64 bits | `0` si no hay elementos; en otro caso techo de `totalItems / pageSize` |
| `hasNext` | Booleano | `page < totalPages` |
| `hasPrevious` | Booleano | `page > 1` |

Si el catálogo está vacío o la página queda fuera del rango, `items` es una colección vacía. No se modifica el número de página solicitado.

## Dependencias

- **Persistencia existente**: `RealtorApi.Domain.Properties.Property` y `AppDbContext.Properties`.
- **Recursos estáticos existentes**: El seeder y manifiesto almacenan rutas públicas `/images/properties/{fileName}`. El backend debe exponer un alias `/assets` para servir los mismos archivos sin cambiar las rutas persistidas.
