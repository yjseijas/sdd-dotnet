# Tareas: Solución base Realtor

**Entrada**: Documentos de diseño de `/specs/001-realtor-solution-foundation/`

**Prerrequisitos**: `plan.md` (obligatorio), `spec.md` (obligatorio)

**Verificación**: Esta iniciativa es de foundation; la verificación se centra en la estructura correcta de la solución, la alineación con global.json y la ausencia de lógica de negocio en la base.

## Fase 1: Preparación de la solución base

**Objetivo**: Preparar la estructura inicial de la solución y validar la versión de .NET.

- [ ] T001 Crear la carpeta `app/` y el archivo de solución `app/Realtor.sln`.
- [ ] T002 Confirmar que la versión de .NET se toma del archivo `global.json` existente y no se modifica desde esta iniciativa.
- [ ] T003 [P] Crear la estructura base para los proyectos backend y frontend dentro de la solución única.
- [ ] T004 [P] Definir la organización esperada de carpetas para `backend/src`, `backend/tests`, `frontend/src` y `frontend/test`.

**Punto de control**: La solución base existe y la versión de SDK está alineada con el repositorio.

---

## Fase 2: Base del backend

**Objetivo**: Crear la infraestructura mínima del backend siguiendo Minimal APIs.

- [ ] T005 Crear el proyecto `app/backend/src/RealtorApi/` como aplicación ASP.NET Core Minimal API.
- [ ] T006 Registrar la estructura mínima de configuración del backend, sin controllers ni dominio.
- [ ] T007 Configurar `Program.cs` solo con servicios, middleware y mapeo inicial de endpoints base.
- [ ] T008 Crear el proyecto de pruebas `app/backend/tests/RealtorApiTests/` con una base de test mínima y sin lógica funcional.

**Punto de control**: El backend queda preparado para crecer por features verticales, sin caer en controllers ni dominio inicial.

---

## Fase 3: Base del frontend

**Objetivo**: Crear la infraestructura mínima del frontend Blazor.

- [ ] T009 Crear el proyecto `app/frontend/src/RealtorWeb/` como Blazor Web App con Razor Components.
- [ ] T010 Registrar la estructura mínima de la aplicación frontend sin pantallas o flujos funcionales de negocio.
- [ ] T011 Crear el proyecto de pruebas `app/frontend/test/RealtorWeb/` para validar la base del frontend.
- [ ] T012 Configurar la entrada inicial de Blazor con una base limpia y sin estilos ni features funcionales añadidos.

**Punto de control**: El frontend queda listo para implementar pantallas y componentes posteriores, sin lógica de negocio en esta fase.

---

## Fase 4: Validación final de la foundation

**Objetivo**: Comprobar que la solución cumple los requisitos de la base sin introducir alcance no autorizado.

- [ ] T013 Verificar que no existen controllers ni entidades de dominio en la base de la solución.
- [ ] T014 Verificar que `Program.cs` solo contiene configuración base del arranque y no lógica de negocio.
- [ ] T015 Verificar que la estructura de carpetas corresponde a la solución única definida por la constitución.
- [ ] T016 Verificar que la solución y los proyectos pueden compilar con la versión de .NET establecida en `global.json`.
- [ ] T017 [P] Registrar y revisar la documentación de especificación, plan y tareas para confirmar que el alcance de foundation está completo.

**Punto de control**: La iniciativa cumple la definición de foundation y queda lista para que las siguientes specs añadan funcionalidad real.

## Orden de ejecución

- Preparación de la solución base (`T001` a `T004`) sin dependencias de negocio.
- Base del backend (`T005` a `T008`) con validación de estructura mínima.
- Base del frontend (`T009` a `T012`) con validación de configuración base.
- Validación final (`T013` a `T017`) para confirmar cumplimiento del alcance.

## Estrategia de implementación

1. Crear primero la solución y la versión base de .NET.
2. Generar backend con Minimal APIs y pruebas mínimas.
3. Generar frontend con Blazor y pruebas mínimas.
4. Validar el alcance para evitar deuda técnica o funcionalidades que no pertenezcan al foundation.
