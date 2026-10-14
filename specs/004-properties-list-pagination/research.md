# Investigación: Listado paginado de propiedades

**Fecha**: 2026-10-13  
**Especificación**: [spec.md](./spec.md)

## Decisiones

### Endpoint auto-descubrible y handler por caso de uso

- **Decisión**: Implementar `ListProperties` como `ISlice`, delegar el caso de uso a un handler `IHandler<TRequest, TResult>` y usar `AppDbContext` desde ese handler.
- **Fundamento**: La API ya descubre slices mediante `SliceRegistry`, registra handlers mediante `HandlerRegistry` y usa `AppDbContext` sin repositorios genéricos. La constitución y los skills de slices y handlers requieren este patrón.
- **Alternativas consideradas**: Registrar un endpoint directamente en `Program.cs`, crear un controller, agregar un repository o introducir MediatR. Se descartan porque duplicarían mecanismos existentes o contradicen la arquitectura.

### Validación de parámetros y ProblemDetails

- **Decisión**: Usar un request específico del slice y FluentValidation con el filtro central de Minimal APIs. Conectar el filtro al mapeo central de slices y resolver validators desde el scope de la petición.
- **Fundamento**: `Program` ya registra validators por assembly y `ValidationFilterFactory` ya genera `ValidationProblemDetails`; sin embargo, `SliceRegistry.MapSlices` actualmente mapea los slices directamente y no agrega ese filtro. La conexión central es necesaria para validar automáticamente el request.
- **Alternativas consideradas**: Validar manualmente dentro del endpoint, registrar un filtro individual o registrar el validator manualmente. Se descartan por duplicar la integración transversal aprobada.
- **Decisión sobre valores no enteros**: El request debe preservar entradas no numéricas para producir errores de validación detallados, en lugar de depender de que el model binder convierta silenciosamente o aborte la petición antes de que el validator pueda informar el campo.

### Proyección y orden de lectura

- **Decisión**: Ejecutar una consulta `AsNoTracking` con proyección explícita a un modelo de lectura, orden por `Title` ascendente y desempate por `Id`, además de contar el total y aplicar paginación.
- **Fundamento**: El `AppDbContext` ofrece el `DbSet<Property>` requerido y el skill de mapping prescribe proyección EF Core para lecturas. No se cambia el modelo persistente.
- **Alternativas consideradas**: Cargar entidades completas y mapearlas en memoria, paginar después de materializar todas las filas o introducir un repository. Se descartan por cargar datos innecesarios, invalidar la paginación eficiente o agregar una capa trivial.

### Construcción de URL pública de imagen

- **Decisión**: Extraer el nombre de archivo del valor persistido y construir la URL absoluta desde el esquema y host de la petición; publicar el mismo proveedor estático bajo el alias `/assets`, manteniendo `/images`.
- **Fundamento**: El seeder y el manifiesto vigentes persisten `/images/properties/{fileName}`; `Program.cs` sirve esos assets bajo `/images`. La especificación 004 exige devolver `/assets/properties/{fileName}` absoluto y la imagen debe ser servible.
- **Alternativas consideradas**: Devolver sin cambios el valor persistido, reemplazar las rutas de imagen ya existentes o almacenar hosts absolutos en la base de datos. Se descartan porque la primera incumple el nuevo contrato, la segunda altera la iniciativa de persistencia y la tercera acopla datos persistentes al host de despliegue.
- **Alcance del host**: Se usa `Request.Scheme` y `Request.Host` tal como llegan a ASP.NET Core. La publicación detrás de un proxy debe configurar sus encabezados reenviados confiables fuera de este caso de uso.

### Pruebas

- **Decisión**: Agregar únicamente pruebas unitarias xUnit para validación, proyección/mapping de estado y URL, y metadatos; comprobar la API completa manualmente con `RealtorApi.http`.
- **Fundamento**: La constitución restringe por defecto las pruebas automatizadas a unit tests. La especificación solicita expresamente solicitudes manuales en el archivo HTTP.
- **Alternativas consideradas**: Agregar `WebApplicationFactory` o pruebas de integración con una base de datos. Se descartan porque no están autorizadas por el alcance ni por la política de pruebas vigente.

## Decisiones cerradas en clarificación

- `pageSize` máximo: 100.
- Colección del sobre JSON: `items`, al mismo nivel que los metadatos.
- Estado: texto (`Available`, `Rented`, `Maintenance`).

## Referencias locales revisadas

- `.github/instructions/backend.instructions.md`
- `.github/skills/minimal-api-slices/SKILL.md`
- `.github/skills/minimal-api-validation/SKILL.md`
- `.github/skills/vertical-slice-handlers/SKILL.md`
- `.github/skills/vertical-slice-mapping/SKILL.md`
- `.github/skills/result-problem-details/SKILL.md`
- `app/backend/src/RealtorApi/Program.cs`
- `app/backend/src/RealtorApi/Infrastructure/Endpoints/SliceRegistry.cs`
- `app/backend/src/RealtorApi/Infrastructure/Validation/ValidationFilterFactory.cs`
- `app/backend/src/RealtorApi/Infrastructure/Handlers/HandlerRegistry.cs`
- `app/backend/src/RealtorApi/Infrastructure/Persistence/AppDbContext.cs`
- `app/backend/src/RealtorApi/Infrastructure/Persistence/DatabaseSeeder.cs`
- `support/seed-data/seed-manifest.json`
