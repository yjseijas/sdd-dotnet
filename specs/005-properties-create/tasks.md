# Tareas: Alta de propiedades

**Entrada**: `specs/005-properties-create/spec.md` y `specs/005-properties-create/plan.md`

**Prerrequisitos**: SDK de `global.json`; licencia y clave de ImageSharp verificadas y configuradas fuera del repositorio antes de agregar el paquete.

**Verificación**: Se requieren pruebas unitarias conforme a RF-015 y al plan. No agregar pruebas HTTP con `WebApplicationFactory` ni pruebas de integración. Ejecutar compilación y pruebas unitarias, revisar `Up`, `Down` y el snapshot de EF Core, y contrastar el resultado con los criterios de aceptación.

**Organización**: Preparación, base compartida, historias en prioridad P1–P3 y cierre. Las tareas de cada historia corresponden a su propio incremento verificable.

## Fase 1: Preparación

**Objetivo**: Confirmar las condiciones del entorno antes de cambiar el backend o agregar la dependencia de imágenes.

- [X] T001 [P] Verificar que el SDK requerido por `global.json` está disponible para compilar `app/Realtor.sln`.
- [X] T002 [P] Confirmar elegibilidad/licencia de ImageSharp y disponibilidad segura de su clave fuera del repositorio antes de modificar `app/backend/src/RealtorApi/RealtorApi.csproj`; no guardar la clave en el repositorio.

## Fase 2: Base compartida

**Objetivo**: Habilitar `ImageUrl` nulo y mantener compatible la lectura existente antes de completar las historias.

- [X] T003 [P] Cambiar `Property.ImageUrl` a `string?` en `app/backend/src/RealtorApi/Domain/Properties/Property.cs`, sin alterar los demás campos de dominio.
- [X] T004 [P] Configurar `ImageUrl` como nullable en `app/backend/src/RealtorApi/Infrastructure/Persistence/Configurations/PropertyConfiguration.cs`, sin modificar otras restricciones.
- [X] T005 Compilar `app/backend/src/RealtorApi/RealtorApi.csproj` después de T003 y T004 y, solo si compila, crear una única migración `20261101144300_MakePropertyImageUrlNullable` en `app/backend/src/RealtorApi/Infrastructure/Persistence/Migrations/20261101144300_MakePropertyImageUrlNullable.cs` y su archivo `app/backend/src/RealtorApi/Infrastructure/Persistence/Migrations/20261101144300_MakePropertyImageUrlNullable.Designer.cs`; actualizar `app/backend/src/RealtorApi/Infrastructure/Persistence/Migrations/AppDbContextModelSnapshot.cs` para cambiar únicamente la nulabilidad de `ImageUrl`.
- [X] T006 [P] Adaptar `app/backend/src/RealtorApi/Features/Properties/GetPropertiesQuery.cs` para aceptar `ImageUrl` nulo y preservar el contrato del GET existente: URL pública absoluta cuando hay imagen y cadena vacía cuando no la hay, sin agregar pruebas HTTP o de integración.

## Fase 3: Historia de usuario 1 — Crear una propiedad sin imagen (Prioridad: P1) — MVP

**Objetivo**: Permitir el alta sin archivo y persistir/devolver `imageUrl` como nulo, sin ruta ficticia.

**Prueba independiente**: Las pruebas unitarias de validación y mapeo verifican los campos requeridos y la representación con `imageUrl` nulo; revisar el handler confirma que persiste explícitamente el valor nulo. No agregar pruebas HTTP ni de integración.

### Implementación y verificación

- [X] T007 [P] [US1] Crear el contrato multipart de entrada en `app/backend/src/RealtorApi/Features/Properties/CreateProperty/CreatePropertyRequest.cs` con los ocho campos requeridos y la imagen opcional.
- [X] T008 [P] [US1] Crear la respuesta de alta en `app/backend/src/RealtorApi/Features/Properties/CreateProperty/CreatePropertyResponse.cs`, incluyendo `id`, los campos de propiedad y `imageUrl` nullable.
- [X] T009 [US1] Implementar en `app/backend/src/RealtorApi/Features/Properties/CreateProperty/CreatePropertyValidator.cs` las reglas RF-003: valores no vacíos, precio y superficie mayores que cero, cantidades no negativas y estado limitado a `Available`, `Rented` o `Maintenance`.
- [X] T010 [US1] Añadir pruebas unitarias para los valores válidos e inválidos de RF-003 en `app/backend/tests/RealtorApiTests/Properties/CreateProperty/CreatePropertyValidatorTests.cs`.
- [X] T011 [US1] Implementar los mapeos explícitos del slice en `app/backend/src/RealtorApi/Features/Properties/CreateProperty/CreatePropertyMapping.cs`, limitándolos a transformar solicitud, datos y respuesta.
- [X] T012 [US1] Añadir pruebas unitarias de los mapeos, incluida la respuesta con `imageUrl` nulo, en `app/backend/tests/RealtorApiTests/Properties/CreateProperty/CreatePropertyMappingTests.cs`.
- [X] T013 [US1] Implementar `IHandler` en `app/backend/src/RealtorApi/Features/Properties/CreateProperty/CreatePropertyHandler.cs` para crear la propiedad con `ImageUrl = null`, `CreatedAt` UTC y `UpdatedAt` nulo, usando `AppDbContext` y sin registrar individualmente el handler.
- [X] T014 [US1] Implementar `ISlice` y `POST /api/properties` en `app/backend/src/RealtorApi/Features/Properties/CreateProperty/CreatePropertySlice.cs`; aceptar multipart sin archivo, devolver HTTP 201 con la respuesta creada y dejar que los registros automáticos existentes descubran el slice y el handler.

## Fase 4: Historia de usuario 2 — Crear una propiedad con imagen pública (Prioridad: P2)

**Objetivo**: Validar imágenes PNG/JPEG, almacenarlas con nombre generado y persistir una ruta pública relativa sin romper el listado existente.

**Prueba independiente**: Las pruebas unitarias del almacenamiento verifican formatos válidos, contenido corrupto, extensión y MIME, límites inclusivos de 5 MiB y 256 MiB, nombre no controlado por el cliente y escritura segura; revisar la integración confirma limpieza ante fallo de persistencia y publicación en `/assets/properties/`.

### Implementación y verificación

- [X] T015 [US2] Agregar `SixLabors.ImageSharp` a `app/backend/src/RealtorApi/RealtorApi.csproj` solo después de completar T002 y configurar la licencia/clave de forma segura fuera del repositorio.
- [X] T016 [US2] Implementar en `app/backend/src/RealtorApi/Features/Properties/CreateProperty/PropertyImageStorage.cs` la identificación y decodificación asíncronas con `CancellationToken`, validación de contenido/formato/extensión/MIME, estimación de memoria antes de decodificar, límite de tamaño, nombre generado sin sobrescritura y almacenamiento en `app/backend/src/RealtorApi/wwwroot/assets/properties`.
- [X] T017 [US2] Añadir pruebas unitarias para `PropertyImageStorage` en `app/backend/tests/RealtorApiTests/Properties/CreateProperty/PropertyImageStorageTests.cs`, incluyendo PNG/JPEG aceptados, archivo vacío/corrupto, límites exactos de 5.242.880 y 268.435.456 bytes, rechazo de valores superiores y nombre seguro.
- [X] T018 [P] [US2] Integrar el almacenamiento de imagen con `CreatePropertyHandler` en `app/backend/src/RealtorApi/Features/Properties/CreateProperty/CreatePropertyHandler.cs`: persistir `/assets/properties/{nombre-generado}` solo tras guardar la imagen y, si falla la persistencia, intentar eliminar exactamente el archivo recién creado.
- [X] T019 [P] [US2] Configurar en `app/backend/src/RealtorApi/Program.cs` la publicación estática exclusiva de `wwwroot/assets/properties` bajo `/assets/properties`, creando el directorio antes de configurar su proveedor y conservando los proveedores de seed existentes.

## Fase 5: Historia de usuario 3 — Recibir errores claros ante datos o archivos inválidos (Prioridad: P3)

**Objetivo**: Rechazar entradas inválidas con errores de validación, separar el exceso de tamaño como HTTP 413 y devolver errores internos genéricos sin filtrar detalles.

**Prueba independiente**: Las pruebas unitarias cubren errores de validación, exceso de tamaño, fallos de almacenamiento/persistencia y compensación cuando pueda aislarse sin simular `DbSet` ni el proveedor LINQ de EF Core; no se automatizan solicitudes HTTP.

### Implementación y verificación

- [X] T020 [US3] Implementar `app/backend/src/RealtorApi/Features/Properties/CreateProperty/CreatePropertyRequestBodyLimitMiddleware.cs` y conectarlo en `app/backend/src/RealtorApi/Program.cs` antes del model binding de `POST /api/properties`: limitar el cuerpo multipart completo a 6.291.456 bytes inclusive, rechazar `Content-Length` superior sin leerlo y, en los demás casos, medir y almacenar como máximo el límite más un byte antes de continuar; devolver HTTP 413 con `ProblemDetails` antes de invocar el endpoint si se excede. En `app/backend/src/RealtorApi/Features/Properties/CreateProperty/CreatePropertySlice.cs`, mapear los archivos mayores de 5.242.880 bytes a HTTP 413 con `ProblemDetails`, los errores de datos, formato y memoria a HTTP 400 con `ValidationProblemDetails`, y los fallos de almacenamiento/persistencia a HTTP 500 con `ProblemDetails` genérico sin rutas físicas ni detalles internos.
- [X] T021 [US3] Añadir pruebas unitarias del mapeo de resultados de error y de los estados/cuerpos esperados en `app/backend/tests/RealtorApiTests/Properties/CreateProperty/CreatePropertySliceTests.cs`, sin iniciar un servidor ni ejecutar pruebas HTTP.
- [X] T022 [P] [US3] Ampliar las pruebas unitarias de entradas inválidas y límites de RF-003 en `app/backend/tests/RealtorApiTests/Properties/CreateProperty/CreatePropertyValidatorTests.cs`, verificando mensajes asociados a los campos inválidos.
- [X] T023 [P] [US3] Añadir en `app/backend/tests/RealtorApiTests/Properties/CreateProperty/CreatePropertyHandlerTests.cs` pruebas unitarias de compensación tras fallo de persistencia y de registro estructurado si falla también la limpieza, solo si pueden aislarse sin simular `DbSet` ni el proveedor LINQ de EF Core.

## Fase 6: Cierre y aspectos transversales

**Objetivo**: Validar compilación, pruebas unitarias, migración y conformidad con los criterios aprobados.

- [X] T024 Ejecutar `dotnet build app/Realtor.sln` y resolver cualquier error de compilación introducido por esta funcionalidad.
- [X] T025 Ejecutar las pruebas unitarias nuevas con `dotnet test app/backend/tests/RealtorApiTests/RealtorApiTests.csproj --filter FullyQualifiedName~CreateProperty` y registrar el resultado; no ejecutar ni agregar pruebas HTTP/de integración como validación de esta funcionalidad.
- [X] T026 [P] Revisar `Up`, `Down` y el snapshot en `app/backend/src/RealtorApi/Infrastructure/Persistence/Migrations/20261101144300_MakePropertyImageUrlNullable.cs`, `app/backend/src/RealtorApi/Infrastructure/Persistence/Migrations/20261101144300_MakePropertyImageUrlNullable.Designer.cs` y `app/backend/src/RealtorApi/Infrastructure/Persistence/Migrations/AppDbContextModelSnapshot.cs`: `Up` solo vuelve nullable `ImageUrl`; verificar que `Down` vuelve a exigirlo, documentar que puede fallar si ya existen valores nulos y no introducir pérdida de datos ni defaults ficticios. No ejecutar manualmente `dotnet ef database update`.
- [X] T027 Contrastar `specs/005-properties-create/spec.md` y `specs/005-properties-create/plan.md` con los cambios de `app/backend/src/RealtorApi/Features/Properties/CreateProperty/`, `app/backend/src/RealtorApi/Features/Properties/GetPropertiesQuery.cs`, `app/backend/src/RealtorApi/Program.cs` y `app/backend/src/RealtorApi/Infrastructure/Persistence/Migrations/`; comprobar los criterios de las tres historias, la compatibilidad del GET, la ausencia de registro individual en `Program.cs` y que no se añadieron pruebas HTTP/de integración ni archivos de documentación auxiliares. En una ejecución manual, enviar un cuerpo multipart de exactamente 6.291.456 bytes y comprobar que el límite agregado no lo rechaza; enviar otro de 6.291.457 bytes y comprobar HTTP 413 con `ProblemDetails` antes del model binding, sin invocar el handler ni crear la propiedad.

## Dependencias y orden de ejecución

### Dependencias entre fases

- **Preparación**: T001 y T002 son independientes. T015 requiere completar T002 antes de agregar ImageSharp.
- **Base compartida**: T003 y T004 pueden ejecutarse en paralelo. T005 requiere ambos. T006 requiere T003 y puede ejecutarse en paralelo con T005.
- **US1**: depende de la base compartida. T007 y T008 pueden ejecutarse en paralelo; T009 sigue a T007 y T010 a T009; T011 requiere T007 y T008 y T012 sigue a T011; T013 integra los contratos y la validación; T014 sigue a T013.
- **US2**: depende de US1 porque amplía el mismo handler y endpoint. T016 requiere T015; T017 sigue a T016. T018 y T019 requieren T016 y pueden ejecutarse en paralelo, en archivos distintos.
- **US3**: depende de US1 y US2 porque completa el mapeo de errores del mismo endpoint. T020 sigue a T019 porque ambas tareas modifican `Program.cs`; T020 precede a T021. T022 puede ejecutarse en paralelo con T020/T021; T023 depende de T018 y usa un archivo de pruebas distinto.
- **Cierre**: T024 y T025 verifican todo el incremento; ejecutar T025 después de T024. T026 puede revisarse en paralelo con la compilación/pruebas tras T005. T027 es la revisión final después de las fases anteriores.

### Dependencias entre historias

- **US1 (P1)**: requiere la base compartida; no requiere ImageSharp.
- **US2 (P2)**: requiere US1 para extender el handler y el endpoint ya creados, además de la licencia configurada antes de agregar ImageSharp.
- **US3 (P3)**: completa las respuestas de error de los componentes introducidos por US1 y US2.

### Oportunidades de paralelismo

- Preparación: T001 con T002.
- Base: T003 con T004; después, T005 con T006.
- US1: T007 con T008.
- US2: T018 con T019, una vez completada T016.
- US3: T022 con T020/T021; T023 puede avanzar en paralelo porque modifica un archivo de pruebas separado.
- Cierre: T026 con T024/T025 una vez generada la migración.
- No asignar en paralelo tareas que escriban el mismo archivo; en particular, respetar el orden de las ampliaciones de `CreatePropertyHandler.cs`, `CreatePropertySlice.cs` y `CreatePropertyValidatorTests.cs`.

## Estrategia de implementación

### Primero el MVP: US1

1. Completar la preparación y la nulabilidad de `ImageUrl`, incluyendo compatibilidad del GET.
2. Implementar los contratos, validación, mapeos, handler y slice de creación sin imagen.
3. Ejecutar las pruebas unitarias de validación y mapeo, y comprobar que el alta representa y persiste `imageUrl` como nulo.

### Entrega incremental

1. Completar US1 como incremento mínimo útil.
2. Agregar el almacenamiento/publicación de imagen y verificar sus límites y formatos con pruebas unitarias para US2.
3. Completar los errores de validación, exceso de tamaño, fallos internos y compensación con pruebas unitarias para US3.
4. Ejecutar compilación, suite unitaria y revisión final de migración/criterios.

### Alcance de verificación

- Pruebas exclusivamente unitarias, conforme a RF-015 y al plan.
- No agregar `WebApplicationFactory`, pruebas HTTP automatizadas ni pruebas de integración.
- Las verificaciones manuales de respuestas HTTP y publicación descritas en el plan no se convierten en tareas de automatización.
- El `Down` de la migración vuelve a requerir `ImageUrl`; si ya existen valores nulos podría no aplicarse. Revisar y registrar ese riesgo sin inventar defaults, borrar datos ni ejecutar una actualización manual de base de datos.
