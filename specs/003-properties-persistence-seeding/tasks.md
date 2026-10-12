# Tareas: Persistencia y seeding de propiedades

**Entrada**: Documentos de diseño de `/specs/003-properties-persistence-seeding/`

**Prerrequisitos**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `quickstart.md` y `contracts/` cumplen la base de la feature.

**Verificación**: Esta feature requiere comprobación de migración, seeding idempotente, integración de assets y validación del flujo de inicio. No se fuerza TDD si la especificación no lo exige, pero sí se deben ejecutar comprobaciones automatizadas de compilación y pruebas del backend.

## Formato de tareas

- [ ] T001 [P] ...
- [ ] T002 [US1] ...

## Fase 1: Preparación y alineación del alcance

**Objetivo**: Confirmar la estructura del backend, el manifiesto de seed y los archivos de soporte que debe publicar la feature.

- [ ] T001 [P] Validar la estructura base del proyecto `RealtorApi` y la ubicación de `support/seed-data` para esta feature en `app/backend/src/RealtorApi/` y `support/seed-data/`.
- [ ] T002 [P] Confirmar el contrato del manifest y la ruta pública esperada de las imágenes según `specs/003-properties-persistence-seeding/contracts/README.md` y `contracts/seed-manifest.schema.json`.

---

## Fase 2: Base de persistencia y dominio

**Objetivo**: Preparar el dominio y la infraestructura mínima necesaria para persistir propiedades con estado estable en PostgreSQL.

- [ ] T003 [US1] Implementar el enum `PropertyStatus` en `app/backend/src/RealtorApi/Domain/Properties/PropertyStatus.cs` con los valores `Available`, `Rented` y `Maintenance`.
- [ ] T004 [US1] Implementar la entidad `Property` en `app/backend/src/RealtorApi/Domain/Properties/Property.cs` con los campos mínimos exigidos por la spec: `Id`, `Title`, `Description`, `Address`, `Price`, `Status`, `BedroomCount`, `BathroomCount`, `AreaSquareMeters`, `ImageUrl`, `CreatedAt` y `UpdatedAt`.
- [ ] T005 [US1] Crear `AppDbContext` en `app/backend/src/RealtorApi/Infrastructure/Persistence/AppDbContext.cs` con `DbSet<Property>` y `modelBuilder.ApplyConfigurationsFromAssembly(...)` sin configuración inline.
- [ ] T006 [US1] Crear la configuración `PropertyConfiguration` en `app/backend/src/RealtorApi/Infrastructure/Persistence/Configurations/PropertyConfiguration.cs` para mapear `PropertyStatus` como texto y aplicar las restricciones del modelo.
- [ ] T007 [US1] Generar y revisar la migración EF Core única para esta feature en `app/backend/src/RealtorApi/Infrastructure/Persistence/Migrations/`, comprobando `Up()`, `Down()`, snapshot y columnas críticas.

**Punto de control**: El dominio de propiedades queda persistido con estados correctos y el esquema refleja la migración coherente de la feature.

---

## Fase 3: Historia de usuario 1: Persistir propiedades y su estado (Prioridad: P1) 🎯 MVP

**Objetivo**: Garantizar que la propiedad e información operativa queden guardadas y consultables con un estado persistido de forma estable.

**Prueba independiente**: Verificar que la entidad `Property` y el enum `PropertyStatus` compilan, persisten con el valor textual correcto y que la migración refleja los campos mínimos del dominio.

### Verificación de la historia de usuario 1

- [ ] T008 [P] [US1] Validar el modelo de dominio y la conversión a texto en base de datos con pruebas de backend en `app/backend/tests/RealtorApiTests/`.
- [ ] T009 [P] [US1] Revisar la migración generada para confirmar que `Status` se guarda como texto y no como número en `AppDbContext` y `PropertyConfiguration`.

### Implementación de la historia de usuario 1

- [ ] T010 [US1] Ajustar el proyecto y los archivos de dominio/persistencia para que `Property` quede registrado en el sistema y esté listo para la migración final.
- [ ] T011 [US1] Confirmar que `PropertyStatus` se persiste con conversiones EF Core y que `UpdatedAt` acepta valor nulo según el modelo.

**Punto de control**: La historia 1 cumple sus criterios de persistencia y estado operativos.

---

## Fase 4: Historia de usuario 2: Seeding automático y assets externos (Prioridad: P1)

**Objetivo**: Ejecutar el seeding sin pasos manuales y dejar disponibles los datos y activos de imagen para runtime.

**Prueba independiente**: Comprobar que la aplicación inicia con migraciones y seeding automáticos, y que los assets de `support/seed-data` se publican y se resuelven desde el manifiesto sin duplicación.

### Verificación de la historia de usuario 2

- [ ] T012 [P] [US2] Ejecutar pruebas de seeding y arranque en `app/backend/tests/RealtorApiTests/` para validar que la operación sigue siendo idempotente tras múltiples ejecuciones.
- [ ] T013 [P] [US2] Comprobar que el manifiesto central y los assets quedan publicados y accesibles en runtime según el contrato de `support/seed-data/seed-manifest.json` y `contracts/seed-manifest.schema.json`.

### Implementación de la historia de usuario 2

- [ ] T014 [US2] Crear el manifiesto central `support/seed-data/seed-manifest.json` y mantener el origen de verdad del seed en esa ubicación con rutas relativas y públicas.
- [ ] T015 [US2] Implementar `DatabaseSeeder` en `app/backend/src/RealtorApi/Infrastructure/Persistence/DatabaseSeeder.cs` con versiones síncrona y asíncrona equivalentes, idempotentes y con `CancellationToken` propagado.
- [ ] T016 [US2] Registrar `UseSeeding()` y `UseAsyncSeeding()` sobre `AppDbContext` en el punto de configuración de `RealtorApi` con delegación a `DatabaseSeeder` y sin lógica duplicada fuera del seeder.
- [ ] T017 [US2] Añadir la extensión de migración `MigrationExtensions` en `app/backend/src/RealtorApi/Infrastructure/Persistence/MigrationExtensions.cs` para ejecutar `MigrateAsync()` siempre al arrancar y evitar la condición por migraciones pendientes.
- [ ] T018 [US2] Actualizar `app/backend/src/RealtorApi/RealtorApi.csproj` para incluir los JSON y assets de `support/seed-data` en el build y la publicación, dejando accesibles las rutas finales para la API.
- [ ] T019 [US2] Revisar `Program.cs` para que llame a `await app.MigrateAsync();` antes de `app.Run();` sin lógica de seed ni migración manual en el arranque.

**Punto de control**: El seeding es automático, idempotente y se ejecuta al iniciar la aplicación con o sin migraciones pendientes nuevas.

---

## Fase 5: Historia de usuario 3: Consultas con imágenes y estados listos para runtime (Prioridad: P2)

**Objetivo**: Preparar la capa de consulta para que `ImageUrl` y los estados de propiedad estén listos para usar en consultas futuras sin rutas de soporte ni conversiones duplicadas.

**Prueba independiente**: Validar que la URL almacenada apunta a una ruta pública servida por la API y que el estado de la propiedad se encuentra preparado para consultas no destructivas.

### Verificación de la historia de usuario 3

- [ ] T020 [P] [US3] Verificar que `ImageUrl` está apuntando a la ruta pública de la API y no a un path físico de `support` en la validación de runtime y tests.
- [ ] T021 [P] [US3] Confirmar que los datos de `Property` están listos para futuras consultas de listado o detalle sin cambiar el esquema de persistencia.

### Implementación de la historia de usuario 3

- [ ] T022 [US3] Revisar el flujo de publicación y resolución del manifest para confirmar que las rutas finales de imagen se exponen a través de la API y no a un valor local del entorno.
- [ ] T023 [US3] Validar que la información de `Property` y `PropertyStatus` está lista para consultas futuras con datos consistentes y estables.

**Punto de control**: La funcionalidad queda lista para consumo del catálogo de propiedades y futuras consultas con imágenes.

---

## Fase 6: Cierre y validación transversal

**Objetivo**: Completar la verificación final de la feature y asegurar que respeta la constitución y los criterios de aceptación.

- [ ] T024 [P] Ejecutar `dotnet build app/backend/src/RealtorApi/RealtorApi.csproj` para confirmar que la feature compila sin errores.
- [ ] T025 [P] Ejecutar `dotnet test app/backend/tests/RealtorApiTests/RealtorApiTests.csproj --logger "console;verbosity=minimal"` para validar migración, seeding e idempotencia.
- [ ] T026 Revisar y cerrar los criterios de aceptación de `spec.md`, `plan.md` y `quickstart.md`, confirmando que la feature cumple `CE-001` a `CE-005`.
- [ ] T027 Verificar que la documentación de la feature y los artefactos de `specs/003-properties-persistence-seeding/` reflejan el estado final y quedan en español conforme a la constitución.

---

## Dependencias y orden de ejecución

### Dependencias entre fases

- **Fase 1**: sin dependencias, prepara la base de la feature.
- **Fase 2**: depende de la preparación; bloquea el dominio y persistencia.
- **Fase 3**: depende del dominio y de la migración; su objetivo principal es persistir la propiedad y su estado.
- **Fase 4**: depende del modelo y del contexto; implementa el seed y la publicación de assets.
- **Fase 5**: depende del mismo modelo y del flujo de publicación; prepara la información para consultas futuras.
- **Fase 6**: depende de que todas las fases previas estén validadas.

### Dependencias entre historias

- **US1 (P1)**: requiere dominio, migración y validación del modelo persistente.
- **US2 (P1)**: depende de la entidad y del contexto; debe seguir el flujo de migración y seeding automático.
- **US3 (P2)**: depende de la salida ya persistida y del manifiesto de assets para asegurar rutas públicas y consultas futuras.

### Oportunidades de paralelismo

- T001 y T002 pueden ejecutarse en paralelo.
- T008 y T009 pueden ejecutarse en paralelo durante la validación del modelo US1.
- T012 y T013 pueden ejecutarse en paralelo durante la validación del seeding US2.
- T020 y T021 pueden ejecutarse en paralelo para la verificación del uso de `ImageUrl` y la preparación de consultas US3.
- T024 y T025 son verificaciones paralelizables si se ejecutan en entornos de trabajo separados, siempre que se compartan los mismos resultados de la feature.

### Estrategia de implementación

1. Completar la preparación y la base del dominio.
2. Implementar HU1 con el modelo persistente y la migración.
3. Implementar HU2 con `DatabaseSeeder`, `UseSeeding`, `UseAsyncSeeding` y publicación de assets.
4. Validar HU3 para confirmar que `ImageUrl` apunta a rutas públicas y que la data queda lista para consultas futuras.
5. Ejecutar la verificación final y cerrar la feature con los criterios de aceptación.
