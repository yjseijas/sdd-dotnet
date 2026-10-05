---
name: testing-minimal-apis
description: Pruebas para Minimal APIs por vertical slice usando xUnit y FluentAssertions; pruebas de integración con WebApplicationFactory y Testcontainers solo cuando la spec y tasks lo aprueban explícitamente.
---

# Skill: Testing Minimal APIs

## Cuándo usar este skill

Usar este skill cuando una tarea de `tasks.md` requiera:

- crear pruebas unitarias para handlers o reglas de caso de uso
- validar contratos HTTP de endpoints Minimal API
- validar `ProblemDetails` o `ValidationProblemDetails`
- agregar cobertura de regresión para un bug corregido
- ejecutar pruebas con `dotnet test`

Este skill NO autoriza por sí mismo la creación de pruebas.

Toda prueba DEBE estar respaldada por:

```text
spec.md
plan.md
tasks.md
```

y por una tarea explícita en `tasks.md`.

## Relación con reglas del proyecto

Antes de escribir o modificar pruebas:

1. leer `spec.md`, `plan.md` y `tasks.md`
2. validar trazabilidad de cada escenario
3. leer instrucciones activas del repo

Rutas válidas de referencia:

```text
.github/instructions/backend.instructions.md
.github/instructions/database.instructions.md
.github/copilot-instructions.md
.specify/memory/constitution.md
```

Skills relacionados:

```text
.github/skills/minimal-api-slices/SKILL.md
.github/skills/minimal-api-validation/SKILL.md
.github/skills/result-problem-details/SKILL.md
.github/skills/vertical-slice-handlers/SKILL.md
.github/skills/vertical-slice-mapping/SKILL.md
.github/skills/dotnet-webapi/SKILL.md
.github/skills/run-tests/SKILL.md
```

## Regla constitucional de tipos de prueba

Por defecto se permiten unit tests.

Las pruebas de integración (incluyendo `WebApplicationFactory` y Testcontainers con PostgreSQL) SOLO se permiten cuando:

- la spec aprobada lo justifica explícitamente
- el plan lo detalla
- existe tarea explícita en `tasks.md`

Si esa justificación no existe, no crear pruebas de integración.

## Estrategia recomendada

```text
Regla de dominio
    ↓
Prueba unitaria

Handler / caso de uso
    ↓
Prueba unitaria

Endpoint + pipeline HTTP real
    ↓
Prueba de integración (solo si está aprobada)

Persistencia EF Core + PostgreSQL real
    ↓
Testcontainers (solo si está aprobada)
```

## Organización recomendada

Ubicación sugerida:

```text
app/backend/tests/RealtorApi.Tests/
```

Organizar por feature y caso de uso:

```text
app/backend/tests/RealtorApi.Tests/
  Features/
    Properties/
      CreateProperty/
        CreatePropertyHandlerTests.cs
        CreatePropertyEndpointTests.cs
  Infrastructure/
    Testing/
      RealtorApiFactory.cs
      PostgreSqlContainerFixture.cs
      TestDataFactory.cs
```

Evitar separar por carpetas técnicas globales (`UnitTests/`, `IntegrationTests/`, etc.) cuando rompa la cohesión del slice.

## Pruebas unitarias

Las pruebas unitarias de handlers deben:

- ejecutarse sin levantar el host ASP.NET Core
- verificar comportamiento observable
- usar Arrange-Act-Assert
- propagar `CancellationToken` cuando aplique
- evitar acoplarse a detalles internos

Evitar:

- probar métodos privados
- validar orden interno irrelevante
- mockear `DbSet<T>` o el proveedor LINQ de EF Core

## Pruebas de integración (condicionales)

Si fueron aprobadas por spec/plan/tasks, deben:

- ejecutar pipeline real con `WebApplicationFactory<Program>`
- usar `HttpClient` creado por la factory
- enviar requests HTTP reales
- validar status code, content-type y contrato de respuesta
- validar `ProblemDetails` o `ValidationProblemDetails` cuando corresponda

No llamar directamente al método del endpoint en pruebas de integración.

## Persistencia real con PostgreSQL (condicional)

Cuando la tarea exija persistencia real:

- usar PostgreSQL en Testcontainers
- aislar base de datos de desarrollo
- aplicar migraciones con `Database.MigrateAsync()`
- seguir flujo real de inicialización y seeding del proyecto
- mantener independencia entre pruebas

No usar SQLite como sustituto cuando el comportamiento dependa de PostgreSQL.

## Contratos HTTP mínimos a verificar

Por cada endpoint cubierto, verificar al menos:

- status codes definidos por spec
- response contract (DTO esperado)
- errores contractuales (`ProblemDetails` / `ValidationProblemDetails`)
- persistencia real cuando el caso lo requiera

No inventar respuestas HTTP fuera de spec/plan.

## Ejecución de pruebas

Para ejecución y filtros de `dotnet test`, usar el skill:

```text
.github/skills/run-tests/SKILL.md
```

Comandos base:

```powershell
dotnet test
```

```powershell
dotnet test app/backend/tests/RealtorApi.Tests
```

Una tarea de testing no se considera completada si no se ejecutaron y validaron las pruebas nuevas.

## Prohibiciones

Está prohibido:

- crear pruebas sin tarea en `tasks.md`
- crear pruebas para comportamiento no descrito por spec
- compartir estado entre pruebas
- depender del orden de ejecución
- usar base local de desarrollo
- deshabilitar pruebas para hacer pasar el pipeline
- cambiar assertions para aceptar implementación incorrecta

## Checklist

- [ ] La prueba tiene trazabilidad en `tasks.md`.
- [ ] El escenario está definido por spec/plan.
- [ ] Se priorizó unit test cuando era suficiente.
- [ ] Si hay integración, está explícitamente aprobada por spec/plan/tasks.
- [ ] Se verifican contratos HTTP y `ProblemDetails` cuando aplica.
- [ ] La persistencia usa PostgreSQL real cuando la tarea lo exige.
- [ ] Se ejecutaron pruebas focalizadas y conjunto relevante.
- [ ] El proyecto compila y las pruebas pasan.
