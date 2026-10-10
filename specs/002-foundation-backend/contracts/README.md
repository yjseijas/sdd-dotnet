# Contratos: infraestructura base del backend

## Estado

Esta iniciativa define contratos internos de infraestructura, no contratos de negocio ni de persistencia. Su objetivo es soportar la base común para futuras features del backend.

## Contratos clave

- `ISlice`: contrato para registrar endpoints minimal API automáticamente.
- `IHandler<TRequest, TResult>`: contrato para handlers ejecutados por consenso de assembly y registro central.
- `Result<T>` / `ErrorDetails`: representación tipada de errores esperados y su código HTTP asociado.
- `IValidator<TRequest>`: integración con validación automática basada en el tipo de solicitud.
- `/health`: endpoint de salud con respuesta simple y sin autenticación de aplicación.

## Regla de futuro

Los contratos de negocio, DTOs de intercambio y endpoints funcionales se definirán en las specs que añadan casos de uso reales del dominio Realtor.
