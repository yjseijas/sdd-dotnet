# Tareas: Solución base Realtor

**Entrada**: Documentos de diseño de `/specs/001-realtor-solution-foundation/`

**Prerrequisitos**: `plan.md` (obligatorio), `spec.md` (obligatorio), `research.md`, `data-model.md` y `quickstart.md` cuando corresponden.

**Verificación**: Esta iniciativa prepara la base técnica del proyecto; la validación se centra en la estructura compartida, la alineación con `global.json`, la ausencia de lógica de negocio y la compilación de la solución.

## Fase 1: Preparación de la solución compartida

**Objetivo**: Confirmar la versión del SDK y dejar la estructura base de la solución única preparada para backend y frontend.

- [X] T001 Crear la estructura raíz de la solución en `app/` y asegurar la presencia de `app/Realtor.sln`.
- [X] T002 Confirmar que la versión de .NET se toma exclusivamente del archivo `global.json` y no se modifica desde esta iniciativa.
- [X] T003 [P] Crear la organización de carpetas de backend y frontend dentro de `app/` según la estructura definida en `plan.md`.
- [X] T004 [P] Registrar los proyectos base esperados en `app/backend/` y `app/frontend/` sin incorporar negocio ni features funcionales.

**Punto de control**: La solución compartida existe, la versión del SDK está sincronizada con `global.json` y la estructura base está lista para el desarrollo real.

---

## Fase 2: Historia de usuario 1: Preparar la base del repositorio para el equipo de desarrollo (Prioridad: P1) 🎯 MVP

**Objetivo**: Garantizar una sola solución común y una estructura inicial consistente sobre la que puedan crecer backend y frontend sin introducir lógica de negocio.

**Prueba independiente**: Verificar que la versión de .NET proviene de `global.json`, que la solución queda en `app/Realtor.sln` y que los proyectos principales existen bajo la estructura esperada.

### Verificación de la historia de usuario 1

- [X] T005 [P] [US1] Validar la versión del SDK definida en `global.json` y documentar la comprobación en `quickstart.md` o equivalente.
- [X] T006 [P] [US1] Revisar que la estructura de carpetas cumple la regla de solución única y la jerarquía indicada en `plan.md`.

### Implementación de la historia de usuario 1

- [X] T007 [US1] Crear `app/Realtor.sln` y ajustar la solución principal para incluir los proyectos base de backend y frontend.
- [X] T008 [US1] Crear la estructura de carpetas para `app/backend/src/RealtorApi/`, `app/backend/tests/RealtorApiTests/`, `app/frontend/src/RealtorWeb/` y `app/frontend/test/RealtorWeb/`.
- [X] T009 [US1] Verificar que no se introduce lógica de negocio, entidades de dominio ni casos de uso funcionales en esta iniciativa.

**Punto de control**: La base del repositorio queda preparada y no excede el alcance de la foundation.

---

## Fase 3: Historia de usuario 2: Crear la capa de backend inicial con ASP.NET Core Minimal APIs (Prioridad: P1)

**Objetivo**: Preparar el proyecto backend con la ruta mínima para futuras features verticales, sin controllers ni dominio.

**Prueba independiente**: Comprobar que `RealtorApi` existe como proyecto ASP.NET Core con Minimal APIs y que `Program.cs` limita la configuración base.

### Verificación de la historia de usuario 2

- [X] T010 [P] [US2] Verificar que `app/backend/src/RealtorApi/` queda configurado como proyecto ASP.NET Core y no usa controladores.
- [X] T011 [P] [US2] Comprobar que `Program.cs` contiene solo configuración de arranque mínima (servicios, middleware y mapeo base) sin lógica funcional.

### Implementación de la historia de usuario 2

- [X] T012 [US2] Crear el proyecto `app/backend/src/RealtorApi/RealtorApi.csproj` con la base del runtime ASP.NET Core Minimal API.
- [X] T013 [US2] Configurar `app/backend/src/RealtorApi/Program.cs` para servicios y middleware mínimos, sin endpoints funcionales ni controllers.
- [X] T014 [US2] Crear `app/backend/tests/RealtorApiTests/RealtorApiTests.csproj` con una base de pruebas mínima para validar el arranque del backend.
- [X] T015 [US2] Revisar la estructura del backend para confirmar que no existe dominio, entidades persistentes ni lógica de negocio en la base.

**Punto de control**: El backend queda preparado para crecer por vertical slices sin introducir deuda técnica inicial.

---

## Fase 4: Historia de usuario 3: Crear la capa de frontend inicial con Blazor Web App (Prioridad: P1)

**Objetivo**: Preparar la app web con Blazor Web App y Razor Components, dejando la estructura base para pantallas y navegación futuras sin funcionalidad real.

**Prueba independiente**: Verificar que el proyecto frontend usa Blazor Web App con Razor Components y que la base está preparada sin pantallas ni flujos de negocio.

### Verificación de la historia de usuario 3

- [X] T016 [P] [US3] Validar que `app/frontend/src/RealtorWeb/` es un proyecto Blazor Web App con Razor Components.
- [X] T017 [P] [US3] Comprobar que la base del frontend no incluye pantallas ni lógica funcional de negocio.

### Implementación de la historia de usuario 3

- [X] T018 [US3] Crear el proyecto `app/frontend/src/RealtorWeb/RealtorWeb.csproj` con la estructura base de Blazor Web App.
- [X] T019 [US3] Configurar la entrada de la aplicación en `app/frontend/src/RealtorWeb/Program.cs` y los componentes base de inicio (`App.razor`, `Routes.razor` y `Components/Pages/` si aplica) sin páginas funcionales.
- [X] T020 [US3] Crear `app/frontend/test/RealtorWeb/RealtorWeb.csproj` como proyecto de pruebas mínimo para la base del frontend.
- [X] T021 [US3] Revisar que la capa frontend se mantiene dentro del alcance de foundation y no define flujos de negocio ni features operativas.

**Punto de control**: El frontend queda listo para la evolución posterior con componentes y navegación, pero sin business logic en esta fase.

---

## Fase 5: Cierre y validación final

**Objetivo**: Validar el alcance completo de la foundation y confirmar que la solución cumple la constitución, la spec y el plan.

- [X] T022 [P] Revisar el cumplimiento de la constitución, la spec y el plan para confirmar que no se ha excedido el alcance de foundation.
- [X] T023 Validar que la solución puede compilar con la versión marcada en `global.json` ejecutando `dotnet build app/Realtor.sln`.
- [X] T024 Revisar la documentación base (`spec.md`, `plan.md`, `quickstart.md` y `research.md`) para confirmar que el alcance queda consistente y traducido al español.
- [X] T025 Verificar que la solución final cumple los requisitos de la foundation: una solución única, backend Minimal API, frontend Blazor y ausencia de lógica de negocio.

**Punto de control**: La iniciativa foundation está completa, validada y lista para que las siguientes specs definan funcionalidad real.

---

## Dependencias y orden de ejecución

### Dependencias entre fases

- **Fase 1**: sin dependencias; prepara la base compartida.
- **Fase 2 (HU1)**: depende de la preparatoria; valida la base del repositorio y la solución única.
- **Fase 3 (HU2)**: depende de la estructura base y de la solución compartida.
- **Fase 4 (HU3)**: depende de la estructura base y comparte la misma solución.
- **Fase 5**: depende de que las tres historias hayan quedado validadas.

### Dependencias entre historias

- **US1**: no tiene dependencias de negocio; es la base necesaria para US2 y US3.
- **US2**: depende de que la solución y la estructura de backend estén creadas.
- **US3**: depende de que la solución y la estructura de frontend estén creadas.

### Oportunidades de paralelismo

- T003 y T004 pueden ejecutarse en paralelo porque actúan sobre distintos segmentos del repositorio y no comparten archivos funcionales.
- T005 y T006 pueden ejecutarse en paralelo en la verificación de la historia US1.
- T010 y T011 pueden ejecutarse en paralelo en la verificación de US2.
- T016 y T017 pueden ejecutarse en paralelo en la verificación de US3.
- T022 y T023 pueden revisarse en paralelo durante la fase final, siempre que no se altere la compilación general de la solución.

## Estrategia de implementación

1. Completar primero la preparación de la estructura y la validación del SDK.
2. Implementar la historia US1 para dejar una sola solución con la estructura base.
3. Implementar la historia US2 del backend minimal API sin introducir negocio ni controllers.
4. Implementar la historia US3 del frontend Blazor sin crear pantallas funcionales.
5. Cerrar con una validación global de la solución y del alcance permitido por la constitución.
