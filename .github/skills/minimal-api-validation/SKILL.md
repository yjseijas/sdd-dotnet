---
name: minimal-api-validation
description: Implementa validación automática de Minimal APIs mediante FluentValidation, assembly scanning y ValidationFilterFactory.
---

# Skill: Minimal API Validation

## Objetivo

Agregar validators sin modificar `Program.cs`, sin ejecutar validación manual y sin agregar filtros individuales por endpoint.

Arquitectura:

```text
FluentValidation + Assembly Scanning + ValidationFilterFactory + IServiceProviderIsService
```

## Cuándo usar

Usar cuando una tarea:

- crea o modifica un validator
- modifica el registro de validators
- modifica `ValidationFilterFactory`
- cambia el contrato de `ValidationProblemDetails`
- modifica la integración con `MapSliceEndpoints`

## Registro automático

Preferir el mecanismo oficial de FluentValidation:

```csharp
builder.Services.AddValidatorsFromAssembly(
    typeof(Program).Assembly,
    ServiceLifetime.Scoped);
```

No crear un scanner propio salvo justificación arquitectónica aprobada.

## Ubicación

Los validators específicos deben vivir cerca del slice:

```text
Features/
  Properties/
    CreateProperty/
      CreateProperty.Validators.cs
```

## Factory

`ValidationFilterFactory.Create` debe:

1. inspeccionar la firma del handler
2. identificar parámetros candidatos
3. construir `IValidator<T>`
4. consultar disponibilidad con `IServiceProviderIsService`
5. agregar validación cuando exista validator
6. devolver pass-through cuando no exista

## Flujo

```text
Request
    ↓
Factory
    ↓
¿Existe IValidator<T>?
   │
   ├── Sí → ValidateAsync
   │          ├── válido → Handler
   │          └── inválido → 400 + ValidationProblemDetails
   │
   └── No → Handler
```

## Reglas

- usar `ValidateAsync`
- propagar `CancellationToken`
- mantener formato consistente de `ValidationProblemDetails`
- agrupar errores por propiedad cuando corresponda
- mantener reglas de negocio fuera de validators

## Prohibiciones

- registrar validators individualmente
- llamar `Validate` o `ValidateAsync` desde endpoints
- agregar filtros individuales a endpoints
- crear validators vacíos para forzar convenciones
- devolver strings o excepciones como error de validación
- duplicar `ValidationFilterFactory`

## Checklist

- [ ] La validación está descrita por la spec.
- [ ] El validator vive en el slice correspondiente.
- [ ] No se agregó registro manual.
- [ ] El validator es descubierto automáticamente.
- [ ] El endpoint no valida manualmente.
- [ ] Sin validator existe pass-through.
- [ ] El error devuelve HTTP 400.
- [ ] Se usa `ValidationProblemDetails`.
- [ ] Se propaga `CancellationToken`.
- [ ] El proyecto compila.
