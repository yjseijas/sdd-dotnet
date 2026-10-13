# Especificación de funcionalidad: Listado paginado de propiedades

**Rama de funcionalidad**: `004-properties-list-pagination`

**Creada**: 2026-10-13

**Estado**: Borrador

> El estado de la especificación se modifica automáticamente del flujo Speckit: Borrador → Aprobada → En implementación → Implementada. No editarlo manualmente fuera de ese flujo.

**Entrada**: Descripción del usuario: "Crear la especificación 004-properties-list-pagination. Definir un listado paginado de propiedades con imágenes públicas absolutas, ordenado por título, con validación y metadatos de paginación."

## Escenarios de usuario y pruebas *(obligatorio)*

### Historia de usuario 1: Consultar el catálogo paginado (Prioridad: P1)

Quiero consultar las propiedades en páginas ordenadas por título y con sus imágenes públicas para revisar el catálogo sin cargar todas las propiedades a la vez.

**Motivo de prioridad**: El listado es la capacidad principal de consulta del catálogo y permite recorrer los datos existentes en segmentos previsibles.

**Prueba independiente**: Consultar la primera página con valores predeterminados y después solicitar otra página; verificar el orden, los elementos y los metadatos de ambas respuestas.

**Escenarios de aceptación**:

1. **Dado** que existen propiedades, **Cuando** consulto el listado sin indicar página ni tamaño, **Entonces** recibo la página 1 con un máximo de 6 propiedades, ordenadas por título ascendente, junto con los metadatos de paginación.
2. **Dado** que existen al menos 13 propiedades, **Cuando** consulto la página 2 con tamaño 6, **Entonces** recibo las propiedades que corresponden a esa página y los valores de total, páginas y navegación coinciden con el conjunto completo.
3. **Dado** que una propiedad tiene una imagen persistida, **Cuando** aparece en el listado, **Entonces** su `imageUrl` identifica su imagen mediante una URL absoluta pública servida por la aplicación.

### Historia de usuario 2: Recibir errores claros ante parámetros inválidos (Prioridad: P2)

Quiero recibir un error de validación comprensible cuando solicite una página o un tamaño de página inválido, para corregir la consulta sin obtener resultados engañosos.

**Motivo de prioridad**: La validación evita solicitudes de paginación sin sentido y hace explícitos los errores de entrada.

**Prueba independiente**: Enviar valores no enteros o valores menores o iguales a cero para `page` o `pageSize` y comprobar que cada solicitud inválida se rechaza con estado 400 y detalles de validación.

**Escenarios de aceptación**:

1. **Dado** que `page` o `pageSize` es cero, negativo o no representa un entero válido, **Cuando** solicito el listado, **Entonces** recibo una respuesta 400 con detalles de validación.
2. **Dado** que solicito una página posterior a la última disponible, **Cuando** consulto el listado, **Entonces** recibo una página vacía con metadatos coherentes y sin error de servidor.

### Casos límite

- Si no existen propiedades, el listado devuelve una colección vacía, cero elementos y cero páginas, con `hasNext` y `hasPrevious` en falso.
- Si la página solicitada supera la última página, devuelve una colección vacía; `hasPrevious` refleja si la página indicada es posterior a la primera y `hasNext` es falso.
- Si falta alguno de los parámetros, se aplica su valor predeterminado de manera independiente.
- Si una consulta contiene un parámetro de paginación que no sea un entero positivo, se rechaza con estado 400 y un detalle asociado al valor inválido.
- Si varias propiedades comparten título, se ordenan por identificador ascendente como desempate para mantener un orden estable entre páginas.

## Requisitos *(obligatorio)*

### Requisitos funcionales

- **RF-001**: El sistema DEBE ofrecer el listado de propiedades en `GET /api/properties`.
- **RF-002**: El sistema DEBE aceptar los parámetros de consulta `page` y `pageSize`, usando de forma independiente los valores predeterminados 1 y 6 cuando no se proporcionen.
- **RF-003**: El sistema DEBE rechazar con estado HTTP 400 y detalles `ProblemDetails` o `ValidationProblemDetails` los valores de paginación que no sean enteros positivos.
- **RF-004**: El sistema DEBE ordenar las propiedades por título ascendente; cuando los títulos coincidan, DEBE usar el identificador ascendente como desempate.
- **RF-005**: Cada elemento del listado DEBE incluir al menos `id`, `title`, `description`, `address`, `price`, `status`, `bedroomCount`, `bathroomCount`, `areaSquareMeters` e `imageUrl`, sin exponer directamente la entidad persistente.
- **RF-006**: Cada `imageUrl` DEBE ser una URL absoluta pública servida por la aplicación, con el formato `http(s)://{host}/assets/properties/{fileName}`; no DEBE exponer rutas físicas internas ni devolver una ruta relativa.
- **RF-007**: El sistema DEBE construir `imageUrl` utilizando el esquema y el host de la solicitud actual.
- **RF-008**: La respuesta DEBE incluir los metadatos `page`, `pageSize`, `totalItems`, `totalPages`, `hasNext` y `hasPrevious`, consistentes con el resultado y el total de propiedades. `totalPages` DEBE ser el redondeo hacia arriba de `totalItems / pageSize` (cero si no hay elementos); `hasNext` DEBE indicar si `page < totalPages` y `hasPrevious` si `page > 1`.
- **RF-009**: El sistema DEBE propagar la cancelación de la solicitud durante la obtención y composición de los resultados.
- **RF-010**: `app/backend/src/RealtorApi/RealtorApi.http` DEBE incluir ejemplos para `GET /api/properties` y `GET /api/properties?page=1&pageSize=6`.

### Entidades principales *(si la funcionalidad maneja datos)*

- **Resumen de propiedad**: Representación de lectura de una propiedad existente con sus datos de catálogo y su URL pública de imagen.
- **Resultado paginado de propiedades**: Colección de resúmenes de propiedad y metadatos que identifican la página, el tamaño solicitado, el total y la disponibilidad de páginas anterior y siguiente.

## Criterios de éxito *(obligatorio)*

### Resultados medibles

- **CE-001**: Sin parámetros, el 100 % de las consultas válidas devuelve la primera página, con hasta 6 propiedades ordenadas por título ascendente.
- **CE-002**: Para un conjunto de 13 propiedades con tamaño de página 6, las tres páginas contienen 6, 6 y 1 propiedades respectivamente, y los metadatos reflejan 13 elementos y 3 páginas.
- **CE-003**: El 100 % de los elementos devueltos incluye una URL de imagen absoluta con host y esquema de la solicitud, sin rutas internas ni relativas.
- **CE-004**: El 100 % de las consultas con valores de paginación no enteros o menores o iguales a cero recibe estado 400 y detalles de validación.
- **CE-005**: Una página fuera del rango disponible devuelve cero elementos y metadatos coherentes, sin provocar un error de servidor.

## Supuestos

- La entidad persistente, los datos de propiedades y los assets públicos se definen en iniciativas previas; esta funcionalidad solo los consulta y no cambia el esquema persistente.
- `page` y `pageSize` deben ser enteros positivos. No se establece un máximo adicional para `pageSize` en esta especificación.
- Los estados se representan conforme al contrato del modelo persistente vigente.
- Las URL de imagen se construyen con el esquema y el host de la solicitud recibida, y las imágenes referenciadas ya están publicadas bajo `/assets/properties/`.
- Cuando dos propiedades comparten título, el identificador se utiliza como criterio secundario de orden ascendente.
