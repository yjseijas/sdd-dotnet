---
name: 004-properties-list-pagination
agent: speckit.specify
---

/speckit.specify

Crear la especificación 004-properties-list-pagination.

## Objetivo
Definir e implementar un endpoint Minimal API basado en ISlice para devolver la lista paginada de propiedades, incluyendo la imagen correspondiente de cada propiedad mediante URL absoluta pública.

## Contexto obligatorio
- Respetar reglas de backend y persistencia vigentes del repositorio.
- Seguir skills oficiales aplicables:
  - minimal-api-slices
  - minimal-api-validation
  - vertical-slice-handlers
  - vertical-slice-mapping
  - result-problem-details
- Mantener consistencia con el patrón ISlice y auto-descubrimiento de endpoints.
- Mantener compatibilidad con el modelo persistente existente de propiedades.
- No introducir controllers.

## Estructura obligatoria
La lógica del endpoint debe vivir en Features dentro de RealtorApi:

app/backend/src/RealtorApi/
- Features/
  - Properties/
    - ListProperties/
      - ListPropertiesSlice.cs (o separación equivalente Endpoint/Handler/Mapping/Validators)
- Infrastructure/
  - Slices/
  - Handlers/
  - Validation/
- Domain/
- Program.cs

Regla obligatoria:
- Crear carpeta Features en el root de RealtorApi (si no existe) y colocar allí el slice de negocio.

## Alcance funcional incluido

### 1) Endpoint de listado paginado
- Método: GET
- Ruta: /api/properties
- Debe devolver lista paginada de propiedades.

### 2) Parámetros de paginación
- Query params:
  - page
  - pageSize
- Comportamiento por defecto cuando no llegan parámetros:
  - page = 1
  - pageSize = 6
- Orden obligatorio:
  - title ascendente

### 3) Contrato de respuesta
Cada item de propiedad debe incluir al menos:
- id
- title
- description
- address
- price
- status
- bedroomCount
- bathroomCount
- areaSquareMeters
- imageUrl

Requisito obligatorio de imagen:
- Debe devolverse imageUrl correspondiente a cada propiedad.
- imageUrl debe ser URL absoluta pública servible por API.
- Formato esperado de salida: `http(s)://{host}/assets/properties/{fileName}`
- Prohibido devolver rutas físicas internas del servidor.
- Prohibido devolver imageUrl relativo en la respuesta final del endpoint.

Metadatos de paginación requeridos:
- page
- pageSize
- totalItems
- totalPages
- hasNext
- hasPrevious

### 4) Validación y errores
- Validar parámetros page y pageSize.
- Si parámetros inválidos:
  - devolver 400 con ProblemDetails o ValidationProblemDetails.
- Propagar CancellationToken en todo flujo.

### 5) Mapping y acceso a datos
- No devolver entidades EF directamente.
- Usar contratos explícitos de response.
- Para lectura, preferir proyección EF Core directa con AsNoTracking.
- Mantener endpoint delgado y lógica de caso de uso en handler del slice.
- Construir imageUrl absoluta con base en el request actual (scheme + host) durante el mapping de respuesta.

### 6) Prueba manual de endpoint
- Agregar prueba de endpoint en app/backend/src/RealtorApi/RealtorApi.http:
  - GET /api/properties
  - GET /api/properties?page=1&pageSize=6

## Criterios de aceptación
1. Given propiedades seed existentes, when consulto GET /api/properties sin query params, then devuelve page 1 con hasta 6 registros ordenados por title ascendente.
2. Given propiedades suficientes, when consulto /api/properties?page=2&pageSize=6, then devuelve la página correcta con metadatos consistentes.
3. Given parámetros inválidos (por ejemplo page <= 0 o pageSize <= 0), when consulto endpoint, then devuelve 400 con detalle de validación.
4. Given propiedades con imagen persistida, when consulto listado, then cada propiedad incluye imageUrl absoluta válida.
5. Given host local `http://localhost:5023`, when consulto listado, then imageUrl sigue patrón `http://localhost:5023/assets/properties/{fileName}`.

## Restricciones explícitas
- No usar controllers.
- No implementar funcionalidades fuera del listado paginado.
- No romper convenciones ISlice existentes.
- No introducir mapeadores automáticos no requeridos.
- Todo cambio debe quedar trazado en tasks.md antes de marcar completado.