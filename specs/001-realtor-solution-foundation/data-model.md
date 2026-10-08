# Modelo de datos: foundation de la solución Realtor

## Estado del diseño

Esta iniciativa no define un modelo de dominio ni persistencia. Su alcance es netamente estructural: preparar la solución principal, los proyectos backend/frontend y sus pruebas.

## Entidades

No aplica ninguna entidad de negocio en esta fase.

## Estructura de solución seleccionada

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

## Reglas de validación para esta fase

- La versión de .NET debe provenir de `global.json` y no puede modificarse desde esta iniciativa.
- La solución debe ser única y compartida para backend y frontend.
- El backend debe usar Minimal APIs y evitar controllers.
- El frontend debe usar Blazor Web App con Razor Components.
- No se crean entidades de dominio, casos de uso ni se implementa negocio.

## Observaciones

Los modelos concretos, entidades de dominio, contratos de API y flujos funcionales se definirán en iniciativas posteriores de la solución.
