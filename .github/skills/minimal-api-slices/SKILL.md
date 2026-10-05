---
name: minimal-api-slices
description: Implementa y mantiene endpoints Minimal API auto-descubribles mediante ISlice, reflection y mapeo centralizado.
---

# Skill: Minimal API Slices

## Objetivo

Permitir que agregar un endpoint no requiera modificar `Program.cs`, registries manuales ni listas centralizadas.

Arquitectura:

```text
Minimal API + ISlice + Reflection + DI + MapSliceEndpoints
```

## Cuándo usar

Usar cuando una tarea:

- agrega o modifica un endpoint
- crea o modifica `ISlice`
- modifica `RegisterSlices`
- modifica `MapSliceEndpoints`
- migra endpoints manuales al patrón ISlice

## Precondiciones

1. Leer `spec.md`, `plan.md` y `tasks.md`.
2. Leer `backend.instructions.md`.
3. Verificar si la infraestructura ya existe.
4. No recrear infraestructura existente.

## Contrato

```csharp
public interface ISlice
{
    void AddEndpoint(IEndpointRouteBuilder app);
}
```

Cada endpoint debe ser público, concreto, no abstracto y stateless.

## Registro

`RegisterSlices(IServiceCollection, Assembly)` debe:

1. inspeccionar el assembly aprobado
2. encontrar clases públicas, concretas y no abstractas que implementen `ISlice`
3. registrarlas bajo el contrato `ISlice`
4. evitar duplicados

No descubrir por nombre de clase.

## Mapeo

`MapSliceEndpoints(IEndpointRouteBuilder)` debe:

1. crear el `RouteGroupBuilder`
2. aplicar `ValidationFilterFactory.Create`
3. resolver `IEnumerable<ISlice>`
4. llamar `AddEndpoint(group)` para cada slice

## Agregar un endpoint

1. Confirmar ruta, verbo, request, response y status codes en la spec.
2. Crear el directorio del caso de uso.
3. Crear la clase endpoint e implementar `ISlice`.
4. Delegar al handler del caso de uso.
5. No modificar `Program.cs`.
6. No crear registry manual.
7. Ejecutar build y tests autorizados.

## Restricciones de lifetime

Si `ISlice` se registra como singleton:

- debe ser stateless
- no debe recibir dependencias scoped en constructor
- las dependencias de request deben resolverse en el handler de ruta o delegarse al handler del caso de uso

## Prohibiciones

- registrar endpoints individuales en `Program.cs`
- crear `PropertiesEndpoints.cs` u otros registries manuales
- duplicar scanners de reflection
- mantener listas manuales de tipos
- descubrir endpoints por convención de nombre
- guardar estado mutable de request en un slice

## Checklist

- [ ] La tarea existe en `tasks.md`.
- [ ] El endpoint está descrito en `spec.md`.
- [ ] Implementa `ISlice`.
- [ ] La clase es pública, concreta y stateless.
- [ ] `AddEndpoint` contiene el mapping de ruta.
- [ ] No se modificó `Program.cs` para registrar el endpoint.
- [ ] No se creó registry manual.
- [ ] La validación global sigue aplicada.
- [ ] El proyecto compila.
- [ ] Los tests autorizados pasan.
