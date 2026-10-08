# Guía rápida de validación: foundation Realtor

## Objetivo

Validar que la base de la solución cumple el alcance definido por la iniciativa y que no se introducen features ni lógica de negocio.

## Prerrequisitos

- El repositorio contiene `global.json` en la raíz.
- Debe estar instalado el SDK definido en ese archivo.
- La solución aún no implementa funcionalidad de negocio.

## Validación sugerida

1. Verificar la versión del SDK:

```bash
dotnet --version
```

2. Verificar que la versión indicada coincide con la declarada en `global.json`.

3. Comprobar la estructura de la solución:

```text
app/
├── Realtor.sln
├── backend/
│   ├── src/
│   │   └── RealtorApi/
│   └── tests/
│       └── RealtorApiTests/
└── frontend/
    ├── src/
    │   └── RealtorWeb/
    └── test/
        └── RealtorWeb/
```

4. Verificar la base del backend:
   - `RealtorApi` debe ser una app con Minimal APIs.
   - No debe haber controllers.
   - `Program.cs` debe limitarse a servicios, middleware y mapeo base.

5. Verificar la base del frontend:
   - `RealtorWeb` debe ser un proyecto Blazor Web App.
   - Debe usar Razor Components.
   - No debe incluir pantallas ni lógica funcional.

6. Ejecutar la compilación de la solución cuando la base esté creada:

```bash
dotnet build app/Realtor.sln
```

## Resultado esperado

- La estructura representa la solución única exigida por la constitución.
- La versión del SDK procede de `global.json` sin cambios manuales.
- No existe lógica de negocio ni entidades de dominio en esta iniciativa.
- La base queda lista para que las próximas specs añadan la funcionalidad real del sistema.
