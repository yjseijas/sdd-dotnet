# Tareas: Actualización de propiedades

**Entrada**: Documentos de diseño de `specs/006-properties-update/`

**Prerrequisitos**: `specs/006-properties-update/plan.md` y
`specs/006-properties-update/spec.md`.

**Verificación**: Se incluyen las pruebas unitarias xUnit solicitadas por
`RF-015` y `plan.md`. La comprobación de HTTP, los límites anteriores al enlace
de parámetros y el rendimiento son manuales y condicionales a un entorno
aislado no productivo. No se crearán pruebas HTTP ni de integración.

**Restricciones**: No agregar dependencias, cambiar el esquema, migraciones,
datos seed, endpoints POST/GET existentes ni `Program.cs` para registrar este
slice individualmente. No agregar limitación de frecuencia en la aplicación,
claves/secretos ni archivos auxiliares como `quickstart.md` o `contracts/`.
Conservar la compatibilidad de creación de 005.

## Fase 1: Preparación

**Objetivo**: Confirmar la estructura y los componentes que se reutilizarán,
sin generar proyectos ni dependencias nuevas.

- [X] T001 Confirmar en `app/Realtor.sln`, `app/backend/src/RealtorApi/RealtorApi.csproj` y `app/backend/tests/RealtorApiTests/RealtorApiTests.csproj` la solución, el backend y la suite xUnit existentes; no crear proyectos ni agregar paquetes.

## Fase 2: Base compartida

**Objetivo**: Habilitar el límite agregado multipart para PUT antes del enlace
de parámetros, preservando la protección existente de POST.

- [X] T002 Ampliar en `app/backend/src/RealtorApi/Features/Properties/CreateProperty/CreatePropertyRequestBodyLimitMiddleware.cs` la selección de rutas para incluir `PUT /api/properties/{id}`; limitar el cuerpo multipart completo a 6.291.456 bytes inclusive antes del model binding, devolver `413 ProblemDetails` ante un byte adicional (también si no hay `Content-Length` confiable), y rebobinar los cuerpos aceptados; conservar sin cambios el límite y comportamiento de `POST` de 005.

**Punto de control**: Las historias pueden usar el límite común de cuerpo; no
se automatiza la verificación HTTP del middleware.

## Fase 3: Historia de usuario 1 — Actualizar sin imagen (Prioridad: P1, MVP)

**Objetivo**: Permitir actualizar los ocho campos de negocio de una propiedad
existente sin exigir imagen ni cambiar la imagen publicada.

**Prueba independiente**: Con pruebas unitarias, comprobar el mapeo de campos,
validación de entradas válidas y actualización sin archivo; `imageUrl` conserva
exactamente su valor anterior, no se invoca almacenamiento y se preservan
`Id`/`CreatedAt`. Comprobar también que prevalece la última actualización
persistida sin detección ni rechazo de concurrencia.

### Implementación

- [X] T003 [P] [US1] Crear `UpdatePropertyRequest` en `app/backend/src/RealtorApi/Features/Properties/UpdateProperty/UpdatePropertyRequest.cs` con los ocho campos editables y `IFormFile? Image`; ligar el GUID exclusivamente desde la ruta y no exponer campos de sistema.
- [X] T004 [US1] Implementar las reglas de entrada de negocio y límites persistentes con FluentValidation en `app/backend/src/RealtorApi/Features/Properties/UpdateProperty/UpdatePropertyValidator.cs`, sin lógica de validación en el slice ni en el mapping.
- [X] T005 [P] [US1] Crear `UpdatePropertyResponse` en `app/backend/src/RealtorApi/Features/Properties/UpdateProperty/UpdatePropertyResponse.cs` con el identificador, los campos de negocio y `imageUrl`, sin entidad EF ni rutas físicas.
- [X] T006 [US1] Implementar el mapping puro entre request, propiedad y response en `app/backend/src/RealtorApi/Features/Properties/UpdateProperty/UpdatePropertyMapping.cs`; no incluir consultas, validación, persistencia ni efectos secundarios.
- [X] T007 [US1] Implementar en `app/backend/src/RealtorApi/Features/Properties/UpdateProperty/UpdatePropertyHandler.cs` la búsqueda por GUID antes de cualquier efecto y la actualización sin archivo: cambiar solo los ocho campos, preservar exactamente `imageUrl`, `Id` y `CreatedAt`, y asignar `UpdatedAt` UTC sin escribir ni eliminar archivos.
- [X] T008 [US1] Implementar `PUT /api/properties/{id}` multipart en `app/backend/src/RealtorApi/Features/Properties/UpdateProperty/UpdatePropertySlice.cs`, responder `200` con la representación explícita actualizada, delegar al handler y usar el descubrimiento automático `ISlice`/`IHandler`, sin registros individuales en `Program.cs`.

### Verificación unitaria

- [X] T009 [P] [US1] Crear pruebas unitarias xUnit en `app/backend/tests/RealtorApiTests/Properties/UpdateProperty/UpdatePropertyValidatorTests.cs` para entradas válidas y fronteras válidas de los campos requeridos.
- [X] T010 [P] [US1] Crear pruebas unitarias xUnit en `app/backend/tests/RealtorApiTests/Properties/UpdateProperty/UpdatePropertyMappingTests.cs` para el mapeo de los ocho campos y response, y comprobar que el input no permite cambiar `Id`, `CreatedAt`, `UpdatedAt` ni `ImageUrl`.
- [X] T011 [US1] Crear pruebas unitarias xUnit en `app/backend/tests/RealtorApiTests/Properties/UpdateProperty/UpdatePropertyHandlerTests.cs` para la actualización sin imagen, conservación exacta de `imageUrl`, ausencia de llamadas al almacenamiento y política de prevalencia de la última actualización persistida; verificar que la limpieza no borra archivos que puedan seguir referenciados y documentar que una imagen desplazada por concurrencia puede quedar huérfana sin barrido global; no introducir token de concurrencia, rechazo de conflicto, `DbSet` simulado ni pruebas HTTP.

**Punto de control**: La edición sin imagen es verificable unitariamente y
conserva los recursos existentes.

## Fase 4: Historia de usuario 2 — Reemplazar la imagen (Prioridad: P2)

**Objetivo**: Admitir imagen PNG/JPEG verificada y reemplazar de forma segura
la URL e imagen previa, con compensación ante fallo de persistencia.

**Prueba independiente**: Las pruebas unitarias comprueban archivo nuevo UUID
sin sobrescritura, URL pública relativa, rollback compensatorio y limpieza
limitada a imagen administrada no compartida; una falla de limpieza posterior
no revierte el éxito.

### Implementación

- [X] T012 [US2] Ampliar el almacenamiento compartido en `app/backend/src/RealtorApi/Features/Properties/CreateProperty/PropertyImageStorage.cs` con eliminación segura basada en URL canónica bajo el directorio administrado y nombre UUID derivado del formato verificado; limitar el borrado a archivos de carga administrados y excluir explícitamente assets seed, además de rechazar traversal, rutas absolutas/externas y rutas fuera del directorio.
- [X] T013 [US2] Extender `app/backend/src/RealtorApi/Features/Properties/UpdateProperty/UpdatePropertyHandler.cs` para reutilizar la validación existente de PNG/JPEG, MIME/extensión y límites inclusivos de 5.242.880 bytes/268.435.456 bytes, escribir la imagen nueva solo después de comprobar existencia, usar UUID/`CreateNew` y persistir solo la URL relativa pública; si falla la persistencia, compensar únicamente el archivo nuevo, y después del guardado limpiar la imagen previa solo si es administrada y no está referenciada por otra propiedad, registrando estructuradamente fallos sin degradar el éxito.
- [X] T014 [US2] Traducir en `app/backend/src/RealtorApi/Features/Properties/UpdateProperty/UpdatePropertySlice.cs` el resultado de imagen inválida de esta actualización a `415 ProblemDetails`, sin alterar la respuesta de creación existente de 005.
- [X] T015 [P] [US2] Crear pruebas unitarias xUnit en `app/backend/tests/RealtorApiTests/Properties/UpdateProperty/PropertyImageStorageUpdateTests.cs` para PNG/JPEG, extensión y MIME coherentes, archivo inválido, límites inclusivos y excesos de tamaño/memoria, UUID, escritura no sobrescribible y rechazo de URL seed (incluso dentro del directorio administrado), externa, traversal o fuera del directorio.
- [X] T016 [US2] Ampliar las pruebas unitarias xUnit de `app/backend/tests/RealtorApiTests/Properties/UpdateProperty/UpdatePropertyHandlerTests.cs` para reemplazo de imagen, fallo de escritura, fallo de persistencia y compensación, exclusión de URL compartida, logging estructurado y éxito persistido aunque falle la limpieza previa.

**Punto de control**: El reemplazo funciona sin sobrescribir recursos y la
limpieza nunca elimina imágenes seed, externas o compartidas.

## Fase 5: Historia de usuario 3 — Rechazar solicitudes inválidas o inexistentes (Prioridad: P3)

**Objetivo**: Devolver errores previsibles y genéricos, sin efectos parciales,
para identificadores, campos, imágenes y límites no aceptables.

**Prueba independiente**: Las pruebas unitarias verifican límites y reglas de
validación, que una propiedad inexistente no cause escrituras, y que errores de
almacenamiento/persistencia mantengan el estado previo y no filtren rutas ni
detalles internos. La comprobación de códigos HTTP se hace manualmente, no con
pruebas HTTP automatizadas.

### Implementación y verificación unitaria

- [X] T017 [P] [US3] Completar en `app/backend/src/RealtorApi/Features/Properties/UpdateProperty/UpdatePropertyValidator.cs` las reglas para texto no vacío, máximos persistentes, decimales compatibles con `decimal(18,2)`, cantidades no negativas y estados permitidos; ampliar `app/backend/tests/RealtorApiTests/Properties/UpdateProperty/UpdatePropertyValidatorTests.cs` con casos inválidos y fronteras.
- [X] T018 [US3] Completar en `app/backend/src/RealtorApi/Features/Properties/UpdateProperty/UpdatePropertySlice.cs` una operación directamente invocable para parsear el GUID de ruta y rechazar un valor malformado con `400 ValidationProblemDetails` antes de llamar al handler; implementar también la conversión directamente invocable de resultados: `400 ValidationProblemDetails` para validación y memoria; `404` para GUID válido inexistente; `413` para exceso individual de archivo; `415` para imagen inválida; y `500 ProblemDetails` genérico para fallos técnicos, sin detalles internos ni rutas físicas.
- [X] T019 [US3] Completar en `app/backend/tests/RealtorApiTests/Properties/UpdateProperty/UpdatePropertyHandlerTests.cs` las pruebas unitarias de GUID inexistente sin llamada a almacenamiento ni mutación, datos inválidos sin efectos, fallos técnicos con estado previo preservado y fallo de compensación registrado estructuradamente.
- [X] T020 [US3] Crear `app/backend/tests/RealtorApiTests/Properties/UpdateProperty/UpdatePropertyErrorMappingTests.cs` con pruebas unitarias directas del mapeo `400`, `404`, `413`, `415` y `500`, incluidos los cuerpos `ProblemDetails`/`ValidationProblemDetails` y el rechazo de GUID malformado antes de invocar el handler; no iniciar servidor ni agregar pruebas HTTP/de integración.

**Punto de control**: Los rechazos no modifican propiedad ni recursos, y las
respuestas no revelan información interna.

## Fase 6: Cierre y aspectos transversales

**Objetivo**: Ejecutar las verificaciones aprobadas, comprobar compatibilidad y
registrar los límites de la validación manual.

- [X] T021 Compilar la solución con `dotnet build app/Realtor.sln`; resolver errores sin incorporar cambios fuera del alcance indicado en `specs/006-properties-update/plan.md`.
- [X] T022 Ejecutar `dotnet test` para `app/backend/tests/RealtorApiTests/RealtorApiTests.csproj`, sin exigir PostgreSQL ni credenciales y sin agregar pruebas HTTP o de integración.
- [ ] T023 En un entorno aislado no productivo disponible, verificar manualmente los casos multipart, códigos y ausencia de efectos descritos en `specs/006-properties-update/plan.md`, incluidos los GUID malformados (`400 ValidationProblemDetails` antes del handler), GUID válidos inexistentes (`404`), límites inclusivos de archivo y memoria, cuerpo agregado de 6.291.456/6.291.457 bytes antes del model binding, limpieza segura y compatibilidad de POST/GET; no automatizar solicitudes HTTP ni alterar seeds. Registrar el resultado en esta tarea de `specs/006-properties-update/tasks.md`; si el entorno no está disponible, indicar que no se ejecutó.
- [ ] T024 Si existe un entorno no productivo aislado, ejecutar manualmente la medición CE-005 según `specs/006-properties-update/plan.md`: 100 solicitudes válidas controladas, 50 sin imagen y 50 con imagen fija de 1 MiB y 1024×768, secuenciales desde el mismo cliente tras cinco solicitudes de calentamiento; registrar en esta tarea de `specs/006-properties-update/tasks.md` entorno, cargas y resultado, y comprobar p95 inferior a un segundo. No crear benchmark ni prueba HTTP automatizada; si no hay entorno, no declarar aprobado el criterio.
- [X] T025 Revisar el diff completo de `app/backend/` y los componentes `app/backend/src/RealtorApi/Features/Properties/UpdateProperty/`, `app/backend/src/RealtorApi/Features/Properties/CreateProperty/` y `app/backend/src/RealtorApi/Program.cs` para confirmar que no se alteraron esquema, migraciones, seeds, POST/GET, contrato de creación, registros individuales, control de concurrencia ni rate limiting de aplicación; confirmar también que no se agregaron paquetes, claves o secretos.
- [X] T026 Ejecutar `git diff --check` y revisar `specs/006-properties-update/tasks.md` para confirmar cobertura RF-001–RF-017 y CE-001–CE-006, rutas válidas, ausencia de archivos auxiliares y que el estado de `specs/006-properties-update/spec.md` permanezca en `En implementación` mientras no existan las validaciones manuales definitivas.

## Dependencias y orden de ejecución

### Dependencias entre fases

- **Preparación (Fase 1)**: T001; no requiere tareas previas.
- **Base (Fase 2)**: T002 depende de T001 y precede a las historias que usan
  el límite agregado.
- **US1 (Fase 3)**: T003 y T005 son independientes; T004 depende de T003;
  T006 depende de T003–T005; T007 depende de request, validator y mapping;
  T008 integra el handler. T009–T011 dependen de los tipos y del
  comportamiento que verifican.
- **US2 (Fase 4)**: Depende de la base y del contrato/handler de US1. T012
  habilita la limpieza segura; T013 depende de T012 y amplía el handler; T014
  traduce el error específico sin cambiar 005; T015 depende de T012 y T016 de
  T013.
- **US3 (Fase 5)**: Depende de US1 y US2 porque completa la validación y los
  resultados de error del mismo slice. T017 precede a la validación final;
  T018 completa el contrato de errores; T019 prueba los rechazos del handler y
  T020 prueba el mapeo de resultados después de T018.
- **Cierre (Fase 6)**: T021–T022 requieren la implementación integrada; T023
  y T024 solo se ejecutan si hay entorno aislado no productivo; T025–T026
  verifican alcance y formato al final.

### Dependencias entre historias

- **US1 (P1)**: Requiere T001–T002; es el MVP y no depende de las otras
  historias.
- **US2 (P2)**: Requiere el contrato y handler iniciales de US1, pues extiende
  esos mismos archivos y el almacenamiento compartido.
- **US3 (P3)**: Completa los errores del slice y las pruebas de rechazo tras
  integrar US1 y US2; sus escenarios se verifican independientemente una vez
  disponible la implementación final.

### Oportunidades de paralelismo

- En US1, T003 y T005 pueden avanzar en paralelo porque modifican archivos
  distintos; T009 y T010 también pueden ejecutarse en paralelo después de
  disponer de sus respectivos tipos/mapping.
- En US2, T015 puede ejecutarse en paralelo con T013 una vez acordada e
  implementada la interfaz de almacenamiento de T012: usan archivos distintos.
- En US3, T017 y T018 afectan archivos distintos y pueden prepararse en
  paralelo tras integrar US2; T019 y T020 son pruebas unitarias en archivos
  distintos que pueden ejecutarse en paralelo una vez completadas sus
  implementaciones.
- No se marca como paralela ninguna tarea que comparta un archivo con otra
  tarea pendiente. Aunque los archivos de US1–US3 sean compartidos, las
  historias se implementan en orden para evitar conflictos y mantener cada
  incremento verificable.

## Estrategia de implementación

### Primero el MVP

Completar T001–T011 para ofrecer actualización válida sin imagen, con campos
editables y `imageUrl` preservada; compilar y ejecutar las pruebas unitarias.
Este es el alcance sugerido para el primer incremento.

### Entrega incremental

Después del MVP, implementar US2 (T012–T016) para reemplazo y limpieza segura;
continuar con US3 (T017–T020) para rechazos y errores. Cerrar con T021–T026,
sin convertir las verificaciones manuales del plan en pruebas HTTP.
