# Tareas: Infraestructura base del backend

**Entrada**: Documentos de diseño de `/specs/002-foundation-backend/`

**Prerrequisitos**: `plan.md` y `spec.md` obligatorios; además `research.md`, `data-model.md` y `contracts/` disponibles para esta iniciativa.

**Verificación**: Esta iniciativa requiere pruebas unitarias para validación, registro de slices y conversiones de errores; las tareas incluyen la verificación automatizada que el plan exige.

## Formato: `[ID] [P?] [Historia] Descripción`

- [X] T001 ...

## Fase 1: Preparación (estructura compartida)

**Objetivo**: Preparar la estructura de infraestructura transversal del backend sin introducir entidades de dominio ni casos de uso de negocio.

- [X] T001 Crear la estructura base de carpetas y archivos para infraestructura transversal en `app/backend/src/RealtorApi/Infrastructure/`, `Features/` y `app/backend/tests/RealtorApiTests/`.
- [X] T002 Configurar `app/backend/src/RealtorApi/RealtorApi.csproj` con las referencias necesarias para ASP.NET Core Minimal APIs, FluentValidation y `ILogger`.
- [X] T003 [P] Configurar `app/backend/tests/RealtorApiTests/RealtorApiTests.csproj` con xUnit y referencias de prueba para la infraestructura del backend.

---

## Fase 2: Base (prerrequisitos bloqueantes)

**Objetivo**: Definir los contratos y la infraestructura compartida que habilitan el auto-descubrimiento, la validación y la representación uniforme de errores.

- [X] T004 Implementar el contrato `ISlice` y el mecanismo base para registrar endpoints en `app/backend/src/RealtorApi/Infrastructure/Endpoints/`.
- [X] T005 [P] Implementar los contratos `IHandler<TRequest, TResult>` y `Result<T>` en `app/backend/src/RealtorApi/Infrastructure/Handlers/` y `app/backend/src/RealtorApi/Infrastructure/Results/`.
- [X] T006 [P] Preparar la infraestructura de validación y logging estructurado en `app/backend/src/RealtorApi/Infrastructure/Validation/` y `app/backend/src/RealtorApi/Infrastructure/Logging/`.
- [X] T007 Definir el registro central de servicios y el punto de entrada de la infraestructura para `Program.cs` sin endpoints manuales ni lógica de negocio.

**Punto de control**: La base está lista para que las historias de usuario puedan depender de los contratos compartidos.

---

## Fase 3: Historia de usuario 1: Descubrimiento y mapeo centralizado de endpoints (Prioridad: P1) 🎯 MVP

**Objetivo**: Permitir que cada endpoint se publique mediante `ISlice` y que el arranque no requiera una lista manual.

**Prueba independiente**: Validar que una implementación de prueba de `ISlice` dentro del assembly de tests se descubre, no se duplica y puede mapear un endpoint; además, comprobar que `/health` responde `{"status":"healthy"}`.

### Verificación de la historia de usuario 1

- [X] T008 [P] [US1] Añadir pruebas unitarias para el descubrimiento de `ISlice` y la ausencia de duplicados en `app/backend/tests/RealtorApiTests/Infrastructure/SliceDiscoveryTests.cs`.
- [X] T009 [P] [US1] Añadir pruebas unitarias para la sonda `/health` en `app/backend/tests/RealtorApiTests/Infrastructure/HealthEndpointTests.cs`.

### Implementación de la historia de usuario 1

- [X] T010 [P] [US1] Implementar el registrador y descubridor de slices en `app/backend/src/RealtorApi/Infrastructure/Endpoints/SliceRegistry.cs` y archivos asociados.
- [X] T011 [US1] Implementar la sonda `/health` como slice en `app/backend/src/RealtorApi/Infrastructure/Health/HealthSlice.cs`.
- [X] T012 [US1] Ajustar `app/backend/src/RealtorApi/Program.cs` para registrar la infraestructura y mapear automáticamente los endpoints descubiertos sin endpoints manuales.

**Punto de control**: El sistema puede incorporar un endpoint nuevo sin modificar el arranque central ni mantener una lista estática.

---

## Fase 4: Historia de usuario 2: Validación automática y handlers (Prioridad: P1)

**Objetivo**: Registrar handlers por assembly y ejecutar validación automática cuando exista un validador para la solicitud.

**Prueba independiente**: Ejecutar pruebas unitarias que validen el registro automático de `IHandler`, la omisión de validación cuando no existe validador y la respuesta HTTP 400 con `ValidationProblemDetails` cuando hay errores.

### Verificación de la historia de usuario 2

- [X] T013 [P] [US2] Añadir pruebas unitarias para auto-registro de handlers y validación automática en `app/backend/tests/RealtorApiTests/Infrastructure/HandlerAndValidationTests.cs`.
- [X] T014 [P] [US2] Añadir pruebas unitarias para el caso sin validator y el manejo de errores `ValidationProblemDetails` en `app/backend/tests/RealtorApiTests/Infrastructure/ValidationFilterTests.cs`.

### Implementación de la historia de usuario 2

- [X] T015 [P] [US2] Implementar el registro automático de handlers concretos en `app/backend/src/RealtorApi/Infrastructure/Handlers/HandlerRegistry.cs`.
- [X] T016 [US2] Implementar el filtro de validación por tipo con `ValidationFilterFactory` o equivalente en `app/backend/src/RealtorApi/Infrastructure/Validation/ValidationFilter.cs`.
- [X] T017 [US2] Configurar el descubrimiento automático de validators y la respuesta HTTP 400 con `ValidationProblemDetails` en `app/backend/src/RealtorApi/Infrastructure/Validation/`.

**Punto de control**: Las solicitudes con validator aplicable se rechazan correctamente; las que no tienen validator continúan su flujo normal.

---

## Fase 5: Historia de usuario 3: Errores unificados y logging estructurado (Prioridad: P2)

**Objetivo**: Unificar la representación de errores esperados y registrar eventos con propiedades estructuradas sin depender de texto interpolado.

**Prueba independiente**: Validar la conversión de `Result` a `ProblemDetails` y comprobar que el logging usa `ILogger` con propiedades estructuradas.

### Verificación de la historia de usuario 3

- [X] T018 [P] [US3] Añadir pruebas unitarias para la conversión de `Result` a `ProblemDetails` y el logging estructurado en `app/backend/tests/RealtorApiTests/Infrastructure/ResultAndLoggingTests.cs`.

### Implementación de la historia de usuario 3

- [X] T019 [US3] Implementar la conversión centralizada de errores esperados a `ProblemDetails` en `app/backend/src/RealtorApi/Infrastructure/Results/ResultProblemDetailsMapper.cs`.
- [X] T020 [US3] Implementar el helper de logging estructurado en `app/backend/src/RealtorApi/Infrastructure/Logging/StructuredLoggerExtensions.cs`.
- [X] T021 [US3] Asegurar que los componentes lógicos de infraestructura usen `ILogger` con propiedades estructuradas y no interpolación textual como único mecanismo de diagnóstico.

**Punto de control**: Los errores esperados mantienen su código y detalle HTTP, y la infraestructura ofrece logging diagnósticos estructurados.

---

## Fase 6: Cierre y aspectos transversales

**Objetivo**: Validar que la iniciativa cumple la especificación, la gobernanza y la verificación de la solución.

- [X] T022 [P] Revisar la documentación de `specs/002-foundation-backend/` para confirmar que el alcance sigue limitado a infraestructura y no incluye entidades ni casos de uso de negocio.
- [X] T023 Ejecutar `dotnet build app/Realtor.sln` para verificar compilación con el SDK de `global.json`.
- [X] T024 Ejecutar `dotnet test app/backend/tests/RealtorApiTests/RealtorApiTests.csproj` para confirmar que la infraestructura de endpoints, validación y errores cumple las pruebas unitarias.
- [X] T025 Verificar la conformidad constitucional: sin controllers, sin `AppDbContext`, sin migraciones, sin registros manuales de endpoints y con `/health` sin autenticación de aplicación.

---

## Dependencias y orden de ejecución

### Dependencias entre fases

- **Preparación (Fase 1)**: sin dependencias; puede comenzar inmediatamente.
- **Base (Fase 2)**: depende de la preparación y bloquea la infraestructura base del backend.
- **Historia de usuario 1 (Fase 3)**: depende de la base y de la definición de `ISlice`.
- **Historia de usuario 2 (Fase 4)**: depende de la base y del registro de handlers/validación.
- **Historia de usuario 3 (Fase 5)**: depende de la base y de los contratos de resultados y logging.
- **Cierre (Fase 6)**: depende de todas las historias incluidas en el alcance.

### Dependencias entre historias

- **US1 (P1)**: no tiene dependencias funcionales sobre otras historias; es la base de la infraestructura de endpoints.
- **US2 (P1)**: depende del descubrimiento compartido y de los contratos de validación definidos en la base.
- **US3 (P2)**: depende de los contratos de `Result` y del registro legible con `ILogger`.

### Oportunidades de paralelismo

- Las tareas marcadas con `[P]` operan sobre archivos distintos y no comparten dependencias de implementación.
- Las pruebas unitarias de cada historia pueden escribirse en paralelo con la implementación de sus componentes asociados, siempre y cuando se respete la dependencia del contrato base.
- La validación y la preparación de la infraestructura pueden avanzar en paralelo sobre diferentes carpetas dentro de `Infrastructure/` y `tests/`.

---

## Estrategia de implementación

### Primero el MVP (solo historia de usuario 1)

1. Completar la preparación y la base de contratos.
2. Implementar el descubrimiento de slices y la sonda `/health`.
3. Validar con pruebas de `ISlice` y `/health`.
4. Confirmar que el backend responde sin listas manuales ni autenticación.

### Entrega incremental

1. Añadir la validación automática y el auto-registro de handlers.
2. Unificar errores esperados y logging estructurado.
3. Ejecutar build y pruebas para cerrar la iniciativa con evidencia de conformidad.
4. Revisar la alcance para que no se introduzcan entidades, controllers ni persistencia no aprobada.
