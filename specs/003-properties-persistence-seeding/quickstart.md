# Guía rápida de validación

## Prerrequisitos

- SDK .NET 11 definido en `global.json`.
- PostgreSQL disponible para el entorno de pruebas local o un proveedor compatible configurado en la app.
- Archivo de datos `support/seed-data` disponible en el repo.

## Validación del modelo y la compilación

```bash
dotnet restore

dotnet build app/backend/src/RealtorApi/RealtorApi.csproj
```

Resultado esperado:
- El proyecto `RealtorApi` compila sin errores.
- La entidad `Property` y el enum `PropertyStatus` quedan definidos con el esquema requerido.

## Validación de la migración y el seeding

```bash
dotnet test app/backend/tests/RealtorApiTests/RealtorApiTests.csproj --logger "console;verbosity=minimal"
```

Resultado esperado:
- Las pruebas del seeder y del modelo pasan.
- La inicialización no duplica registros ni rompe la idempotencia.
- `UseSeeding()` y `UseAsyncSeeding()` se ejecutan durante el arranque.

## Validación de arranque

```bash
dotnet run --project app/backend/src/RealtorApi/RealtorApi.csproj
```

Resultado esperado:
- La aplicación inicia correctamente.
- Se ejecuta `await app.MigrateAsync();` antes de `app.Run()`.
- Sin necesidad de ejecutar `dotnet ef database update` manualmente, el contexto de EF Core aplica migraciones y luego seeding.

## Validación de assets y URL de imagen

1. Confirmar que los archivos JSON del seed y los assets de `support/seed-data` se publican en runtime.
2. Revisar la ruta pública final de `ImageUrl` y verificar que apunta a un endpoint servido por la API, no a un path local del SO.
3. Validar que el valor del estado persistido sea texto (`Available`, `Rented`, `Maintenance`) y no un número.

## Criterios de cierre

La feature se considera lista cuando se cumplen:
- compilación exitosa,
- migración única y revisada,
- seeding idempotente,
- assets accesibles en runtime,
- `ImageUrl` usa rutas públicas de la API.
