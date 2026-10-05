---
applyTo: "app/backend/**/*.cs"
---

# Instrucciones de backend

Estas instrucciones aplican a todo trabajo backend del proyecto.

El backend DEBE implementarse con:

- .NET 11.
- ASP.NET Core Minimal APIs.
- Vertical Slice Architecture.
- EF Core.
- Npgsql.
- PostgreSQL.
- FluentValidation.
- ProblemDetails.
- logging estructurado mediante `ILogger`.
- operaciones asíncronas para I/O.
- propagación de `CancellationToken`.

La versión exacta del SDK DEBE estar fijada mediante `global.json` en la raíz del repositorio.

Los procedimientos especializados DEBEN seguir los skills oficiales del proyecto cuando corresponda.

---

## Relación con Spec-Driven Development

Todo cambio backend DEBE originarse en una spec aprobada.

Antes de implementar cualquier cambio backend, el agente DEBE:

1. Leer `spec.md`.
2. Leer `plan.md`.
3. Leer `tasks.md`.
4. Identificar la tarea específica.
5. Determinar qué instrucciones y skills corresponden.
6. Implementar únicamente el alcance definido.
7. Verificar la implementación.
8. Marcar la tarea como `[X]` únicamente después de completar y verificar el cambio.

Flujo obligatorio:

```text
spec.md
    ↓
plan.md
    ↓
tasks.md
    ↓
implementación
    ↓
verificación
    ↓
[X]
```

Está prohibido:

- implementar funcionalidad no descrita en `spec.md`
- implementar tareas inexistentes en `tasks.md`
- ampliar el alcance por iniciativa propia
- crear abstracciones anticipadas no requeridas
- realizar refactors grandes fuera del alcance de la spec
- marcar una tarea como `[X]` antes de verificarla

---

## Skills obligatorios

Antes de implementar una tarea backend, el agente DEBE determinar qué skill corresponde.

Según el tipo de cambio, usar uno o más de los siguientes:

```text
.github/skills/minimal-api-slices/SKILL.md
.github/skills/minimal-api-validation/SKILL.md
.github/skills/vertical-slice-handlers/SKILL.md
.github/skills/vertical-slice-mapping/SKILL.md
.github/skills/result-problem-details/SKILL.md
.github/skills/testing-minimal-apis/SKILL.md

.github/skills/domain-events/SKILL.md
.github/skills/module-public-api/SKILL.md
.github/skills/ef-core-entity-configuration/SKILL.md
.github/skills/ef-core-migrations/SKILL.md
.github/skills/database-migration-and-seeding/SKILL.md
```

`domain-events` y `module-public-api` son condicionales y solo deben utilizarse cuando la spec o el plan justifiquen su necesidad.

Las reglas detalladas de persistencia DEBEN seguir también:

```text
.github/instructions/database.instructions.md
```

---

## Arquitectura obligatoria

El backend DEBE organizarse mediante Vertical Slice Architecture.

La unidad principal de organización es:

```text
feature
+
caso de uso
```

Está prohibido organizar casos de uso mediante carpetas técnicas globales como:

```text
Controllers/
Services/
Managers/
Repositories/
DTOs/
Validators/
Helpers/
Commands/
Queries/
```

Ejemplo:

```text
Features/
  Properties/
    CreateProperty/
      CreateProperty.Endpoint.cs
      CreateProperty.Handler.cs
      CreateProperty.Mapping.cs
      CreateProperty.Validators.cs
```

Cada slice DEBE mantener juntos sus elementos relacionados.

La infraestructura transversal realmente compartida puede vivir fuera de las features, por ejemplo:

```text
Infrastructure/
  Endpoints/
  Validation/
  Handlers/
  Persistence/
```

La lógica compartida solo DEBE extraerse cuando exista reutilización real y comprobable.

---

## Convención de estructura de Vertical Slices

Cada caso de uso DEBE vivir en su propio directorio dentro de la feature correspondiente.

Preferir nombres como:

```text
CreateProperty
UpdateProperty
DeleteProperty
ChangePropertyStatus
GetPropertyById
ListProperties
```

Evitar nombres técnicos genéricos.

Como convención predeterminada, cada slice puede separar responsabilidades en:

```text
<Slice>.Endpoint.cs
<Slice>.Handler.cs
<Slice>.Mapping.cs
<Slice>.Validators.cs
```

No es obligatorio crear archivos vacíos. Solo crear los archivos que el caso de uso necesite.

---

## Minimal APIs e ISlice

Todos los endpoints DEBEN implementarse mediante ASP.NET Core Minimal APIs.

Está prohibido usar controllers.

Todos los endpoints DEBEN seguir el patrón `ISlice`.

Contrato:

```csharp
public interface ISlice
{
    void AddEndpoint(IEndpointRouteBuilder app);
}
```

Ejemplo:

```csharp
public sealed class CreatePropertyEndpoint : ISlice
{
    public void AddEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/properties", HandleAsync);
    }
}
```

Los endpoints DEBEN ser auto-descubiertos y mapeados mediante la infraestructura centralizada.

Está prohibido:

- registrar endpoints individuales en `Program.cs`
- crear registries manuales por feature
- mantener listas manuales de endpoints
- duplicar scanners de assemblies
- descubrir endpoints por nombre de clase

Agregar un nuevo endpoint NO DEBE requerir modificar `Program.cs`.

Las implementaciones de `ISlice` DEBEN ser stateless.

Las dependencias scoped de una petición NO DEBEN inyectarse en el constructor de un `ISlice` registrado como singleton. Deben resolverse en el handler de ruta o delegarse al handler del caso de uso.

El procedimiento detallado DEBE seguir:

```text
.github/skills/minimal-api-slices/SKILL.md
```

---

## Program.cs

`Program.cs` DEBE limitarse a:

- configurar servicios
- configurar middleware
- registrar infraestructura
- registrar módulos
- mapear endpoints mediante el mecanismo centralizado
- iniciar el flujo centralizado de migración
- iniciar la aplicación

Ejemplo conceptual:

```csharp
builder.Services.AddValidatorsFromAssembly(
    typeof(Program).Assembly,
    ServiceLifetime.Scoped);

builder.Services.RegisterSlices(typeof(Program).Assembly);
builder.Services.RegisterHandlers(typeof(Program).Assembly);

var app = builder.Build();

app.MapSliceEndpoints();

await app.MigrateAsync();

app.Run();
```

`Program.cs` NO DEBE contener:

- lógica de negocio
- lógica de validación
- consultas de negocio
- lógica de migraciones
- lógica de seed
- inserción manual de datos iniciales
- registros individuales de endpoints, validators o handlers

---

## Endpoints delgados

Los endpoints DEBEN ser delgados.

Responsabilidad principal:

```text
HTTP Request
    ↓
Binding
    ↓
Validación automática
    ↓
Handler
    ↓
Result
    ↓
HTTP Response
```

Cada endpoint DEBE:

- tener request explícito cuando reciba entrada
- tener response explícito cuando devuelva datos
- delegar la ejecución al handler correspondiente
- devolver códigos HTTP correctos
- devolver `ProblemDetails` en errores esperados
- registrar logs estructurados cuando corresponda
- estar trazado a una tarea en `tasks.md`
- respetar el contrato definido por la spec

Los endpoints NO DEBEN contener:

- reglas de negocio
- lógica compleja
- consultas complejas a la base de datos
- decisiones del dominio
- lógica reutilizable
- lógica de migración o seed
- validación manual cuando exista la validación automática aprobada

---

## Validación automática de Minimal APIs

Toda entrada que tenga un `IValidator<T>` registrado DEBE validarse automáticamente antes de ejecutar el handler.

El backend utiliza:

```text
FluentValidation
+
Assembly scanning
+
ValidationFilterFactory
+
IServiceProviderIsService
+
Endpoint Filters
```

La validación es convention-based.

Un endpoint con un request validable NO DEBE:

- invocar manualmente el validator
- inyectar `IValidator<T>` solo para validar la entrada
- registrar manualmente `ValidationFilter<T>`
- agregar manualmente `AddEndpointFilter<T>()`

Los validators DEBEN registrarse automáticamente mediante assembly scanning.

Ejemplo:

```csharp
builder.Services.AddValidatorsFromAssembly(
    typeof(Program).Assembly,
    ServiceLifetime.Scoped);
```

Está prohibido registrar validators individualmente en `Program.cs`.

`ValidationFilterFactory` DEBE detectar si existe `IValidator<T>` para un parámetro del handler.

Flujo:

```text
Endpoint
    ↓
ValidationFilterFactory
    ↓
¿Existe IValidator<T>?
   │
   ├── Sí → ValidateAsync
   │          │
   │          ├── válido → Handler
   │          └── inválido → HTTP 400 + ValidationProblemDetails
   │
   └── No → Pass-through → Handler
```

La ausencia de validator NO DEBE producir error.

La validación asíncrona DEBE propagar `CancellationToken`.

El procedimiento detallado DEBE seguir:

```text
.github/skills/minimal-api-validation/SKILL.md
```

---

## Handlers de casos de uso

Cada handler DEBE representar un único caso de uso.

Los handlers DEBEN:

- coordinar el caso de uso
- aplicar reglas de aplicación
- delegar invariantes al dominio cuando corresponda
- usar operaciones asíncronas para I/O
- aceptar y propagar `CancellationToken`
- devolver resultados explícitos
- utilizar `AppDbContext` directamente cuando corresponda
- mantener la lógica específica dentro del slice

Los handlers NO DEBEN requerir una interfaz individual por cada implementación.

Evitar:

```text
ICreatePropertyHandler
CreatePropertyHandler
```

Preferir un marker interface común:

```csharp
public interface IHandler;
```

Ejemplo:

```csharp
internal sealed class CreatePropertyHandler(
    AppDbContext context,
    ILogger<CreatePropertyHandler> logger)
    : IHandler
{
}
```

Los handlers DEBEN registrarse automáticamente mediante assembly scanning.

Está prohibido:

- registrar handlers individualmente en `Program.cs`
- mantener listas manuales de handlers
- crear scanners por feature
- introducir MediatR únicamente para conectar endpoint y handler
- crear command/query wrappers vacíos sin valor real

El procedimiento detallado DEBE seguir:

```text
.github/skills/vertical-slice-handlers/SKILL.md
```

---

## Lógica de negocio

La lógica de negocio NO debe residir en:

- componentes UI
- endpoints
- `Program.cs`
- `DbContext`
- configuraciones EF Core
- clases de infraestructura
- `MigrationExtensions`
- `DatabaseSeeder`
- archivos de mapping

La lógica debe vivir en:

```text
Dominio
```

o:

```text
Handler del caso de uso
```

según corresponda.

Como regla general:

- invariantes y reglas propias del dominio pertenecen al dominio
- coordinación del caso de uso pertenece al handler
- transporte HTTP pertenece al endpoint
- persistencia pertenece a EF Core y la infraestructura correspondiente

---

## Contratos

Las entidades de dominio o persistencia NO deben mezclarse con requests o responses.

Está prohibido devolver entidades EF Core directamente desde endpoints.

Usar contratos explícitos por caso de uso.

Ejemplos:

```text
CreatePropertyRequest
CreatePropertyResponse
UpdatePropertyRequest
PropertySummaryResponse
PropertyDetailsResponse
```

Los contratos DEBEN exponer únicamente la información necesaria.

Cuando un request o response pertenezca exclusivamente a un slice, DEBE permanecer dentro del slice.

Solo mover contratos a `Shared/` cuando exista reutilización real entre varios slices del mismo módulo.

Está prohibido crear carpetas globales de DTOs, requests o responses para contratos específicos de casos de uso.

---

## Estados del dominio

Los estados del dominio DEBEN modelarse mediante tipos explícitos.

Está prohibido utilizar strings mágicos para estados conocidos.

Para propiedades, usar un tipo explícito equivalente a:

```csharp
public enum PropertyStatus
{
    Available,
    Rented,
    Maintenance,
    Inactive
}
```

Las reglas y valores definitivos siempre deben respetar la spec vigente.

---

## Mapping explícito

Los mappings DEBEN ser explícitos.

No introducir librerías de mapping automático únicamente para evitar transformaciones simples.

Está prohibido introducir:

```text
AutoMapper
Mapster
Mapperly
```

salvo que la spec o `plan.md` justifiquen explícitamente su necesidad.

### Objeto individual

Para mappings individuales específicos de un slice, preferir C# Extension Blocks cuando mejoren claridad y agrupación.

Ejemplo:

```csharp
internal static class CreatePropertyMappings
{
    extension(CreatePropertyRequest request)
    {
        internal Property ToEntity()
        {
            return new Property(
                request.Name,
                request.Address);
        }
    }

    extension(Property property)
    {
        internal CreatePropertyResponse ToResponse()
        {
            return new CreatePropertyResponse(
                property.Id,
                property.Name,
                property.Status);
        }
    }
}
```

### Colección ya materializada

Cuando la colección ya está en memoria y el resultado completo debe materializarse, preferir:

```csharp
var response = properties
    .Select(x => x.ToSummaryResponse())
    .ToList();
```

### Query EF Core de lectura

Preferir proyección directa desde EF Core cuando no se necesite cargar la entidad completa.

```csharp
var response = await context.Properties
    .AsNoTracking()
    .Select(property => new PropertySummaryResponse(
        property.Id,
        property.Name,
        property.Status))
    .ToListAsync(cancellationToken);
```

### Streaming real

Usar:

```text
IEnumerable<T>
yield return
IAsyncEnumerable<T>
```

solo cuando el caso de uso requiera evaluación diferida, streaming o procesamiento incremental.

### Regla de selección

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

Los mappings NO DEBEN contener lógica de negocio, consultas a base de datos, validaciones, side effects ni persistencia.

El procedimiento detallado DEBE seguir:

```text
.github/skills/vertical-slice-mapping/SKILL.md
```

---

## Shared dentro de una feature o módulo

Cada feature o módulo puede tener un directorio `Shared/` únicamente para elementos reutilizados realmente por múltiples slices del mismo contexto.

Ejemplo:

```text
Features/
  Properties/
    CreateProperty/
    UpdateProperty/
    DeleteProperty/
    Shared/
      Errors/
      Requests/
      Responses/
      Routes/
      Mapping/
```

`Shared/` NO DEBE convertirse en un directorio general para código cuya ubicación no está clara.

Preferir duplicación pequeña y localizada antes que una abstracción prematura.

---

## Acceso a persistencia desde handlers

Los handlers pueden utilizar `AppDbContext` directamente.

No crear repositories para envolver operaciones simples de EF Core.

Ejemplo innecesario:

```text
PropertyRepository.GetByIdAsync()
PropertyRepository.AddAsync()
PropertyRepository.SaveAsync()
```

cuando solo delegan a `DbSet<T>`, LINQ y `SaveChangesAsync()`.

Preferir:

```text
Handler
    ↓
AppDbContext
    ↓
EF Core
```

Crear una abstracción de persistencia únicamente cuando exista una necesidad real y aprobada.

Está prohibido crear repositories genéricos.

---

## Result Pattern y ProblemDetails

Los errores esperados del negocio NO DEBEN utilizar excepciones como control de flujo normal.

Ejemplos:

```text
NotFound
Conflict
Validation
BusinessRule
Forbidden
```

Los handlers DEBEN devolver un resultado explícito equivalente a `Result<T>` cuando un caso de uso pueda terminar con un error esperado.

Flujo:

```text
Handler
    ↓
Result<T>
    │
    ├── Success → Response HTTP
    │
    └── Error → Mapping centralizado → ProblemDetails
```

Las excepciones deben reservarse para condiciones inesperadas o fallos técnicos excepcionales.

La conversión de errores a HTTP DEBE ser centralizada y consistente.

El procedimiento detallado DEBE seguir:

```text
.github/skills/result-problem-details/SKILL.md
```

---

## Logging estructurado

El backend DEBE utilizar `ILogger<T>`.

Preferir:

```csharp
logger.LogInformation(
    "Property {PropertyId} was created",
    property.Id);
```

Evitar interpolación cuando corresponde logging estructurado.

Está prohibido registrar:

- contraseñas
- tokens
- secretos
- connection strings
- credenciales

No agregar logs sin valor operacional.

---

## Operaciones asíncronas

Las operaciones de I/O DEBEN utilizar APIs asíncronas cuando estén disponibles.

Ejemplos:

```csharp
await context.SaveChangesAsync(cancellationToken);

await context.Properties
    .FirstOrDefaultAsync(
        x => x.Id == id,
        cancellationToken);
```

Los handlers y validators asíncronos DEBEN aceptar y propagar `CancellationToken`.

Evitar:

```text
.Result
.Wait()
.GetAwaiter().GetResult()
```

salvo necesidad excepcional justificada.

---

## Side effects mediante eventos

Usar eventos solo cuando un caso de uso produzca efectos secundarios que requieran desacoplamiento real.

Ejemplo:

```text
CreatePropertyHandler
    ↓
Guardar propiedad
    ↓
Persistencia exitosa
    ↓
PropertyCreatedEvent
    ↓
Event Handler(s)
```

No crear eventos por anticipación ni para sustituir llamadas directas simples.

Usar eventos únicamente cuando:

- exista un efecto secundario real
- exista más de un consumidor potencial o desacoplamiento justificado
- la spec o `plan.md` lo requieran

Si se requiere entrega confiable después de una transacción, la estrategia debe definirse explícitamente en spec y plan.

El procedimiento detallado DEBE seguir:

```text
.github/skills/domain-events/SKILL.md
```

---

## Comunicación entre módulos

Esta sección aplica solo cuando la aplicación posee módulos funcionales explícitos.

Un módulo NO DEBE acceder directamente a:

- entidades internas de otro módulo
- `DbContext` de otro módulo
- handlers internos de otro módulo
- infraestructura interna de otro módulo

La comunicación entre módulos DEBE realizarse mediante `PublicApi` o eventos.

No crear proyectos `PublicApi` especulativamente.

El procedimiento detallado DEBE seguir:

```text
.github/skills/module-public-api/SKILL.md
```

---

## Comunicación frontend-backend

La comunicación desde frontend hacia backend DEBE usar Refit con interfaces tipadas.

Las interfaces DEBEN:

- ser tipadas
- organizarse por módulo o capacidad
- registrarse mediante `IHttpClientFactory`
- resolverse mediante inyección de dependencias

La configuración HTTP DEBE centralizar:

- base address
- timeouts
- handlers
- autenticación cuando corresponda
- logging HTTP
- resiliencia

La resiliencia HTTP DEBE utilizar la configuración estándar del proyecto mediante `Microsoft.Extensions.Http.Resilience`.

Está prohibido usar `HttpClient` directamente para llamadas de API desde componentes o servicios de UI.

Está prohibido usar `RestService.For<T>()` fuera del registro aprobado con `IHttpClientFactory`.

---

## Persistencia

Todo cambio relacionado con:

- EF Core
- Npgsql
- PostgreSQL
- entidades persistentes
- `AppDbContext`
- `IEntityTypeConfiguration<T>`
- migraciones
- seeders
- inicialización de base de datos

DEBE seguir:

```text
.github/instructions/database.instructions.md
```

Cuando una tarea backend modifica persistencia, ambas instrucciones DEBEN cumplirse.

La aplicación DEBE ejecutar durante el arranque:

```csharp
await app.MigrateAsync();

app.Run();
```

La llamada a `MigrateAsync()` NO DEBE condicionarse a una comprobación previa de migraciones pendientes.

El seeder NO DEBE invocarse manualmente desde `Program.cs` ni desde `MigrationExtensions`.

---

## Testing

El testing DEBE seguir la constitución vigente y la spec activa.

Por defecto, solo se permiten unit tests.

Los unit tests deben enfocarse en:

- lógica de dominio
- handlers
- validators
- reglas de negocio
- comportamiento aislado del caso de uso

Está prohibido crear tests no solicitados por la spec vigente.

Cualquier tipo de test distinto a unit tests requiere justificación explícita en spec, plan y tasks, y debe ser permitido por la constitución.

---

## Flujo completo de un Vertical Slice

```text
Cliente
    ↓
Refit
    ↓
Minimal API
    ↓
ISlice
    ↓
Validación automática
    ↓
Handler
    ↓
Dominio
    ↓
AppDbContext
    ↓
EF Core
    ↓
PostgreSQL
    ↓
Result<T>
    ↓
ProblemDetails o Response
```

Cuando existen side effects aprobados:

```text
Handler
    ↓
Persistencia exitosa
    ↓
Evento
    ↓
Event Handler(s)
```

---

## Prohibiciones generales

Está prohibido:

- usar controllers
- implementar funcionalidad fuera de la spec
- implementar tareas inexistentes
- organizar casos de uso por capas técnicas globales
- colocar lógica de negocio en endpoints
- colocar lógica de negocio en `Program.cs`
- colocar lógica de negocio en `DbContext`
- devolver entidades EF Core desde endpoints
- usar strings mágicos para estados del dominio
- crear repositories genéricos
- crear services genéricos innecesarios
- introducir MediatR sin necesidad aprobada
- crear interfaces individuales de handlers sin necesidad real
- registrar endpoints, validators o handlers manualmente
- usar mapping automático sin justificación
- devolver errores inconsistentes en lugar de `ProblemDetails`
- usar excepciones para errores esperados del negocio
- exponer excepciones internas al cliente
- ignorar `CancellationToken` sin justificación
- bloquear operaciones asíncronas innecesariamente
- ejecutar manualmente el seeder
- condicionar `MigrateAsync()` a migraciones pendientes
- crear tests fuera del alcance autorizado
- realizar refactors grandes fuera de la tarea vigente
- marcar una tarea como `[X]` sin verificarla

---

## Checklist antes de finalizar una tarea backend

### Trazabilidad

- [ ] La spec activa existe.
- [ ] `spec.md` fue leído.
- [ ] `plan.md` fue leído.
- [ ] `tasks.md` fue leído.
- [ ] La tarea existe en `tasks.md`.
- [ ] La implementación corresponde al alcance aprobado.
- [ ] No se agregó funcionalidad fuera de la spec.

### Arquitectura

- [ ] El código respeta Vertical Slice Architecture.
- [ ] El caso de uso está organizado dentro de su feature.
- [ ] No se crearon controllers.
- [ ] No se crearon services o repositories genéricos.
- [ ] No se introdujeron abstracciones especulativas.
- [ ] La lógica de negocio está en dominio o handler.

### Endpoint y validación

- [ ] El endpoint implementa `ISlice`.
- [ ] El endpoint permanece delgado.
- [ ] Existe request explícito cuando corresponde.
- [ ] Existe response explícito cuando corresponde.
- [ ] No se devuelven entidades EF Core.
- [ ] La entrada se valida automáticamente cuando existe validator.
- [ ] Los status codes son correctos.
- [ ] Los errores usan `ProblemDetails` o `ValidationProblemDetails`.

### Handler

- [ ] El handler representa un único caso de uso.
- [ ] Implementa `IHandler`.
- [ ] Se registra automáticamente.
- [ ] Propaga `CancellationToken`.
- [ ] No existe una interfaz individual innecesaria.
- [ ] Usa `AppDbContext` directamente cuando corresponde.

### Mapping

- [ ] El mapping es explícito.
- [ ] La estrategia corresponde al escenario real.
- [ ] No contiene lógica de negocio.
- [ ] Permanece en el slice cuando es específico.
- [ ] Solo se movió a `Shared` cuando existe reutilización real.

### Calidad

- [ ] El logging es estructurado.
- [ ] Las operaciones de I/O son asíncronas.
- [ ] No se exponen datos sensibles.
- [ ] Los tests autorizados pasan.
- [ ] El proyecto compila.

### Persistencia, cuando corresponda

- [ ] Se siguió `database.instructions.md`.
- [ ] Se aplicaron los skills de persistencia correspondientes.
- [ ] La migración fue revisada.
- [ ] `Program.cs` llama `await app.MigrateAsync();`.
- [ ] El seeder no se invoca manualmente.

Solo después de completar las verificaciones correspondientes se puede marcar la tarea como `[X]`.
