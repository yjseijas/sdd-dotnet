# Guía rápida de validación: infraestructura base del backend

## Objetivo

Validar que la infraestructura del backend cumple la intención de la especificación: autodescubrimiento de endpoints y handlers, validación automática, transformación de errores a `ProblemDetails` y la sonda `/health`.

## Prerrequisitos

- SDK .NET 11 instalado y compatible con `global.json`.
- Proyecto `app/backend/src/RealtorApi` en el repositorio.
- Proyecto de pruebas `app/backend/tests/RealtorApiTests` disponible.

## Validación sugerida

1. Comprobar el SDK y la solución:

```bash
dotnet --version
dotnet build app/Realtor.sln
```

2. Ejecutar las pruebas del backend:

```bash
dotnet test app/backend/tests/RealtorApiTests/RealtorApiTests.csproj
```

3. Ejecutar la aplicación backend:

```bash
dotnet run --project app/backend/src/RealtorApi
```

4. Validar la sonda `/health` con una petición HTTP local:

```bash
curl http://localhost:5000/health
```

5. Validar que la respuesta cambia de estado y se comporta como una API mínima sin autenticación:

```json
{ "status": "healthy" }
```

## Resultado esperado

- La solución compila y las pruebas unitarias pasan.
- Los endpoints, handlers y validators se registran por convención y assembly discovery.
- Los errores esperados se transforman en `ProblemDetails` con códigos HTTP correctos.
- La app responde a `/health` sin requerir autenticación ni acceso a persistencia.
