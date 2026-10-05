---

description: "Plantilla de tareas para implementar una funcionalidad"

---

# Tareas: [NOMBRE DE LA FUNCIONALIDAD]

**Entrada**: Documentos de diseño de `/specs/[###-nombre-de-funcionalidad]/`

**Prerrequisitos**: `plan.md` (obligatorio), `spec.md` (obligatorio si hay
historias de usuario), además de `research.md`, `data-model.md` y `contracts/`
cuando correspondan.

**Verificación**: Incluir tareas automatizadas cuando las requiera la
especificación o el plan. Para cada comportamiento, describir la verificación
aplicable, automatizada o manual, conforme al plan. No asumir TDD ni exigir que
una prueba falle antes de implementar, salvo que la especificación aprobada lo
requiera.

**Organización**: Agrupar las tareas por historia de usuario para facilitar la
implementación y verificación independiente de cada historia.

## Formato: `[ID] [P?] [Historia] Descripción`

- **[P]**: Puede ejecutarse en paralelo (archivos distintos y sin dependencias).
- **[Historia]**: Historia asociada (p. ej., HU1, HU2, HU3).
- Incluir rutas exactas y resultados verificables en las descripciones.

## Convenciones de rutas

- Usar las rutas reales de proyectos y archivos seleccionadas en `plan.md`.
- Para el backend, organizar las rutas por funcionalidad/caso de uso conforme a
  Minimal APIs y arquitectura vertical.
- Para el frontend, usar las rutas reales del proyecto Blazor.
- No presuponer nombres de proyectos, carpetas, lenguajes o herramientas que el
  plan no haya seleccionado.

<!--
  Las tareas siguientes son ejemplos. La orden /speckit.tasks debe sustituirlos
  con tareas derivadas de las historias y prioridades de spec.md, los requisitos
  y decisiones de plan.md, las entidades de data-model.md y los contratos.
  Cada historia debe poder implementarse y verificarse como incremento útil.
  No conservar estas tareas de ejemplo en el tasks.md generado.
-->

## Fase 1: Preparación (estructura compartida)

**Objetivo**: Preparar la estructura mínima acordada en el plan.

- [ ] T001 Crear la estructura de proyectos y carpetas definida en `plan.md`.
- [ ] T002 Configurar los proyectos y dependencias seleccionados.
- [ ] T003 [P] Configurar formato, análisis estático y herramientas requeridas.

---

## Fase 2: Base (prerrequisitos bloqueantes)

**Objetivo**: Implementar únicamente la infraestructura común que necesitan las
historias aprobadas. No iniciar una historia que dependa de esta fase mientras
sus prerrequisitos estén incompletos.

- [ ] T004 Implementar la configuración compartida requerida por el plan.
- [ ] T005 [P] Configurar autenticación y autorización si el alcance las requiere.
- [ ] T006 [P] Preparar el enrutamiento Minimal API y la integración del frontend
      definidos en el plan.
- [ ] T007 Preparar persistencia, manejo de errores o registro solo si se requiere.

**Punto de control**: La base está lista para las historias que dependen de ella.

---

## Fase 3: Historia de usuario 1: [Título] (Prioridad: P1) 🎯 MVP

**Objetivo**: [Valor que entrega esta historia]

**Prueba independiente**: [Cómo comprobar el resultado sin depender de otras
historias]

### Verificación de la historia de usuario 1

- [ ] T008 [P] [HU1] Verificar el contrato de [operación] según
      `contracts/` [ruta de prueba].
- [ ] T009 [P] [HU1] Verificar el recorrido de usuario [recorrido]
      [ruta de prueba o procedimiento].

### Implementación de la historia de usuario 1

- [ ] T010 [P] [HU1] Implementar [modelo o tipo] en [ruta del backend].
- [ ] T011 [HU1] Implementar el caso de uso [nombre] en [ruta vertical].
- [ ] T012 [HU1] Exponer el endpoint Minimal API necesario en [ruta].
- [ ] T013 [HU1] Implementar la interacción de interfaz Blazor en [ruta].
- [ ] T014 [HU1] Añadir validación, manejo de errores y medidas de seguridad
      requeridas para esta historia.

**Punto de control**: La historia funciona y cumple sus criterios de aceptación
de forma verificable.

---

## Fase 4: Historia de usuario 2: [Título] (Prioridad: P2)

**Objetivo**: [Valor que entrega esta historia]

**Prueba independiente**: [Cómo comprobarla sin depender de otras historias]

### Verificación de la historia de usuario 2

- [ ] T015 [P] [HU2] Verificar el contrato o comportamiento [descripción]
      [ruta o procedimiento].

### Implementación de la historia de usuario 2

- [ ] T016 [P] [HU2] Implementar [componente] en [ruta].
- [ ] T017 [HU2] Implementar el caso de uso y endpoint Minimal API en [ruta].
- [ ] T018 [HU2] Implementar o integrar la interfaz Blazor en [ruta].

**Punto de control**: Ambas historias cumplen sus criterios de aceptación.

---

## Fase 5: Historia de usuario 3: [Título] (Prioridad: P3)

**Objetivo**: [Valor que entrega esta historia]

**Prueba independiente**: [Cómo comprobarla sin depender de otras historias]

### Verificación de la historia de usuario 3

- [ ] T019 [P] [HU3] Verificar el comportamiento [descripción]
      [ruta o procedimiento].

### Implementación de la historia de usuario 3

- [ ] T020 [P] [HU3] Implementar [componente] en [ruta].
- [ ] T021 [HU3] Implementar el caso de uso y endpoint Minimal API en [ruta].
- [ ] T022 [HU3] Implementar o integrar la interfaz Blazor en [ruta].

**Punto de control**: Todas las historias seleccionadas cumplen sus criterios.

---

[Añadir fases para las historias necesarias, conservando prioridad, verificación
independiente, rutas reales y trazabilidad.]

## Fase N: Cierre y aspectos transversales

**Objetivo**: Completar tareas comunes a las historias implementadas.

- [ ] TXXX [P] Actualizar documentación en español afectada por el cambio.
- [ ] TXXX Verificar los criterios de aceptación y la conformidad constitucional.
- [ ] TXXX Revisar seguridad, validación y tratamiento de datos pertinentes.
- [ ] TXXX Ejecutar las verificaciones definidas en `plan.md` y registrar resultados.
- [ ] TXXX Validar `quickstart.md` si corresponde.

---

## Dependencias y orden de ejecución

### Dependencias entre fases

- **Preparación (Fase 1)**: Sin dependencias; puede comenzar inmediatamente.
- **Base (Fase 2)**: Depende de la preparación y bloquea solo las historias que
  requieran sus componentes.
- **Historias de usuario (Fases 3+)**: Respetan las dependencias documentadas;
  historias independientes pueden avanzar en paralelo.
- **Cierre**: Depende de las historias incluidas en el alcance.

### Dependencias entre historias

- **HU1 (P1)**: [Indicar prerrequisitos o ninguno].
- **HU2 (P2)**: [Indicar prerrequisitos; mantener independencia verificable].
- **HU3 (P3)**: [Indicar prerrequisitos; mantener independencia verificable].

### Dentro de cada historia

- Las tareas se trazan a requisitos y criterios de aceptación concretos.
- Completar las dependencias necesarias del caso de uso antes de integrarlo con
  su endpoint Minimal API y su interfaz Blazor.
- Ejecutar y registrar la verificación descrita en el plan.
- No imponer TDD; aplicarlo solo cuando lo requiera la especificación aprobada.

### Oportunidades de paralelismo

- Las tareas marcadas [P] no comparten archivos ni dependencias pendientes.
- Las historias independientes pueden desarrollarse en paralelo si hay capacidad.
- Verificar que el paralelismo no introduzca conflictos de archivos o contratos.

---

## Ejemplo de paralelismo: historia de usuario 1

```text
Tarea: Verificar contrato de [operación] en [ruta de prueba]
Tarea: Verificar recorrido de usuario [recorrido] en [ruta o procedimiento]

Tarea: Implementar [componente independiente] en [ruta]
Tarea: Implementar [otro componente independiente] en [ruta]
```

---

## Estrategia de implementación

### Primero el MVP (solo historia de usuario 1)

1. Completar la preparación y los prerrequisitos pertinentes.
2. Implementar HU1 según su alcance.
3. Verificarla independientemente contra sus criterios.
4. Presentar o desplegar el incremento si el flujo del proyecto lo permite.

### Entrega incremental

1. Completar la base necesaria para las historias.
2. Implementar y verificar HU1.
3. Añadir HU2 y verificarla sin romper HU1.
4. Añadir HU3 y verificarla sin romper los incrementos anteriores.

### Estrategia de equipo en paralelo

1. Acordar los contratos y completar juntos los prerrequisitos compartidos.
2. Asignar historias independientes en paralelo.
3. Integrar y comprobar cada historia contra los contratos y el plan.

## Notas

- [P] indica tareas independientes, no una prioridad.
- La etiqueta [HU] vincula la tarea con una historia para mantener trazabilidad.
- Las tareas deben ser concretas, verificables y usar rutas reales.
- No incluir dependencias entre historias que comprometan su verificación
  independiente sin declararlas en el plan.
- Registrar los resultados de verificación requeridos antes de dar por terminado
  el incremento.
