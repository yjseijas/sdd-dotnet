---
name: 003-properties-persistence-seeding  
agent: speckit.specify
---

/speckit.specify

Crear la especificación 003-properties-persistence-seeding.

## Objetivo
Definir e implementar el modelo persistente inicial de propiedades y estados de propiedad, incluyendo migración EF Core, seeding idempotente automático y manejo de archivos externos para que queden consumibles por el proceso de inserción y por futuras consultas de propiedades con imagen.

## Contexto obligatorio
- Respetar estrictamente las reglas de persistencia definidas en database.instructions.md.
- Seguir los skills oficiales de entidad/configuración, migraciones y seeding del repositorio.
- Mantener la arquitectura backend y el patrón de Vertical Slice.
- Mantener compatibilidad funcional con el patrón de slice existente, por ejemplo HealthSlice.cs, evitando cambios no requeridos por la spec.

## Estructura obligatoria (ajustada)
La organización principal DEBE ser por Features para casos de uso de negocio.  
No crear una jerarquía paralela de endpoints de negocio fuera de Features.

app/backend/src/RealtorApi/
- Domain/
  - Properties/
    - Property.cs
    - PropertyStatus.cs
- Features/
  - Properties/
    - CreateProperty/
    - UpdateProperty/
    - GetPropertyById/
    - ListProperties/
    - ChangePropertyStatus/
    - Shared/ (opcional)
- Infrastructure/
  - Persistence/
    - AppDbContext.cs
    - DatabaseSeeder.cs
    - MigrationExtensions.cs
    - Configurations/
      - PropertyConfiguration.cs
  - Migrations/
- Program.cs
- RealtorApi.csproj
- appsettings.json

## Reglas de organización
1. Todo endpoint de negocio DEBE vivir dentro de su slice en Features.
2. No crear carpetas paralelas para endpoints de negocio duplicando Features.
3. Shared solo se permite si existe reutilización real entre 2 o más slices de la misma feature.
4. Si no hay reutilización comprobable, no crear Shared.
5. Persistencia y migraciones se mantienen bajo Infrastructure para cumplir reglas vigentes de base de datos.

## Fuentes externas de seed y assets
- properties.json
- properties-statuses.json
- properties
- Crear manifiesto central obligatorio:
  - support/seed-data/seed-manifest.json

## Alcance funcional incluido

### 1) Entidades persistentes
- PropertyStatus con valores:
  - Available
  - Rented
  - Maintenance
- Property con al menos:
  - Id
  - Title
  - Description
  - Address
  - Price
  - Status
  - BedroomCount
  - BathroomCount
  - AreaSquareMeters
  - ImageUrl
  - CreatedAt
  - UpdatedAt
- Status debe almacenarse como string mediante conversión EF Core.

### 2) Configuración EF Core
- Configurar entidades mediante IEntityTypeConfiguration en Infrastructure/Persistence/Configurations.
- Prohibido mapping inline en OnModelCreating.
- Registrar DbSet necesarios en AppDbContext.
- Aplicar restricciones, longitudes, precisión y defaults según definición funcional.

### 3) Migración EF Core
- Generar una sola migración coherente para todo el cambio funcional de esta spec.
- Revisar Up, Down y snapshot antes de cerrar tareas.
- Prohibido generar múltiples migraciones por cada entidad si pertenecen al mismo cambio funcional.

### 4) Seeding automático obligatorio
- Configurar UseSeeding y UseAsyncSeeding en DbContext.
- Implementar DatabaseSeeder con versión sync y async equivalentes.
- Seed idempotente obligatorio.
- Program debe ejecutar siempre app.MigrateAsync antes de app.Run.
- Prohibido invocar manualmente el seeder desde MigrationExtensions o como fallback manual.
- El flujo debe cumplir la secuencia definida en database.instructions.md, incluyendo ejecución de UseAsyncSeeding aunque no existan migraciones pendientes.

### 5) Inclusión de archivos externos en el proyecto
- Integrar JSON, manifest e imágenes de support en RealtorApi.csproj para build y publish.
- Los JSON y el manifest deben quedar disponibles en runtime para el seeding.
- Las imágenes deben quedar disponibles en la ubicación final consumible por la API.
- El seed debe resolver rutas desde seed-manifest.json y no con rutas hardcodeadas dispersas.

### 6) Regla de ImageUrl
- ImageUrl en Property debe guardar la ruta final pública servida por la API.
- Prohibido guardar en ImageUrl rutas físicas de support.
- La ruta final debe permitir devolución directa de imagen en futuras consultas.

### 7) Pruebas requeridas
- Validar configuración EF de entidad y conversión enum-string.
- Validar que la migración contiene el esquema esperado.
- Validar idempotencia del seed.
- Validar ejecución de seed async vía UseAsyncSeeding durante arranque.
- Validar lectura de JSON y seed-manifest.
- Validar copia/sincronización de imágenes a destino final y consistencia de ImageUrl.

## Restricciones explícitas
- No usar controllers.
- No usar HasData para este escenario.
- No condicionar MigrateAsync por migraciones pendientes.
- No usar dotnet ef database update como criterio obligatorio de cierre.
- Todo cambio debe estar trazado en tasks.md antes de marcar completado.

## Criterios de éxito
- Modelo persistente de Property y PropertyStatus implementado y compilando.
- Migración única generada y revisada.
- Seeding automático operativo con UseSeeding y UseAsyncSeeding.
- Datos de support integrados al csproj y consumibles en runtime.
- Imágenes disponibles en ruta final del proyecto API.
- ImageUrl persistido con ruta final servida por la API.