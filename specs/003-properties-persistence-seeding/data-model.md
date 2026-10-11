# Modelo de datos: propiedades y estados

## Entidades del dominio

### `PropertyStatus`

Enumeración del dominio que expresa la situación operativa de la propiedad.

Valores permitidos:
- `Available`
- `Rented`
- `Maintenance`

Reglas:
- Debe almacenarse como texto en la base de datos usando conversión EF Core.
- Debe usarse como valor de negocio y no como código numérico.

### `Property`

Entidad persistente de la cartera inmobiliaria.

| Campo | Tipo | Requerido | Descripción |
|---|---|---:|---|
| `Id` | `Guid` | Sí | Identificador único de la propiedad. |
| `Title` | `string` | Sí | Título legible para la propiedad. |
| `Description` | `string` | Sí | Descripción operativa y comercial. |
| `Address` | `string` | Sí | Dirección física de la propiedad. |
| `Price` | `decimal` | Sí | Precio de la propiedad. |
| `Status` | `PropertyStatus` | Sí | Estado operativo publicado en la base de datos como texto. |
| `BedroomCount` | `int` | Sí | Número de dormitorios. |
| `BathroomCount` | `int` | Sí | Número de baños. |
| `AreaSquareMeters` | `decimal` | Sí | Superficie útil en metros cuadrados. |
| `ImageUrl` | `string` | Sí | Ruta pública servida por la API, no una ruta física local. |
| `CreatedAt` | `DateTime` | Sí | Marca de tiempo de creación en UTC. |
| `UpdatedAt` | `DateTime?` | No | Última actualización; puede ser nula si nunca se modificó. |

## Reglas de validación del modelo

- `Title` debe tener contenido no vacío.
- `Description` debe ser no nula y con longitud mínima útil.
- `Address` debe ser no vacía.
- `Price` debe ser mayor a cero.
- `BedroomCount` y `BathroomCount` deben ser valores positivos o cero de forma defensiva.
- `AreaSquareMeters` debe ser mayor a cero.
- `ImageUrl` debe apuntar a una ruta pública soportada por la aplicación y no a archivos de soporte locales.
- El estado debe cumplirse con el enum `PropertyStatus` y su conversión EF Core.

## Relación y persistencia

- La entidad se registra como `DbSet<Property>` en `AppDbContext`.
- El mapping se realizará exclusivamente mediante `PropertyConfiguration` dentro de `Infrastructure/Persistence/Configurations`.
- La configuración usará `ValueConverter` o equivalente para persistir `PropertyStatus` como texto.
- La migración será una sola para la spec, agrupando todas las piezas de dominio y seed.

## Archivo de diseño del seeding

Los datos iniciales se resuelven desde un manifiesto central que define:
- JSON base de propiedades
- JSON de estados
- activos externos a publicar
- rutas públicas esperadas en runtime

Esto permite que el `DatabaseSeeder` cargue los datos sin rutas dispersas ni `HasData()` para valores dinámicos.
