---
name: domain-events
description: Implementa eventos y handlers desacoplados para side effects reales aprobados por la spec o el plan.
---

# Skill: Domain Events

## Objetivo

Desacoplar efectos secundarios reales del caso de uso principal sin introducir eventos innecesarios.

## Cuándo usar

Usar solo cuando la spec o el plan justifiquen:

- uno o varios side effects
- múltiples consumidores potenciales
- desacoplamiento entre acción principal y reacciones
- publicación posterior a una operación principal exitosa

## Flujo

```text
Handler principal
    ↓
Persistencia exitosa
    ↓
Evento
    ↓
Event Handler(s)
```

## Reglas

- el handler principal no debe conocer todos los consumidores
- el evento debe representar un hecho ocurrido
- los handlers de evento deben tener una responsabilidad clara
- propagar `CancellationToken`
- registrar logging estructurado cuando corresponda
- definir explícitamente estrategia de confiabilidad si se requiere entrega garantizada

## No usar cuando

- solo existe una llamada directa simple
- no hay necesidad de desacoplamiento
- se quiere anticipar un posible requerimiento futuro
- el comportamiento puede resolverse claramente dentro del caso de uso

## Confiabilidad

Si el evento debe sobrevivir fallos de proceso o garantizar entrega después de commit, la spec y el plan deben definir una estrategia explícita, por ejemplo un mecanismo transaccional/outbox aprobado.

No asumir confiabilidad distribuida automáticamente.

## Checklist

- [ ] El evento está justificado por spec o plan.
- [ ] Representa un hecho real.
- [ ] Existe una necesidad de desacoplamiento.
- [ ] El handler principal no conoce consumidores concretos.
- [ ] La estrategia de orden respecto a persistencia está definida.
- [ ] La confiabilidad requerida está explícitamente definida.
