# Plan de implementación: Infraestructura base del backend

**Rama**: `002-foundation-backend` | **Fecha**: 2026-10-10 | **Especificación**: [spec.md](./spec.md)

**Entrada**: Especificación de funcionalidad de `/specs/002-foundation-backend/spec.md`

## Resumen

Esta iniciativa establece la infraestructura transversal del backend `RealtorApi` sin añadir entidades de dominio ni casos de uso funcionales. El objetivo principal es que el sistema pueda descubrir y mapear endpoints por `ISlice`, registrar handlers por inspección de assembly, validar parámetros con FluentValidation de forma automática, convertir resultados esperados a `ProblemDetails`, registrar eventos con `ILogger` estructurado y exponer una sonda `/health` de infraestructura sin autenticación ni lógica de negocio.

## Contexto técnico

**Lenguaje/versión**: C# con .NET 11, siguiendo el SDK definido en `global.json` y la solución raíz `app/Realtor.sln`.

**Dependencias principales**: ASP.NET Core Minimal APIs, FluentValidation, `ILogger`, reflexión de assembly, `Microsoft.AspNetCore.Http` y `xUnit` para pruebas unitarias.

**Persistencia**: No aplica para esta iniciativa; la capa base no introduce modelos de dominio ni acceso a base de datos ni persistencia relacional.

**Pruebas**: Pruebas unitarias en `app/backend/tests/RealtorApiTests` con xUnit; se validarán los contratos de descubrimiento, validación, conversión de errores y salud de infraestructura.

**Plataforma objetivo**: Backend web del proyecto Realtor, ejecutado mediante `WebApplication` de ASP.NET Core y accesible vía HTTP local con una sonda `/health` sin capa de autenticación.

**Tipo de proyecto**: Servicio web con arquitectura de backend minimal API y pruebas unitarias de infraestructura.

**Objetivos de rendimiento**: Mantener la carga de arranque baja mediante reflexión de assembly y un registro centralizado, sin crear un registry manual ni duplicación de endpoints.

**Restricciones**: No se permiten controladores MVC, no se agrega lógica de negocio ni entidades de dominio, no se introduce persistencia en esta especificación y no se permiten listas manuales de endpoints o handlers.

**Escala/alcance**: Infraestructura transversal compartida para futuras features; el alcance se limita estrictamente a la preparación de la base del backend.

## Comprobación de la constitución

- [x] El backend propuesto usa ASP.NET Core Minimal APIs, sin controladores MVC, y organiza la funcionalidad verticalmente por caso de uso.
- [x] La interfaz web propuesta usa Blazor; la lógica de negocio del backend y los contratos entre frontend y backend tienen límites explícitos.
- [x] Cada capa, proyecto, abstracción y dependencia nueva responde a una necesidad del alcance y está justificada.
- [x] Los requisitos y criterios de aceptación son verificables; el plan identifica cómo comprobar los comportamientos afectados y documenta si se requieren pruebas automatizadas según la especificación.
- [x] Se identifican las validaciones de entrada, necesidades de seguridad y tratamiento de datos sensibles pertinentes.
- [x] Los artefactos de especificación, planificación y tareas se redactan en español, conservando los identificadores técnicos cuando sea necesario.
- [x] Las decisiones no fijadas por la constitución quedan documentadas aquí, sin asumir tecnologías, patrones o modos de ejecución no aprobados.

## Estructura del proyecto

### Documentación de esta funcionalidad

```text
specs/002-foundation-backend/
├── spec.md
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── README.md
└── tasks.md
```

### Código fuente (raíz del repositorio)

```text
app/
├── Realtor.sln
├── backend/
│   ├── src/
│   │   └── RealtorApi/
│   │       ├── Program.cs
│   │       ├── Infrastructure/
│   │       │   ├── Endpoints/
│   │       │   ├── Handlers/
│   │       │   ├── Validation/
│   │       │   ├── Results/
│   │       │   └── Health/
│   │       └── Features/
│   │           └── Shared/
│   └── tests/
│       └── RealtorApiTests/
└── frontend/
    └── src/
        └── RealtorWeb/
```

**Decisión de estructura**: La infraestructura se implementará dentro del proyecto `RealtorApi` como capas transversales compartidas y no como features de producto. Se separan `ISlice`, `IHandler`, `Result`/`ProblemDetails`, validación, logging y la sonda `/health` para que futuras features puedan reutilizar el arranque centralizado y evitar listas manuales. La arquitectura sigue el principio de solución única y la gobernanza de la constitución sin sobre-diseñar el backend.

## Registro de complejidad

| Decisión o complejidad | Necesidad que resuelve | Alternativa más simple descartada y motivo |
|-------------------------|------------------------|--------------------------------------------|
| Descubrimiento de endpoints por reflexión de assembly | Permite responder al requisito de registrar slices sin editar `Program.cs` | Crear un registro manual por endpoint; se descarta porque rompe el objetivo de auto-descubrimiento y aumenta el acoplamiento |
| Validación automática por tipo y filtro centralizado | Garantiza que las solicitudes se validen con `FluentValidation` sin duplicar lógica en cada endpoint | Validar dentro de cada endpoint; se descarta porque acopla cada slice a la validación y no escala |
| Conversión centralizada de `Result` a `ProblemDetails` | Mantiene un contrato de error consistente entre infraestructura y futuras features | Emplear `Exception` o mensajes sin formato; se descarta porque compromete la trazabilidad y la respuesta HTTP |
| Exponer `/health` sin autenticación y sin dependencia de base de datos | Cumple la especificación y evita requerir persistencia en la iniciativa base | Añadir health más complejo con verificación de DB o auth; se descarta porque excede el alcance y la política de seguridad de la sonda |
