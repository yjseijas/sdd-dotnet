---
name: constitution-update-spec-status
description: Implementa una política obligatoria de ciclo de estados para specs y que el cambio de estado ocurra automáticamente en el flujo Speckit.
agent: speckit.constitution
---

/speckit.constitution

### Objetivo
Implementa una política obligatoria de ciclo de estados para specs y que el cambio de estado ocurra automáticamente en el flujo Speckit.

### Alcance
1. Actualiza la constitución en constitution.md para agregar una sección llamada Ciclo de estado de specs.
2. Ajusta el flujo de implementación para que el estado del spec cambie automáticamente durante la ejecución.
3. Mantén todo el contenido markdown en español.

### Estados Canónicos
- Borrador: al crear el spec.
- Aprobada: al aprobar revisión de spec.
- En implementación: al iniciar implementación.
- Implementada: al finalizar implementación con validaciones cumplidas.

### Reglas Obligatorias
1. Solo se permiten estas transiciones:
- Borrador - Aprobada
- Aprobada - En implementación
- En implementación - Implementada
2. Está prohibido marcar un spec como Implementada si:
- Existe al menos una tarea sin completar en tasks.md
- No existe evidencia de validación en quickstart.md
3. Si falla alguna condición, mantener estado En implementación y reportar bloqueo con causa explícita.

### Cambios Técnicos Requeridos
1. Mantener estado inicial en plantilla de spec en spec-template.md
2. Extender lógica de implementación en speckit.implement.agent.md para:
- Cambiar a En implementación al inicio
- Cambiar a Implementada al final solo si se cumplen condiciones
3. Si el proyecto usa hooks en extensiones, agregar o ajustar hooks before_implement y after_implement para asegurar que la transición sea automática.

### Gobernanza Y Versionado
- Clasificar este cambio constitucional como MINOR por agregar una regla nueva de gobierno operativo.
- Actualizar versión y fecha de enmienda en la constitución.

### Criterios De Aceptación
1. El estado cambia automáticamente sin edición manual.
2. No se puede cerrar implementación con estado inválido.
3. El flujo deja trazabilidad clara de por qué cambió o no cambió el estado.