# Skill: Flujo Spec-Driven Development

## Cuándo usar este skill

Usar este skill antes de ejecutar cualquier comando del flujo spec-kit o antes de crear, modificar o implementar una iniciativa dentro de `specs/`.

Este skill aplica a:

- `speckit.specify`
- `speckit.clarify`
- `speckit.plan`
- `speckit.analyze`
- `speckit.tasks`
- `speckit.implement`

## Regla principal

Ninguna fase puede ejecutarse fuera de orden. Si falta el artefacto requerido de la fase anterior, la ejecución DEBE detenerse inmediatamente.

La spec activa DEBE resolverse desde:

```text
.specify/feature.json
```

El valor de la feature activa DEBE apuntar a una carpeta dentro de:

```text
specs/<NNN>-<nombre>/
```

## Secuencia obligatoria mínima

```text
speckit.specify → speckit.plan → speckit.tasks → speckit.implement
```

Para specs fundacionales o de alto impacto se recomienda:

```text
speckit.specify → speckit.clarify → speckit.plan → speckit.analyze → speckit.tasks → speckit.implement
```

## Validaciones por comando

### `speckit.specify`

Debe crear:

```text
specs/<NNN>-<nombre>/spec.md
```

El archivo `spec.md` debe contener objetivo, alcance, criterios de aceptación y dependencias.

### `speckit.clarify`

Requiere:

```text
specs/<NNN>-<nombre>/spec.md
```

Si falta `spec.md`, detener ejecución con este mensaje:

```text
Ejecución detenida: antes de ejecutar speckit.clarify debe crear el archivo spec.md mediante speckit.specify.
```

### `speckit.plan`

Requiere:

```text
specs/<NNN>-<nombre>/spec.md
```

Si falta `spec.md`, detener ejecución con este mensaje:

```text
Ejecución detenida: antes de ejecutar speckit.plan debe crear el archivo spec.md mediante speckit.specify.
```

### `speckit.analyze`

Requiere:

```text
specs/<NNN>-<nombre>/spec.md
specs/<NNN>-<nombre>/plan.md
```

Si falta alguno, detener ejecución con este mensaje:

```text
Ejecución detenida: antes de ejecutar speckit.analyze debe crear el archivo plan.md mediante speckit.plan.
```

### `speckit.tasks`

Requiere:

```text
specs/<NNN>-<nombre>/spec.md
specs/<NNN>-<nombre>/plan.md
```

Si falta alguno, detener ejecución con este mensaje:

```text
Ejecución detenida: antes de ejecutar speckit.tasks debe crear el archivo plan.md mediante speckit.plan.
```

### `speckit.implement`

Requiere:

```text
specs/<NNN>-<nombre>/spec.md
specs/<NNN>-<nombre>/plan.md
specs/<NNN>-<nombre>/tasks.md
```

Si falta alguno, detener ejecución con este mensaje:

```text
Ejecución detenida: antes de ejecutar speckit.implement debe crear el archivo tasks.md mediante speckit.tasks.
```

## Reglas de trazabilidad

Todo cambio de código, template, migración, archivo de configuración o documentación técnica DEBE rastrear a una tarea en `tasks.md`.

Antes de implementar, validar:

1. La tarea existe en `tasks.md`.
2. La tarea pertenece a la spec activa.
3. La tarea no pertenece a una spec con prefijo numérico posterior a una spec pendiente.
4. La tarea tiene alcance suficiente para justificar el cambio.

Si el cambio no está cubierto por una tarea, detener ejecución y reportar:

```text
Ejecución detenida: el cambio solicitado no está trazado a ninguna tarea de tasks.md en la spec activa.
Actualice la spec y tasks.md antes de implementar.
```

## Reglas de finalización

Una tarea solo puede marcarse como `[X]` cuando:

1. El cambio fue implementado.
2. La implementación respeta la constitution.
3. La validación manual o automática correspondiente fue ejecutada.
4. No quedan archivos incompletos, temporales o divergentes.

Está prohibido marcar tareas como completadas por intención futura.

## Modo interactivo

Cuando un comando opere en modo interactivo, las preguntas DEBEN presentarse una por una y esperar respuesta antes de continuar.

### Pregunta con opciones

```text
Pregunta [N de TOTAL] — [tema corto]
─────────────────────────────────────
[Enunciado claro de la pregunta]

Por qué importa: [1 línea sobre el impacto de decidir mal]

A) [opción concreta con valor específico]
B) [opción concreta con valor específico]  ← Recomendado
C) [opción concreta con valor específico]
D) Otro — escribe tu respuesta

> Responde con la letra (A, B, C o D) o escribe tu respuesta libre.
```

### Pregunta Sí/No

```text
Pregunta [N de TOTAL] — [tema corto]
─────────────────────────────────────
[Enunciado de la pregunta]

Por qué importa: [1 línea]

S) Sí  ← Recomendado
N) No

> Responde S o N.
```

## Revisión obligatoria antes de continuar

Antes de ejecutar cualquier fase, revisar:

- La spec activa existe.
- La ruta de la spec activa está bajo `specs/`.
- No existe `.specify/specs/`.
- La fase solicitada respeta la secuencia.
- Los artefactos requeridos no están vacíos.
- La acción está permitida por la constitution.
