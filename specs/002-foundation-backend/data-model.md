# Modelo de datos e interfaces de infraestructura

## Objetivo

Este documento registra los contratos internos que sustentan la infraestructura base del backend. La iniciativa no define entidades de negocio ni persistencia; la intención es establecer los acuerdos técnicos mínimos que permitirán que futuras slices compartan descubrimiento, validación y manejo de errores.

## Contratos centrales

### 1. `ISlice`

Representa una unidad de endpoint minimal API registrada de forma automática.

```csharp
public interface ISlice
{
    void AddEndpoint(IEndpointRouteBuilder app);
}
```

**Responsabilidad**:
- Encapsular el registro de un endpoint del backend.
- Permitir que el sistema reduzca el acoplamiento con `Program.cs`.
- Permitir convertir una nueva feature en una implementación aislada del arranque central.

### 2. `IHandler<TRequest, TResult>`

Representa un handler concreto asociado a un caso de uso o acción transversal.

```csharp
public interface IHandler<TRequest, TResult>
{
    Task<TResult> HandleAsync(TRequest request, CancellationToken cancellationToken);
}
```

**Responsabilidad**:
- Encapsular la lógica discreta del caso de uso.
- Ser despachable por parte del mecanismo de registro automático.
- Facilitar pruebas unitarias sin acoplar la validación al endpoint.

### 3. Result / ProblemDetails

La infraestructura define una representación unificada de errores esperados para que puedan convertirse en `ProblemDetails` HTTP.

```csharp
public sealed record Result<T>(bool IsSuccess, T? Value, ErrorDetails? Error);
```

**Responsabilidad**:
- Separar el flujo feliz del flujo de error sin lanzar excepciones para decisiones de negocio esperadas.
- Mantener un estado HTTP y un detalle de error explícitos.
- Convertirse en una respuesta HTTP estándar mediante un adaptador central.

### 4. Validación por tipo

La validación se asocia a un tipo de solicitud mediante convención y una interfaz de validator.

```csharp
public interface IValidator<TRequest>
{
    ValidationResult Validate(TRequest request);
}
```

**Responsabilidad**:
- Ejecutar reglas de validación cuando existe un validator aplicable al tipo de la solicitud.
- Devolver errores compatibles con `ValidationProblemDetails`.
- Permitir respuestas 400 sin bloquear el flujo cuando no exista un validator.

### 5. `/health`

La sonda de infraestructura de salud responde un estado simple del proceso, sin verificar la persistencia.

```json
{ "status": "healthy" }
```

**Responsabilidad**:
- Darvisibilidad operacional mínima.
- No depender de DB ni autenticación.
- Servir como demostración de registro automático del esquema de endpoints.

## Reglas de diseño

- El backend no define entidades persistentes ni modelos de negocio en esta iniciativa.
- Todos los contratos se registran por inspección de assembly y no por listas manuales.
- Las decisiones de validación deben quedar centralizadas en un filtro o middleware de infraestructura.
- Los errores esperados deben preservar el estado HTTP y el detalle del problema.
- El logging debe usarse con propiedades estructuradas para facilitar diagnósticos.
