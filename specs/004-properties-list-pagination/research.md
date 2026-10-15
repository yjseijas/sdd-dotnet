# Investigación: listado paginado de propiedades

## Decisión: paginación con `page` y `pageSize` independientes y con valores por defecto

**Rationale:** La especificación exige que ambos valores se resuelvan de forma independiente, con predeterminados de `1` y `6`, y que `pageSize` esté acotado a 1..100. Este enfoque evita estados ambiguos y hace explícitos los errores de validación mediante `ValidationProblemDetails`.

**Alternativas consideradas:**
- Requerir `page` y `pageSize` siempre presentes: se descarta porque contradice la especificación y obliga a clientes a enviar parámetros innecesarios.
- Usar un único parámetro global para la página: se descarta porque rompe la semántica de `page` y `pageSize` y complica la experiencia del cliente.

## Decisión: validación centralizada con `FluentValidation` y filtro de endpoint

**Rationale:** El proyecto base ya usa `AddValidatorsFromAssembly` y `ValidationFilterFactory` para validar request objects de forma automática. Reutilizar este mecanismo asegura que la validación del listado respete el patrón general del backend y devuelva respuestas 400 estándar.

**Alternativas consideradas:**
- Validar inline dentro del endpoint: se descarta porque duplica lógica y desacopla la regla del resto del sistema.
- Usar un modelo de validación distinto al proyecto base: se descarta porque introduce inconsistencia y complica la interoperabilidad con `ProblemDetails`.

## Decisión: generación de `imageUrl` desde el host y esquema actual

**Rationale:** El requisito exige que cada `imageUrl` sea una URL absoluta pública servida por la aplicación, no una ruta física ni relativa. La solución más robusta es construirla con `HttpContext.Request.Scheme`, `Host` y una ruta conocida de assets público, por ejemplo `https://{host}/assets/properties/{fileName}`.

**Alternativas consideradas:**
- Persistir rutas físicas locales del sistema de archivos: se descarta porque no son públicas ni portables.
- Devolver rutas relativas: se descarta porque la especificación exige una URL absoluta con esquema y host.

## Decisión: proyección del resultado del catálogo sin exponer la entidad persistente

**Rationale:** La entidad `Property` puede manejarse internamente, pero la API debe devolver un DTO de lectura con solo los campos requeridos por el catálogo. Esto separa el dominio del contrato externo y preserva el requisito de no exponer la entidad persistente.

**Alternativas consideradas:**
- Serializar `Property` directamente: se descarta porque expone más campos de los necesarios y deja el contrato desacoplado del dominio.
- Crear un modelo de respuesta por cada vista: se descarta por ruido, cuando un único DTO de resumen junto con la estructura paginada resulta suficiente.

## Decisión: no introducir cambios de esquema ni migraciones

**Rationale:** La feature solo consulta propiedades existentes; no requiere crear ni modificar entidades persistentes. Mantener el modelo actual evita migraciones innecesarias y respeta la separación de esta especificación con la anterior de persistencia y seeding.

**Alternativas consideradas:**
- Añadir columnas o entidades para paginación: se descarta porque no existe necesidad funcional y excede el alcance.
- Generar una migración para un resultado de lectura: se descarta por ser un cambio no solicitado y por no alinearse con el modelo actual.

## Evidencia del repositorio

- El proyecto base ya usa `Program.cs`, `ISlice`, `ValidationFilterFactory` y `AppDbContext` con un modelo `Property` existente.
- La capa de infraestructura de endpoints y validación ya está preparada para extenderse con un nuevo slice del catálogo.
- Los assets públicos ya se sirven a través de rutas accesibles por runtime, por lo que la nueva feature puede construir URLs absolutas con el host de la solicitud sin duplicar infraestructura.
