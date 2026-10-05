# Plan de implementación: [FEATURE]

**Rama**: `[###-feature-name]` | **Fecha**: [DATE] | **Especificación**: [link]

**Entrada**: Especificación de funcionalidad de `/specs/[###-feature-name]/spec.md`

**Nota**: La orden `/speckit.plan` completa esta plantilla; su definición describe
el flujo de planificación.

## Resumen

[Extraer de la especificación: requisito principal y enfoque técnico basado en la investigación]

## Contexto técnico

<!--
  ACCIÓN REQUERIDA: Reemplazar esta sección con los detalles técnicos pertinentes
  a la funcionalidad. Los campos orientan el análisis; no implican decisiones
  predeterminadas cuando la especificación o el proyecto no las hayan fijado.
-->

**Lenguaje/versión**: [p. ej., C# y versión de .NET o REQUIERE ACLARACIÓN]

**Dependencias principales**: [p. ej., ASP.NET Core Minimal APIs, Blazor u otras]

**Persistencia**: [si corresponde, tecnología elegida o NO APLICA]

**Pruebas**: [estrategia y herramientas elegidas o REQUIERE ACLARACIÓN]

**Plataforma objetivo**: [plataforma o REQUIERE ACLARACIÓN]

**Tipo de proyecto**: [p. ej., servicio web, aplicación web full stack u otro]

**Objetivos de rendimiento**: [objetivos del dominio o NO DEFINIDOS]

**Restricciones**: [restricciones específicas de la funcionalidad o NINGUNA]

**Escala/alcance**: [dimensión relevante o NO DEFINIDO]

## Comprobación de la constitución

*Punto de control: debe aprobarse antes de la investigación de la Fase 0 y
revisarse nuevamente después del diseño de la Fase 1.*

- [ ] El backend propuesto usa ASP.NET Core Minimal APIs, sin controladores
  MVC, y organiza la funcionalidad verticalmente por caso de uso.
- [ ] La interfaz web propuesta usa Blazor; la lógica de negocio del backend y
  los contratos entre frontend y backend tienen límites explícitos.
- [ ] Cada capa, proyecto, abstracción y dependencia nueva responde a una
  necesidad del alcance y está justificada.
- [ ] Los requisitos y criterios de aceptación son verificables; el plan
  identifica cómo comprobar los comportamientos afectados y documenta si se
  requieren pruebas automatizadas según la especificación.
- [ ] Se identifican las validaciones de entrada, necesidades de seguridad y
  tratamiento de datos sensibles pertinentes.
- [ ] Los artefactos de especificación, planificación y tareas se redactan en
  español, conservando los identificadores técnicos cuando sea necesario.
- [ ] Las decisiones no fijadas por la constitución quedan documentadas aquí,
  sin asumir tecnologías, patrones o modos de ejecución no aprobados.

Si un punto no se cumple, revisar el diseño para ajustarlo. No se permiten
desviaciones de una regla constitucional por mera justificación en el plan;
una contradicción requiere una enmienda aprobada a la constitución.

## Estructura del proyecto

### Documentación de esta funcionalidad

```text
specs/[###-feature]/
├── spec.md              # Especificación de esta funcionalidad
├── plan.md              # Este plan
├── research.md          # Resultado de la investigación de Fase 0
├── data-model.md        # Resultado de diseño de Fase 1, si aplica
├── quickstart.md        # Guía de Fase 1, si aplica
├── contracts/           # Contratos de Fase 1, si aplica
└── tasks.md             # Tareas de Fase 2
```

### Código fuente (raíz del repositorio)

<!--
  ACCIÓN REQUERIDA: Sustituir el árbol de ejemplo con la estructura concreta
  elegida para la funcionalidad. El árbol no prescribe nombres de carpetas,
  proyectos o capas. Mostrar únicamente opciones seleccionadas y rutas reales.
-->

```text
# Documentar aquí los proyectos y archivos seleccionados para esta funcionalidad.
# Mantener el backend Minimal API organizado por casos de uso y el frontend en Blazor.
```

**Decisión de estructura**: [Documentar la estructura seleccionada y explicar
cómo se ajusta a los límites y principios de la constitución]

## Registro de complejidad

> Completar solo cuando la comprobación de la constitución detecte complejidad
> no evidente o una decisión que requiera justificación.

| Decisión o complejidad | Necesidad que resuelve | Alternativa más simple descartada y motivo |
|-------------------------|------------------------|--------------------------------------------|
| [Decisión]              | [Necesidad]            | [Motivo concreto]                          |
