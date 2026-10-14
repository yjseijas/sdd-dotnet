# Plan de implementación: Listado paginado de propiedades

**Rama**: `004-properties-list-pagination` | **Fecha**: 2026-10-13 | **Especificación**: [spec.md](./spec.md)

**Entrada**: Especificación de funcionalidad de `/specs/004-properties-list-pagination/spec.md`

## Resumen

Agregar el caso de uso `ListProperties` como slice de ASP.NET Core Minimal API auto-descubrible. El handler consultará `Property` con una proyección EF Core de solo lectura, devolverá hasta 100 resultados por página y calculará los metadatos totales. El mapping de respuesta convertirá el estado a texto y construirá una URL absoluta por imagen con el esquema y host de la solicitud. Para que el contrato de imagen sea servible, se conservará la ruta pública existente `/images` y se agregará el alias `/assets` sobre el mismo proveedor físico. La validación de query se integrará en el filtro central de slices, sin registrar validadores por endpoint ni modificar el esquema persistente.

## Contexto técnico

**Lenguaje/versión**: C# con .NET 11, según `global.json` y el proyecto backend.

**Dependencias principales**: ASP.NET Core Minimal APIs, EF Core, Npgsql, FluentValidation, `ISlice`, `IHandler<TRequest, TResult>` y `Result<T>` existentes. No se agregan paquetes.

**Persistencia**: PostgreSQL mediante `AppDbContext` y EF Core. Solo lectura, con `AsNoTracking`; no requiere entidad, configuración, seed ni migración nuevos.

**Pruebas**: xUnit, limitado a pruebas unitarias por la constitución. Cubrir validator, mapping de URL/estado y cálculo de metadatos. Verificar el endpoint y las consultas HTTP mediante `RealtorApi.http` y la guía manual; no agregar pruebas de integración.

**Plataforma objetivo**: API ASP.NET Core `RealtorApi` en .NET 11. La URL absoluta utiliza `Request.Scheme` y `Request.Host` tal como llegan a la aplicación.

**Tipo de proyecto**: Backend web de una solución Blazor con endpoint Minimal API.

**Objetivos de rendimiento**: Máximo 100 propiedades por página; una consulta de conteo y una consulta paginada proyectada, ambas asíncronas y cancelables. No se establece una latencia objetivo ni se modifica el esquema para agregar índices.

**Restricciones**: Sin controllers; slice auto-descubrible; contratos explícitos; filtro automático FluentValidation; no exponer entidades EF; no cambiar persistencia ni rutas de imagen ya almacenadas; conservar `/images` mientras se sirve también `/assets/properties/{fileName}`; respetar las reglas de unit testing.

**Escala/alcance**: Listado de propiedades existentes, orden estable por título e identificador. No incluye búsqueda, filtrado, alta/edición, cambios de modelo, frontend ni un límite máximo para `page` distinto del tipo entero positivo aceptado.

## Comprobación de la constitución

*Punto de control inicial aprobado antes de la investigación; se vuelve a evaluar después del diseño.*

- [X] El backend propuesto usa ASP.NET Core Minimal APIs, sin controllers, y organiza la funcionalidad como slice `ListProperties`.
- [X] No se modifica la interfaz Blazor. El response DTO mantiene separado el contrato HTTP de la entidad persistente.
- [X] Se reutilizan `AppDbContext`, `ISlice`, el registro automático, handlers, FluentValidation y `Result<T>` existentes; no se agregan dependencias ni abstracciones de persistencia.
- [X] Los requisitos son verificables. Se cubrirán reglas unitarias y se incluirán solicitudes HTTP manuales para el comportamiento integrado.
- [X] Se validan ambos parámetros, se limita `pageSize` a 100 y se evita exponer rutas físicas. El esquema y host se derivan de la solicitud actual conforme a la especificación.
- [X] Los artefactos de esta iniciativa se redactan en español, salvo identificadores técnicos y contratos de protocolo.
- [X] El tamaño máximo de página y el sobre JSON `{ items, page, pageSize, totalItems, totalPages, hasNext, hasPrevious }` quedaron fijados en la especificación. No se necesitan nuevas decisiones de stack.

### Revisión posterior al diseño

- [X] El diseño no altera el modelo persistente y evita migraciones.
- [X] La validación se aplica desde el registro central de slices y resuelve servicios desde el scope de la solicitud.
- [X] El alias estático satisface la nueva URL sin invalidar las rutas `/images` usadas por el seeder y el modelo vigente.
- [X] La estrategia de pruebas se limita a unit tests automatizados y solicitudes manuales, sin agregar pruebas de integración.

## Estructura del proyecto

### Documentación de esta funcionalidad

```text
specs/004-properties-list-pagination/
├── spec.md
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── properties-list.openapi.json
├── checklists/
│   └── requirements.md
└── tasks.md
```

`tasks.md` se generará durante la fase `speckit.tasks`, no en este plan.

### Código fuente (raíz del repositorio)

```text
app/backend/src/RealtorApi/
├── Features/
│   └── Properties/
│       └── ListProperties/
│           ├── ListPropertiesSlice.cs
│           ├── ListPropertiesHandler.cs
│           ├── ListPropertiesValidator.cs
│           └── ListPropertiesMapping.cs
├── Infrastructure/
│   ├── Endpoints/
│   │   └── SliceRegistry.cs             # Aplicar filtro global de validación
│   └── Validation/
│       └── ValidationFilterFactory.cs    # Usar servicios del scope HTTP
├── Program.cs                           # Servir alias estático /assets
└── RealtorApi.http                      # Solicitudes manuales del endpoint

app/backend/tests/RealtorApiTests/
└── Features/
    └── Properties/
        └── ListPropertiesTests.cs       # Pruebas unitarias del slice
```

**Decisión de estructura**: La funcionalidad nueva queda en `Features/Properties/ListProperties`, siguiendo la organización vertical aprobada. El slice delega en un handler transitorio descubierto por `IHandler<TRequest, TResult>`. Los contratos del caso de uso y mapping permanecen en el slice; la consulta accede directamente a `AppDbContext`. Se ajusta únicamente la infraestructura común de validación que hoy no se conecta desde `MapSlices`, porque esa conexión es necesaria para satisfacer el requisito de 400 con errores de validación. El endpoint no requiere registros manuales en `Program.cs`.

## Decisiones de diseño

1. El contrato JSON contiene `items` y los seis metadatos al mismo nivel. `status` se serializa como el nombre textual de `PropertyStatus`.
2. La consulta proyecta solo los campos requeridos con `AsNoTracking`, ordena por `Title` y luego `Id`, calcula `totalItems` y aplica `Skip`/`Take`. `totalPages` usa redondeo hacia arriba; `hasNext` equivale a `page < totalPages` y `hasPrevious` a `page > 1`.
3. La validación de valores de página se realiza sobre el request del slice mediante FluentValidation. Los valores ausentes usan 1 y 6; `page` debe ser positivo y `pageSize` debe estar en `[1, 100]`. La entrada no entera debe producir `ValidationProblemDetails` HTTP 400, no un valor predeterminado silencioso.
4. El mapping toma el nombre de archivo del `ImageUrl` persistido y crea una URI absoluta con `Request.Scheme`, `Request.Host` y `/assets/properties/{fileName}`. No devuelve la ruta almacenada `/images/...` ni una ruta física.
5. El backend conserva la ruta estática `/images` existente y expone el mismo proveedor también bajo `/assets`; de este modo, el nuevo enlace cumple la ruta pública especificada sin cambiar el seeder ni los valores ya persistidos.
6. La ausencia de resultados o una página fuera de rango produce `items: []` y metadatos calculados del total. No se transforma una página posterior a la última en una página diferente.
7. `SliceRegistry.MapSlices` debe aplicar `ValidationFilterFactory` al grupo de endpoints auto-descubiertos. La factory debe resolver el validator desde `HttpContext.RequestServices` y propagar `RequestAborted`, evitando resolver dependencias scoped desde el contenedor raíz.

## Registro de complejidad

| Decisión o complejidad | Necesidad que resuelve | Alternativa más simple descartada y motivo |
|------------------------|-----------------------|-------------------------------------------|
| Conectar el filtro global de validación a los slices | El registro actual mapea slices sin aplicar `ValidationFilterFactory`; sin el ajuste, las reglas del nuevo validator no garantizarían el contrato de 400 | Validar manualmente en el endpoint o agregar un filtro individual contradice los skills y duplica convenciones |
| Alias de assets `/assets` manteniendo `/images` | La API debe responder con URL absoluta `/assets/properties/{fileName}`, mientras los assets y seeds vigentes usan `/images/properties` | Cambiar las rutas persistidas o eliminar `/images` afectaría la spec previa y queda fuera del alcance |
