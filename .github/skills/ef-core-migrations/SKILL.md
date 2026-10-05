# Skill: Migraciones EF Core

## Cuándo usar este skill

Usar este skill cuando una spec modifique el modelo persistente y sea necesario generar archivos de migración EF Core.

Este skill aplica cuando la spec:

- Agrega entidades persistentes.
- Modifica propiedades persistentes.
- Crea o cambia relaciones.
- Agrega índices o restricciones.
- Cambia nombres de tablas o columnas.
- Agrega seeders que dependen de estructura nueva.

## Regla principal

La generación de archivos de migración es responsabilidad de la spec y DEBE estar trazada en `tasks.md`.

La ejecución de migraciones pendientes NO se hace con `dotnet ef database update` como paso obligatorio de cada spec. La aplicación aplica las migraciones pendientes al arrancar mediante el skill:

```text
.skills/dotnet/database-migration-and-seeding/SKILL.md
```

## Una migración por cambio funcional coherente

Si una spec agrega 2 o más entidades relacionadas, NO se debe crear una migración por entidad.

Se DEBE crear una sola migración por spec o por cambio funcional coherente.

Ejemplos correctos:

```text
AddPropertyManagementEntities
AddTenantLeaseModel
AddMaintenanceRequestWorkflow
AddAuditLogInfrastructure
```

Ejemplos incorrectos:

```text
AddProperty
AddTenant
AddLease
```

Si `Property`, `Tenant` y `Lease` pertenecen a la misma spec y forman un mismo modelo funcional, deben entrar en una sola migración.

## Requisitos antes de crear una migración

Antes de ejecutar `dotnet ef migrations add`, validar:

- [ ] Existe `spec.md`.
- [ ] Existe `plan.md`.
- [ ] Existe `tasks.md`.
- [ ] La tarea de migración existe en `tasks.md`.
- [ ] Todas las entidades de la spec ya fueron creadas.
- [ ] Todos los `DbSet<T>` necesarios fueron registrados.
- [ ] Todas las configuraciones `IEntityTypeConfiguration<T>` fueron creadas.
- [ ] El proyecto compila.

## Comando recomendado

Ejecutar desde la raíz del repositorio o desde la ruta definida por el proyecto:

```powershell
dotnet ef migrations add AddPropertyManagementEntities `
  --project app/backend/src/RealtorApi `
  --startup-project app/backend/src/RealtorApi `
  --output-dir Infrastructure/Persistence/Migrations
```

Ajustar el nombre de la migración según la spec.

## Revisión obligatoria de la migración

Después de generar la migración, revisar:

- Método `Up()`.
- Método `Down()`.
- Snapshot de EF Core.
- Nombre de tablas.
- Tipos de columnas.
- Restricciones de nulabilidad.
- Índices.
- Foreign keys.
- Operaciones peligrosas como `DropColumn`, `DropTable` o pérdida de datos.

Si la migración contiene una operación destructiva, detenerse y solicitar confirmación explícita.

## Prohibiciones

Está prohibido:

- Crear migraciones sin tarea en `tasks.md`.
- Crear una migración por cada entidad cuando pertenecen a una misma spec.
- Ejecutar `dotnet ef database update` como paso obligatorio de finalización de la spec.
- Editar manualmente el snapshot sin justificación.
- Crear tablas manualmente en PostgreSQL para evitar migraciones.
- Usar scripts SQL no trazados.

## Relación con ejecución automática

Este skill solo genera archivos de migración versionados.

La aplicación de esas migraciones sobre la base de datos ocurre al iniciar la aplicación mediante:

```csharp
await app.MigrateAndSeedAsync();
```

Ese comportamiento está definido en:

```text
.skills/dotnet/database-migration-and-seeding/SKILL.md
```

## Checklist de revisión

Antes de marcar la tarea como `[X]`, validar:

- [ ] La migración tiene nombre descriptivo.
- [ ] La migración cubre todos los cambios persistentes de la spec.
- [ ] No hay migraciones duplicadas por entidad.
- [ ] `Up()` fue revisado.
- [ ] `Down()` fue revisado.
- [ ] El snapshot fue actualizado correctamente.
- [ ] La migración está trazada a una tarea en `tasks.md`.
- [ ] La ejecución queda delegada a `MigrateAndSeedAsync`.
