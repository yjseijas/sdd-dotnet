---
name: vertical-slice-handlers
description: Implementa handlers concretos por caso de uso, auto-registro mediante IHandler y acceso directo a AppDbContext cuando corresponde.
---

# Skill: Vertical Slice Handlers

## Objetivo

Mantener un handler concreto por caso de uso, sin MediatR obligatorio, sin interfaces individuales innecesarias y sin repositories triviales.

## Cuándo usar

Usar cuando una tarea:

- agrega un caso de uso backend
- crea o modifica un handler
- modifica el marker `IHandler`
- modifica el registro automático de handlers

## Convención

```csharp
public interface IHandler;
```

Ejemplo:

```csharp
internal sealed class CreatePropertyHandler(
    AppDbContext context,
    ILogger<CreatePropertyHandler> logger)
    : IHandler
{
    public async Task<Result<CreatePropertyResponse>> HandleAsync(
        CreatePropertyRequest request,
        CancellationToken cancellationToken)
    {
        // caso de uso
    }
}
```

## Responsabilidades

Un handler debe:

- representar un solo caso de uso
- coordinar dominio, persistencia y dependencias externas
- usar I/O asíncrono
- propagar `CancellationToken`
- devolver resultado explícito
- mantener lógica específica del caso de uso

## Registro automático

La infraestructura debe:

1. escanear el assembly aprobado
2. encontrar clases concretas que implementen `IHandler`
3. registrarlas con el lifetime aprobado
4. evitar duplicados

Agregar un handler no debe requerir modificar `Program.cs`.

## Persistencia

Se permite usar `AppDbContext` directamente desde el handler cuando el caso de uso lo justifique.

No crear repositories para envolver simples llamadas a EF Core.

## MediatR

No introducir MediatR solamente para conectar endpoint y handler.

Solo puede incorporarse si una spec o decisión arquitectónica aprobada demuestra necesidad concreta.

## Prohibiciones

- crear `ICreatePropertyHandler` sin necesidad real de múltiples implementaciones
- registrar handlers individualmente
- mantener listas manuales de handlers
- crear scanners por feature
- crear generic repositories
- crear command/query wrappers vacíos
- bloquear I/O asíncrono

## Checklist

- [ ] El handler representa un caso de uso.
- [ ] Implementa `IHandler`.
- [ ] Se registra automáticamente.
- [ ] Propaga `CancellationToken`.
- [ ] No existe interfaz individual innecesaria.
- [ ] No existe repository trivial.
- [ ] El logging es estructurado cuando corresponde.
- [ ] El proyecto compila.
