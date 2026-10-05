---
name: result-problem-details
description: Modela errores esperados mediante Result<T> y los transforma de forma consistente a ProblemDetails.
---

# Skill: Result + ProblemDetails

## Objetivo

Evitar excepciones como control de flujo para errores esperados y mantener una traducción HTTP consistente.

## Cuándo usar

Usar cuando un caso de uso puede terminar con errores esperados como:

- NotFound
- Conflict
- Validation
- BusinessRule
- Forbidden

## Modelo conceptual

```text
Handler
    ↓
Result<T>
    │
    ├── Success → HTTP Response
    │
    └── Error → Error Mapping → ProblemDetails
```

## Reglas

- los handlers devuelven `Result<T>` cuando corresponda
- los errores tienen código estable, tipo y mensaje claro
- la conversión HTTP se centraliza
- `ValidationProblemDetails` se usa para validación de entrada
- `ProblemDetails` se usa para otros errores HTTP esperados
- excepciones se reservan para fallos inesperados

## Mapeo recomendado

Definir una estrategia central equivalente a:

```text
NotFound      → 404
Conflict      → 409
Validation    → 400
Forbidden     → 403
BusinessRule  → código definido por la arquitectura
```

El status exacto para reglas de negocio debe seguir la convención aprobada del proyecto.

## Ubicación

Errores específicos del slice permanecen en el slice.

Errores realmente compartidos del módulo pueden ir en:

```text
Shared/Errors/
```

## Prohibiciones

- lanzar excepción para `NotFound` esperado
- devolver strings arbitrarios como error
- devolver objetos anónimos inconsistentes
- exponer `Exception.Message` o stack trace
- mapear el mismo tipo de error a diferentes status codes sin justificación

## Checklist

- [ ] El error es esperado o inesperado correctamente clasificado.
- [ ] Se usa `Result<T>` cuando corresponde.
- [ ] El código de error es estable.
- [ ] La traducción HTTP es centralizada.
- [ ] Se usa `ProblemDetails`.
- [ ] No se exponen detalles internos.
