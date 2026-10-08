# Investigación: base de la solución Realtor

## Decisiones

- **Decisión 1**: Mantener una sola solución compartida en `app/Realtor.sln`.
  - **Racional**: la constitución exige una solución única y compartida para frontend, backend, dominio y persistencia.
  - **Alternativas consideradas**: soluciones separadas por capa o repositorios paralelos; se descartan porque rompen la arquitectura y la gobernanza del proyecto.

- **Decisión 2**: Tomar la versión del SDK desde `global.json` y no tocarla en esta iniciativa.
  - **Racional**: la precondición del flujo exige usar `global.json` como fuente de verdad.
  - **Alternativas consideradas**: instalar un SDK distinto o editar `global.json`; se descarta porque la iniciativa especifica explícitamente no modificarla.

- **Decisión 3**: Crear un backend ASP.NET Core Minimal API y un frontend Blazor Web App dentro de la misma solución.
  - **Racional**: la constitución define el stack obligatorio para la app y exige el uso de Minimal APIs y Blazor.
  - **Alternativas consideradas**: controllers MVC o una app SPA con React; se descarta por contradicción con la constitución.

- **Decisión 4**: No definir entidades de dominio ni casos de uso funcionales en esta fase.
  - **Racional**: la iniciativa foundation se enfoca en estructura, no en negocio.
  - **Alternativas consideradas**: incluir modelos, endpoints funcionales o persistencia; se descarta porque excede el alcance de la spec.

## Requisitos y dependencias resueltas

- Se confirma la presencia de `global.json` en la raíz del repositorio y la versión del SDK a usar.
- Se confirma la necesidad de organizar la solución con dos proyectos principales y sus proyectos de prueba.
- Se confirma que la fase foundation debe limitarse a configuración base: servicios, middleware y arranque mínimo.

## Riesgos y límites

- Si en una siguiente iniciativa se agrega dominio o persistencia, deberá hacerse bajo la arquitectura vertical por slice y con su propia spec y plan.
- Si se introduce lógica de negocio en la foundation, la solución quedará fuera del alcance ya aprobado y deberá rechazarse.

## Resultado esperado

La base quedó definida como una estructura mínima, reproducible, alineada con `global.json` y compatible con la constitución, sin introducir business logic ni features funcionales.
