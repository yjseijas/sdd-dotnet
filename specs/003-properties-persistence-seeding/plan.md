# Plan de implementación: Persistencia y seeding de propiedades

**Rama**: `003-properties-persistence-seeding` | **Fecha**: 2026-10-11 | **Especificación**: [spec.md](./spec.md)

**Entrada**: Especificación de funcionalidad de `/specs/003-properties-persistence-seeding/spec.md`

## Resumen

Esta iniciativa define el modelo persistente inicial de propiedades y estados operativos, prepara la migración de EF Core asociada, integra el seeding automático y publica los assets externos necesarios para que la imagen de cada propiedad quede disponible con una URL pública servida por la API. El objetivo es que el catálogo de propiedades quede listo para futuras consultas sin introducir rutas locales ni depender de ejecuciones manuales.

## Contexto técnico

**Lenguaje/versión**: C# con .NET 11 siguiendo el SDK fijado en `global.json` y la estructura base del repositorio.

**Dependencias principales**: ASP.NET Core Minimal APIs, EF Core, Npgsql, PostgreSQL, FluentValidation, `ILogger`, `IEntityTypeConfiguration<T>`, `UseSeeding()` y `UseAsyncSeeding()`, además de la integración de assets estáticos en la publicación del backend.

**Persistencia**: PostgreSQL como proveedor relacional con EF Core, usando una única migración coherente para la feature. La entidad `Property` se modelará en el dominio con un enum `PropertyStatus` convertido a texto en base de datos.

**Pruebas**: Pruebas unitarias en `app/backend/tests/RealtorApiTests` para validar la migración, el seeder idempotente y el comportamiento de rutas públicas `ImageUrl` y estados persistidos.

**Plataforma objetivo**: Backend `RealtorApi` ejecutado con ASP.NET Core y consumido por Blazor/Web en la misma solución, con assets estáticos publicados y accesibles desde runtime.

**Tipo de proyecto**: Aplicación web full stack con backend minimal API y persistencia relacional, orientada a catálogo inmobiliario.

**Objetivos de rendimiento**: Mantener una única migración funcional, ejecutar el seeding centralizado en arranque y evitar duplicados o rutas dispersas sin penalizar la infraestructura base.

**Restricciones**: No se permiten controladores MVC, no se debe crear lógica de negocio fuera del dominio, no se usará `HasData()` para datos dinámicos ni condicionales y no se persisten rutas físicas de `support` en `ImageUrl`.

**Escala/alcance**: Esta feature se limita al modelo inicial, migración, seeding idempotente y publicación de assets de propiedades; no incluye casos de uso nuevos ni extensiones funcionales ajenas a la data base.

## Comprobación de la constitución

- [x] El backend propuesto usa ASP.NET Core Minimal APIs, sin controladores MVC, y organiza la funcionalidad verticalmente por caso de uso.
- [x] La interfaz web propuesta usa Blazor; la lógica de negocio del backend y los contratos entre frontend y backend tienen límites explícitos.
- [x] Cada capa, proyecto, abstracción y dependencia nueva responde a una necesidad del alcance y está justificada.
- [x] Los requisitos y criterios de aceptación son verificables; el plan identifica cómo comprobar los comportamientos afectados y documenta si se requieren pruebas automatizadas según la especificación.
- [x] Se identifican las validaciones de entrada, necesidades de seguridad y tratamiento de datos sensibles pertinentes.
- [x] Los artefactos de especificación, planificación y tareas se redactan en español, conservando los identificadores técnicos cuando sea necesario.
- [x] Las decisiones no fijadas por la constitución quedan documentadas aquí, sin asumir tecnologías, patrones o modos de ejecución no aprobados.

## Estructura del proyecto

### Documentación de esta funcionalidad

```text
specs/003-properties-persistence-seeding/
├── spec.md
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   ├── README.md
│   └── seed-manifest.schema.json
└── tasks.md
```

### Código fuente (raíz del repositorio)

```text
app/
├── backend/
│   ├── src/
│   │   └── RealtorApi/
│   │       ├── Domain/
│   │       │   └── Properties/
│   │       │       ├── Property.cs
│   │       │       └── PropertyStatus.cs
│   │       ├── Infrastructure/
│   │       │   ├── Persistence/
│   │       │   │   ├── AppDbContext.cs
│   │       │   │   ├── DatabaseSeeder.cs
│   │       │   │   ├── MigrationExtensions.cs
│   │       │   │   └── Configurations/
│   │       │   │       └── PropertyConfiguration.cs
│   │       │   └── Assets/
│   │       ├── Program.cs
│   │       └── RealtorApi.csproj
│   └── tests/
│       └── RealtorApiTests/
├── frontend/
│   └── src/
│       └── RealtorWeb/
└── Realtor.sln

support/
└── seed-data/
    ├── properties.json
    ├── properties.statuses.json
    ├── images/
    └── seed-manifest.json
```

**Decisión de estructura**: La implementation se distribuirá por dominio y persistencia dentro del proyecto `RealtorApi`, manteniendo el modelo `Property` y `PropertyStatus` bajo `Domain/Properties` y evitando controladores y repositorios globales. La infraestructura de datos, migraciones y seeding se consolidará en `Infrastructure/Persistence`, mientras que los assets a publicar se resolverán desde `support/seed-data` y su manifiesto central. Esto respeta la solución única, la vertical slice y las reglas de configuración no inline de EF Core.

## Registro de complejidad

| Decisión o complejidad | Necesidad que resuelve | Alternativa más simple descartada y motivo |
|-------------------------|------------------------|--------------------------------------------|
| Conversión del estado a texto con `ValueConverter` | Garantiza que `PropertyStatus` se persista como texto legible, cumpliendo la especificación y evitando números o enums no convertidos | Persistir un entero o enum sin conversión: se descarta porque rompe la legibilidad del esquema y dificulta diagnósticos en base de datos |
| `app.MigrateAsync()` en el arranque | Asegura que EF Core aplique migraciones y ejecute `UseAsyncSeeding()` aunque no haya cambios pendientes | Comprobar `GetPendingMigrationsAsync()` antes de migrar: se descarta porque puede saltarse el seeding automático tras un arranque ya actualizado |
| Manifest central para `support/seed-data` | Centraliza la relación entre JSONs, assets e imágenes y evita rutas hardcodeadas dispersas | Hardcodear rutas de disco o del repositorio: se descarta porque rompe runtime, despliegue y portabilidad |
| Migración única de la feature | Mantiene el cambio funcional coherente y evita una migración por entidad para la misma iniciativa | Crear migraciones por entidad separada: se descarta porque contradice el criterio de una migración funcional única y complica el seguimiento |
