# Especificación de funcionalidad: Solución base Realtor

**Rama de funcionalidad**: `001-realtor-solution-foundation`

**Creada**: 2026-10-06

**Estado**: Borrador

**Entrada**: Descripción del usuario: "Crear base de la solucion Realtor".

## Escenarios de usuario y pruebas

### Historia de usuario 1: Preparar la base del repositorio para el equipo de desarrollo (Prioridad: P1)

El equipo necesita abrir una sola solución con la estructura inicial del proyecto, sin introducir lógica de negocio ni features operativas. La base debe permitir empezar a trabajar en backend y frontend con una separación clara de responsabilidades y la versión correcta de .NET aplicada desde la configuración existente.

**Motivo de prioridad**: Es la base sobre la que se construyen todas las iniciativas futuras del sistema; sin ella, no es posible iniciar el desarrollo del backend ni del frontend con una estructura consistente.

**Prueba independiente**: Verificar que la solución y los proyectos principales existen bajo la estructura esperada y que la configuración de runtime se toma del archivo global.json sin cambios manuales.

**Escenarios de aceptación**:

1. **Dado** que el repositorio contiene la configuración global de .NET, **Cuando** se prepara la solución base, **Entonces** la versión del SDK debe obtenerse exactamente del archivo global.json y no debe modificarse desde esta iniciativa.
2. **Dado** que la solución base necesita organizar el trabajo, **Cuando** se crea la estructura de proyectos, **Entonces** deben existir la solución principal y los proyectos backend/frontend de forma consistente con la arquitectura declarada.
3. **Dado** que la iniciativa es de foundation, **Cuando** se genera la base, **Entonces** no debe incluirse lógica de negocio ni entidades de dominio ni casos de uso funcionales.

---

### Historia de usuario 2: Crear la capa de backend inicial con ASP.NET Core Minimal APIs (Prioridad: P1)

El backend debe quedar preparado para crecer por casos de uso verticales sin introducir controladores ni lógica de dominio. La estructura debe ser mínima, declarativa y preparada para recibir funcionalidades futuras.

**Motivo de prioridad**: El backend es la base operativa del sistema y debe seguir la arquitectura definida por la constitución para evitar deuda técnica desde el inicio.

**Prueba independiente**: Comprobar que el proyecto backend se crea como aplicación ASP.NET Core con endpoints mínimos y sin controllers.

**Escenarios de aceptación**:

1. **Dado** que el backend debe usar Minimal APIs, **Cuando** se inicializa el proyecto, **Entonces** debe configurarse sin controllers y con la organización apropiada para futuras features.
2. **Dado** que Program.cs representa la configuración base del arranque, **Cuando** se define la aplicación, **Entonces** solo debe contener servicios, middleware y mapeo inicial de endpoints mínimos.
3. **Dado** que la iniciativa es base, **Cuando** se crea el proyecto backend, **Entonces** no debe implementarse lógica funcional ni entidades persistentes.

---

### Historia de usuario 3: Crear la capa de frontend inicial con Blazor Web App (Prioridad: P1)

El frontend debe preparar la estructura web para la aplicación Realtor con una base visual y técnica consistente, enfocada en Razor Components y en una composición modular simple sin features de negocio.

**Motivo de prioridad**: La aplicación necesita una capa de interfaz inicial que permita integrar el sistema completo con un enfoque moderno y compatible con la arquitectura del proyecto.

**Prueba independiente**: Verificar que el proyecto frontend usa Blazor Web App con Razor Components y que la estructura base queda preparada para su evolución posterior.

**Escenarios de aceptación**:

1. **Dado** que la interfaz debe implementarse con Blazor, **Cuando** se crea el proyecto, **Entonces** debe utilizar Blazor Web App con Razor Components.
2. **Dado** que el frontend necesita una base para futuras pantallas, **Cuando** se genera la estructura, **Entonces** debe dejarse preparada para componentes y navegación sin incorporar funcionalidades de negocio aún.
3. **Dado** que la iniciativa es de foundation, **Cuando** se crea el proyecto frontend, **Entonces** no deben definirse pantallas ni flujos funcionales de negocio.

---

### Casos límite

- ¿Qué ocurre cuando la versión de .NET no está definida en global.json? La iniciativa queda bloqueada y no puede continuar, porque la configuración global es la fuente de verdad obligatoria.
- ¿Qué ocurre si se intenta introducir business logic o controllers en esta fase? La iniciativa queda fuera del alcance del foundation y no debe aprobarse.
- ¿Qué ocurre si se crea una estructura que no respeta la solución única? La base no cumple la constitución y debe corregirse antes de avanzar.

## Requisitos

### Requisitos funcionales

- **RF-001**: El repositorio DEBE contener la solución principal en `app/Realtor.sln`.
- **RF-002**: El backend DEBE crear un proyecto en `app/backend/src/RealtorApi/` usando ASP.NET Core Minimal APIs.
- **RF-003**: El backend NO DEBE incluir controllers ni lógica de negocio en esta iniciativa.
- **RF-004**: El proyecto de pruebas backend DEBE ubicarse en `app/backend/tests/RealtorApiTests/`.
- **RF-005**: El frontend DEBE crear un proyecto en `app/frontend/src/RealtorWeb/` usando Blazor Web App con Razor Components.
- **RF-006**: El proyecto de pruebas frontend DEBE ubicarse en `app/frontend/test/RealtorWeb/`.
- **RF-007**: La configuración de arranque del backend y frontend DEBE limitarse a la base del sistema: servicios, middleware y mapeo inicial de endpoints o navegación sin funcionalidad de negocio.
- **RF-008**: Esta iniciativa DEBE evitar cualquier entidad de dominio, caso de uso, feature operativa o lógica funcional.
- **RF-009**: La versión de .NET DEBE derivarse exclusivamente del archivo `global.json` existente y NO DEBE modificarse durante esta iniciativa.
- **RF-010**: La solución DEBE seguir el principio de solución única y el modelo de arquitectura definido por la constitución.

### Entidades principales

No aplica para esta iniciativa, porque la fase foundation no define dominio ni persistencia.

## Criterios de éxito

### Resultados medibles

- **CE-001**: La estructura de proyectos y solución queda organizada en la ruta base esperada y cumple con la arquitectura definida por la constitución.
- **CE-002**: El backend y el frontend pueden avanzar con una base de arranque estable sin introducir lógica de negocio en esta iniciativa.
- **CE-003**: La solución compila o queda preparada para compilar con la versión de .NET declarada en global.json, sin cambios manuales al SDK.
- **CE-004**: La iniciativa evita controllers, entidades de dominio, features y cambios de configuración no autorizados.

## Supuestos

- El proyecto se inicia como una solución fundamentada, sin lógica de negocio ni dominio aún definido.
- La versión de .NET se toma del archivo global.json ya existente en la raíz del repositorio.
- La estructura de carpetas se diseña para futuras features verticales en backend y componentes Razor en frontend.
- Los equipos de desarrollo usarán esta base para crear fases posteriores con requisitos funcionales específicos.
