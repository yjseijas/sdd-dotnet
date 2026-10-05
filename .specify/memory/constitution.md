<!--
Informe de impacto de sincronización
- Cambio de versión: plantilla sin versión → 1.0.0 (adopción inicial).
- Principios modificados: no había principios ratificados; se establecen cinco
  principios iniciales.
- Secciones añadidas: Restricciones de arquitectura y plataforma; Flujo de
  desarrollo y verificación.
- Secciones eliminadas: ninguna.
- Plantillas: ✅ .specify/templates/plan-template.md actualizado; ✅
  .specify/templates/spec-template.md traducido al español; ✅
  .specify/templates/tasks-template.md traducido y alineado con verificación
  por plan, arquitectura vertical y Blazor.
- Comandos: ⚠ .specify/templates/commands/ no existe; no hay archivos que
  revisar ni actualizar.
- Guías de ejecución: ✅ guías existentes revisadas; no requieren cambios.
- Pendientes: ninguno. La fecha de ratificación corresponde a la adopción
  inicial de esta constitución.
-->
# Constitución del Proyecto Realtor

## Principios Fundamentales

### I. Solución Única y Compartida
Esta solución es una y no puede dividirse. Frontend, backend, dominio y persistencia evolucionan dentro del mismo sistema.

Toda iniciativa en specs DEBE contribuir a esta solución compartida, independientemente de si pertenece a la capa frontend o backend.

No se permiten soluciones paralelas, bifurcaciones de arquitectura ni estructuras separadas por tipo de capa.

### II. Spec-Driven Development (No Negociable)
Las specs aprobadas en specs, carpeta ubicada en la raíz del repositorio, son la única fuente de verdad del proyecto.

No se DEBE implementar ninguna funcionalidad que no esté descrita en el spec vigente.

El flujo obligatorio mínimo es:
speckit.specify -> speckit.plan -> speckit.tasks -> speckit.implement

Para specs fundacionales o de alto impacto se recomienda:
speckit.specify -> speckit.clarify -> speckit.plan -> speckit.analyze -> speckit.tasks -> speckit.implement

Ninguna fase obligatoria puede saltarse.

### III. Arquitectura Canónica de Backend y Frontend
El backend DEBE organizarse por features y casos de uso con Vertical Slice Architecture.

Están prohibidos controllers y carpetas técnicas globales genéricas para orquestar el dominio.

Program solo configura servicios, middleware, registro de infraestructura y mapeo de endpoints.

El frontend DEBE implementarse con Blazor Web App y Razor Components.

### IV. Stack Tecnológico Obligatorio
El stack no es negociable y DEBE aplicarse sin excepción:

- SDK: .NET 11.
- Frontend: Blazor Web App con Razor Components.
- Backend: ASP.NET Core Minimal APIs.
- Persistencia: EF Core + Npgsql + PostgreSQL.
- Comunicación: Refit con IHttpClientFactory.
- Validación y errores: FluentValidation + ProblemDetails.
- Estilos: CSS propio centralizado en wwwroot/app.css, sin frameworks CSS.
- Íconos: Lucide Icons como sistema principal.

### V. Calidad de Dominio y Contratos
La lógica de negocio no DEBE residir en componentes UI, endpoints ni DbContext.

Las entidades de dominio no DEBEN mezclarse con requests o responses.

Todo endpoint DEBE tener validación consistente y retornar ProblemDetails con status codes correctos.

DEBE existir logging estructurado.

Solo se permiten unit tests, salvo que una spec aprobada justifique explícitamente otro tipo de prueba en plan y tasks.

### VI. Idioma y Documentación
Todo contenido markdown propio del repositorio DEBE estar escrito en español.

Se permiten nombres técnicos, APIs, comandos, namespaces y paquetes en inglés cuando corresponda.

Está prohibido mezclar idiomas dentro de un mismo documento.

### VII. Gobernanza y Divergencias
Esta constitución prevalece sobre cualquier preferencia personal o convención no normativa.

Si existe divergencia entre convenciones locales y comportamiento operativo upstream de spec-kit, el agente DEBE detenerse, reportar el conflicto y esperar instrucción humana explícita.

Toda divergencia se resuelve por enmienda explícita de esta constitución, no por edición silenciosa de scripts ni por reubicación unilateral de archivos.

---

## Estructura del Repositorio

Las specs viven en la raíz del repositorio:

- specs/NNN-nombre/spec.md
- specs/NNN-nombre/plan.md
- specs/NNN-nombre/tasks.md

No se permite almacenar specs funcionales dentro de .specify.

La carpeta .specify se reserva para infraestructura operativa de spec-kit.

---

## Método de Trabajo

Cada iniciativa DEBE contener exactamente tres archivos:

- spec.md
- plan.md
- tasks.md

Antes de implementar:

1. Leer spec.md.
2. Leer plan.md.
3. Leer tasks.md.
4. Identificar tarea específica.
5. Implementar solo alcance aprobado.
6. Verificar resultado.
7. Marcar tarea como X solo tras verificación.

Reglas de secuencia obligatorias:

1. speckit.clarify requiere spec.md.
2. speckit.plan requiere spec.md.
3. speckit.analyze requiere spec.md y plan.md.
4. speckit.tasks requiere plan.md.
5. speckit.implement requiere tasks.md.

---

## Modo Interactivo de Preguntas

Los comandos speckit.specify y speckit.clarify operan en modo interactivo obligatorio: presentan sus preguntas de una en una y esperan respuesta antes de continuar.

El comando speckit.plan opera en modo interactivo condicional: solo lanza preguntas si existen decisiones estructurales que afecten todas las specs futuras y que no estén resueltas en esta constitución ni en la spec vigente.

Cuando un comando opera en modo interactivo, DEBE seguir este protocolo sin excepción.

Toda pregunta DEBE redactarse de forma clara, directa y entendible por el humano, evitando ambigüedad o jerga innecesaria, para que pueda escoger la opción correcta con seguridad.

### Formato de pregunta con opciones

Pregunta [N de TOTAL] - [tema corto]  
[Enunciado claro, concreto y entendible de la pregunta]

Por qué importa: [1 línea sobre el impacto de decidir mal]

A) [opción concreta con valor específico]  
B) [opción concreta con valor específico]  Recomendado  
C) [opción concreta con valor específico]  
D) Otro - escribe tu respuesta

Responde con la letra (A, B, C o D) o escribe tu respuesta libre.

### Formato de pregunta Sí/No

Pregunta [N de TOTAL] - [tema corto]  
[Enunciado claro y entendible de la pregunta]

Por qué importa: [1 línea]

S) Sí  Recomendado  
N) No

Responde S o N.

### Reglas de las opciones

Cada opción A, B o C DEBE ser concreta y ejecutable, nunca genérica.

Ejemplo correcto: 1024px (tablet landscape).  
Ejemplo prohibido: Un breakpoint estándar.

Las opciones DEBEN ser mutuamente excluyentes: cada una lleva a un resultado de código distinto.

La opción marcada como Recomendado DEBE ser la más adoptada por equipos que usan este stack o la que mejor respeta los principios de esta constitución.

La opción D) Otro SIEMPRE debe estar presente como escape hatch para respuesta personalizada.

### Reglas de respuesta

Si el usuario responde con una letra (A, B, C, S o N), el agente DEBE confirmar la elección en una línea con el valor concreto elegido y pasar inmediatamente a la siguiente pregunta.

Si el usuario responde con texto libre o elige D, el agente DEBE aceptar la respuesta, confirmarla en una línea y pasar inmediatamente a la siguiente pregunta.

Al terminar todas las preguntas, el agente DEBE mostrar un resumen de las decisiones tomadas y generar el artefacto correspondiente: spec.md, sección de clarificaciones en spec.md, o plan.md.

---

## Gobierno

Esta constitución es el documento rector del proyecto y tiene precedencia sobre cualquier otra práctica, convención o preferencia personal.

Las enmiendas DEBEN documentarse con versión semántica:

- MAJOR: eliminación o redefinición incompatible de un principio o del stack obligatorio.
- MINOR: nuevo principio o sección añadida; expansión material de un principio existente.
- PATCH: aclaraciones, redacción o correcciones no semánticas.

Toda PR o revisión DEBE verificar cumplimiento de esta constitución antes de aprobarse.

Cualquier violación DEBE justificarse explícitamente en plan de la iniciativa correspondiente. En ausencia de justificación, la PR DEBE rechazarse.

---

## Precedencia ante Conflictos con Upstream Spec-Kit

Cuando un script, hook, plantilla o feature active state provisto por spec-kit upstream entre en conflicto con esta constitución sobre rutas físicas de artefactos, ubicación de specs, nombres de carpetas o estructura operativa, el agente DEBE detenerse y escalar decisión humana.

Está prohibido armonizar el conflicto moviendo artefactos hacia rutas no canónicas o parcheando scripts upstream para satisfacer convenciones locales sin aprobación explícita.

---

## Protocolo del Agente ante Divergencias

Si un agente detecta divergencia entre un script de spec-kit y esta constitución, DEBE:

1. Detenerse inmediatamente y no ejecutar el comando que provocaría la divergencia.
2. Reportar al humano la naturaleza exacta del conflicto.
3. Esperar instrucción explícita antes de continuar.

Está prohibido al agente mover archivos por iniciativa propia, renombrar carpetas, o editar scripts y templates upstream para resolver el conflicto.

La única resolución legítima es la decisión humana documentada vía enmienda constitucional o ajuste explícito de convención local.


**Versión**: 1.0.0 | **Ratificada**: 2026-10-05 | **Última enmienda**: 2026-10-05
