# Contratos de la feature

## Endpoint: `GET /api/properties`

### Parámetros de consulta

| Nombre | Tipo | Requerido | Valor por defecto | Validación |
|--------|------|-----------|-------------------|-----------|
| `page` | `int` | No | `1` | entero positivo (`> 0`) |
| `pageSize` | `int` | No | `6` | entero entre `1` y `100` |

### Respuesta exitosa (`200 OK`)

```json
{
  "page": 1,
  "pageSize": 6,
  "totalItems": 13,
  "totalPages": 3,
  "hasNext": true,
  "hasPrevious": false,
  "items": [
    {
      "id": "7d5ee947-9eb8-4aff-9eaf-3d3edceb0b5d",
      "title": "Apartamento central",
      "description": "Departamento con jardín y acceso inmediato.",
      "address": "Calle 123, Bogotá",
      "price": 350000000,
      "status": "Available",
      "bedroomCount": 3,
      "bathroomCount": 2,
      "areaSquareMeters": 120,
      "imageUrl": "https://localhost:5001/assets/properties/apartamento-central.jpg"
    }
  ]
}
```

### Error de validación (`400 Bad Request`)

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "pageSize": [
      "El valor de pageSize debe estar entre 1 y 100."
    ]
  }
}
```

### Reglas del contrato

- `items` contiene solo el resumen de lectura de la propiedad.
- Nunca se exponen entidades persistentes ni rutas locales del sistema de archivos.
- `imageUrl` siempre debe ser una URL pública absoluta con host y esquema.
- La respuesta debe mantener un orden estable por `Title` y `Id` en cada página.
- La página fuera de rango debe responder con `200` y una colección vacía, manteniendo metadatos coherentes.
