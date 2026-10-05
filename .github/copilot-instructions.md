<!-- SPECKIT START -->

For additional context about technologies to be used, project structure,
shell commands, and other important information, read the current plan

<!-- SPECKIT END -->

# Instrucciones globales del proyecto

Este repositorio usa Spec-Driven Development como flujo obligatorio de trabajo.

Toda implementación DEBE respetar la constitución del proyecto ubicada en:

`.specify/memory/constitution.md`

La constitución tiene precedencia sobre cualquier preferencia personal, convención automática o sugerencia del agente.

## Idioma obligatorio

Todo archivo `.md` del repositorio DEBE estar escrito en español.

Esto incluye:

- `.github/`
- `.specify/`
- `specs/`
- `README.md`
- documentación técnica
- instrucciones
- prompts
- skills propios del proyecto

Está prohibido mezclar idiomas dentro de un mismo documento `.md`.

Los nombres técnicos, APIs, clases, comandos, namespaces, paquetes y nombres propios del ecosistema .NET pueden mantenerse en inglés cuando corresponda.

### Excepción: skills importadas de terceros

Las skills importadas de terceros o del ecosistema (por ejemplo las que provienen de paquetes externos, del Blazor SDK o de proveedores) PUEDEN mantenerse en inglés y NO están obligadas a traducirse.

Se consideran skills propias del proyecto las creadas y mantenidas dentro de este repositorio para su gobernanza (por ejemplo `spec-driven-workflow`, `minimal-api-slices`, `vertical-slice-handlers`, `blazor-app-css-design-system` y demás skills escritas originalmente en español). Estas DEBEN permanecer en español.

Una skill importada en inglés NO se considera mezcla de idiomas prohibida, siempre que cada documento `.md` mantenga un único idioma de forma interna.

## Spec-Driven Development

Las specs aprobadas ubicadas en `specs/` son la única fuente de verdad del proyecto.

Está prohibido implementar funcionalidad que no esté descrita en el `spec.md` vigente.

Cada iniciativa DEBE contener exactamente tres archivos:

- `spec.md`
- `plan.md`
- `tasks.md`

El flujo obligatorio mínimo es:

```text
speckit.specify → speckit.plan → speckit.tasks → speckit.implement
```

Para specs fundacionales o de alto impacto se recomienda:

```text
speckit.specify → speckit.clarify → speckit.plan → speckit.analyze → speckit.tasks → speckit.implement
```

Ninguna fase obligatoria puede saltarse.

## Validación de secuencia

Antes de ejecutar cualquier fase, validar que existan los artefactos previos.

Si se intenta ejecutar `speckit.plan` sin que exista `spec.md`, detenerse y mostrar:

```text
Ejecución detenida: antes de ejecutar speckit.plan debe crear el archivo spec.md mediante speckit.specify.
```

Si se intenta ejecutar `speckit.tasks` sin que exista `plan.md`, detenerse y mostrar:

```text
Ejecución detenida: antes de ejecutar speckit.tasks debe crear el archivo plan.md mediante speckit.plan.
```

Si se intenta ejecutar `speckit.implement` sin que exista `tasks.md`, detenerse y mostrar:

```text
Ejecución detenida: antes de ejecutar speckit.implement debe crear el archivo tasks.md mediante speckit.tasks.
```

Está prohibido crear archivos vacíos o simulados para saltar fases.

## Trazabilidad obligatoria

Todo código, archivo, template, migración, seeder, componente, endpoint o cambio de configuración DEBE estar trazado a una tarea específica en `tasks.md`.

Si un cambio no tiene tarea asociada en `tasks.md`, debe rechazarse.

Cada tarea completada DEBE marcarse como `[X]`.

## Orden de implementación

El orden de implementación lo define el prefijo numérico de cada spec.

La spec con número menor SIEMPRE se implementa antes que una spec con número mayor.

Ejemplo:

```text
specs/001-foundation/
specs/002-backend-contracts/
specs/003-frontend-ui/
```

La spec `001` debe implementarse antes que `002`, y `002` antes que `003`.

## Estructura de specs

Las specs viven en la raíz del repositorio:

```text
specs/
  001-nombre-de-la-spec/
    spec.md
    plan.md
    tasks.md
```

Está prohibido crear specs dentro de `.specify/specs/`.

La carpeta `.specify/` se reserva únicamente para infraestructura de spec-kit.

## Estructura de `.specify/`

La carpeta `.specify/` solo puede contener:

```text
.specify/
  memory/
  scripts/
  templates/
  extensions.yml
  feature.json
```

Está prohibido crear:

```text
.specify/specs/
```

Si esa carpeta existe, se considera defecto crítico de gobernanza.

## Stack obligatorio

El proyecto usa:

- .NET 11
- ASP.NET Core Minimal APIs.
- Blazor Web App con Razor Components.
- EF Core.
- PostgreSQL con Npgsql.
- Refit para comunicación tipada entre frontend y backend.
- CSS propio en `wwwroot/app.css`.

Está prohibido introducir frameworks, librerías o patrones que contradigan la constitución.

## Reglas de implementación

Antes de escribir código:

1. Leer la spec vigente.
2. Leer `plan.md`.
3. Leer `tasks.md`.
4. Identificar la tarea exacta que se va a implementar.
5. Confirmar que la tarea no esté marcada como `[X]`.
6. Implementar únicamente el alcance descrito.
7. Marcar la tarea como `[X]` solo cuando esté completada y verificada.

## Prohibiciones generales

Está prohibido:

- Implementar funcionalidades no descritas en `spec.md`.
- Saltar fases del flujo spec-driven.
- Crear archivos no trazados a `tasks.md`.
- Cambiar arquitectura sin actualizar `plan.md`.
- Modificar scripts upstream de spec-kit sin decisión humana explícita.
- Mover specs a rutas no canónicas.
- Crear soluciones paralelas.
- Agregar frameworks CSS.
- Usar controllers en backend.
- Configurar entidades EF Core inline en `OnModelCreating`.
- Crear migraciones sin tarea en `tasks.md`.
- Crear una migración por cada entidad cuando pertenecen a la misma spec.
