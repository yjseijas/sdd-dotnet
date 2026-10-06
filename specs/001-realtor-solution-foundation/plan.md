# Plan de implementación: Solución base Realtor

**Rama**: `001-realtor-solution-foundation` | **Fecha**: 2026-10-06 | **Especificación**: [spec.md](./spec.md)

**Entrada**: Especificación de funcionalidad de `/specs/001-realtor-solution-foundation/spec.md`

## Resumen

Esta iniciativa crea la base técnica de la solución Realtor con la estructura mínima necesaria para que el backend y el frontend puedan evolucionar siguiendo la arquitectura elegida por la constitución. La solución no implementa negocio ni features, sino que prepara la solución única, la capa backend con Minimal APIs, la capa frontend con Blazor Web App y la infraestructura base de pruebas.

## Contexto técnico

**Lenguaje/versión**: C# con .NET 11, determinado por el archivo `global.json` existente en la raíz del repositorio.

**Dependencias principales**: ASP.NET Core Minimal APIs, Blazor Web App, Razor Components, proyectos de test en .NET, configuración de arranque estándar del SDK.

**Persistencia**: No aplica para esta iniciativa, porque aún no se define el modelo de dominio ni la infraestructura de datos.

**Pruebas**: Proyectos de prueba unitarios para backend y frontend; se recomienda xUnit como estándar en .NET para mantener consistencia técnica.

**Plataforma objetivo**: Aplicación web full stack con backend y frontend dentro de la misma solución.

**Tipo de proyecto**: Solución full stack con un proyecto backend y un proyecto frontend.

**Objetivos de rendimiento**: No se definen objetivos funcionales de rendimiento porque esta iniciativa solo prepara la base y no implementa negocio.

**Restricciones**: No se permiten controllers, ni lógica de negocio, ni entidades de dominio, ni configuración de versión en el repositorio que contradiga `global.json`.

**Escala/alcance**: Foundation; estructura inicial y validación de la solución base.

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
specs/001-realtor-solution-foundation/
├── spec.md
├── plan.md
├── tasks.md
└── checklists/
    └── requirements.md
```

### Código fuente (raíz del repositorio)

```text
app/
├── Realtor.sln
├── backend/
│   ├── src/
│   │   └── RealtorApi/
│   └── tests/
│       └── RealtorApiTests/
└── frontend/
    ├── src/
    │   └── RealtorWeb/
    └── test/
        └── RealtorWeb/
```

**Decisión de estructura**: La solución se organiza como una única raíz `app/` con un archivo `.sln` compartido, un backend minimal API y un frontend Blazor. Esta estructura respeta la solución única de la constitución, evita separaciones paralelas y deja la base clara para que cada feature se desarrolle dentro de la misma arquitectura.

## Registro de complejidad

| Decisión o complejidad | Necesidad que resuelve | Alternativa más simple descartada y motivo |
|-------------------------|------------------------|--------------------------------------------|
| Mantener la estructura de `app/` con solución única | Garantiza una base compartida y evita proliferación de soluciones paralelas | Crear proyectos separados fuera de la solución central, porque contradice la constitución |
| Limitar Program.cs a configuración mínima | Evita introducir negocio en la fase base y mantiene el arranque limpio | Incluir endpoints funcionales o lógica de dominio en esta iniciativa, que rompe el alcance previsto |
| Tomar la versión .NET de `global.json` | Asegura alineación con el SDK requerido y evita inconsistencias entre proyectos | Modificar global.json manualmente, lo cual está prohibido por la iniciativa |
