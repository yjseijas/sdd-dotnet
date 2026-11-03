# Plan de implementación: Actualización de propiedades

**Rama**: `006-properties-update` | **Fecha**: 2026-11-03 | **Especificación**: [spec.md](./spec.md)

**Entrada**: Especificación de `/specs/006-properties-update/spec.md`.

## Resumen

Agregar `PUT /api/properties/{id}` como un slice Minimal API auto-descubrible
para actualizar una propiedad existente con `multipart/form-data`. La solicitud
solo contiene los ocho campos de negocio editables y una imagen opcional. Se
reutilizan `Property`, `AppDbContext`, FluentValidation, el almacenamiento
ImageSharp y la publicación de recursos ya establecidos por `005-properties-create`.
Sin imagen se conserva exactamente `imageUrl`; con imagen, se escribe primero
un archivo nuevo seguro, se persiste la propiedad y luego se intenta limpiar
la imagen anterior solo si es administrada y no está compartida. No se cambian
el esquema, los seeds, la creación ni el listado existentes.

## Contexto técnico

**Lenguaje/versión**: C# y .NET 11; SDK fijado en `global.json` como
`11.0.100-rc.1.26425.128`.

**Dependencias principales**: ASP.NET Core Minimal APIs, `ISlice`/`IHandler`,
FluentValidation, EF Core, Npgsql, PostgreSQL, `ProblemDetails`, `ILogger`,
`IPropertyImageStorage` y SixLabors.ImageSharp existentes.

**Persistencia**: La entidad `Property` y `AppDbContext` actuales. No se agregan
entidades, relaciones, migraciones, cambios de esquema ni cambios a datos seed.

**Pruebas**: xUnit; solo pruebas unitarias. No se crearán pruebas HTTP,
`WebApplicationFactory`, pruebas de integración ni pruebas contra PostgreSQL/
Testcontainers. No se exigirán credenciales ni una base de datos activa para
compilar o ejecutar la suite unitaria.

**Plataforma objetivo**: Backend `RealtorApi`; almacenamiento existente bajo
`wwwroot/assets/properties`, publicado como `/assets/properties/`.

**Tipo de proyecto**: API web ASP.NET Core Minimal API, organizada por slices
verticales. No hay cambios en Blazor.

**Objetivos de rendimiento**: El percentil 95 de 100 solicitudes válidas debe
ser inferior a 1 segundo. El procedimiento define un entorno no productivo y
datos de carga controlados en «Validación manual y rendimiento». Los límites
de imagen y cuerpo son inclusivos y se aplican antes de decodificar o enlazar
parámetros, respectivamente.

**Restricciones**: No controllers, nueva dependencia, cambios en `Program.cs`
para registro individual, modificación de esquema o seed, pruebas HTTP/
integración, limitación de frecuencia propia, exposición de rutas físicas ni
aceptación de campos de sistema desde el cliente. La limpieza de la imagen
anterior nunca revierte una actualización ya persistida.

**Escala/alcance**: Un endpoint `PUT`, una propiedad por solicitud, cero o una
imagen y actualización de los campos editables existentes.

## Comprobación de la constitución

### Punto de control previo a investigación

- [x] Se conserva Minimal API, el patrón `ISlice` y la organización vertical;
  no se proponen controllers.
- [x] No hay cambios en la interfaz Blazor; solicitud/respuesta son contratos
  propios del caso de uso y no exponen la entidad EF Core.
- [x] Se reutilizan el registro automático, el almacenamiento y la configuración
  de recursos estáticos existentes; no se agrega una capa, proyecto ni dependencia.
- [x] RF-001–RF-017 y CE-001–CE-006 son verificables; RF-015 limita las pruebas
  automatizadas a pruebas unitarias.
- [x] Se identifican validación, límites de carga/memoria, seguridad de rutas,
  concurrencia, errores, rollback compensatorio y logging estructurado.
- [x] Este plan y el resto de la documentación de la iniciativa están en español.
- [x] Las decisiones no fijadas están resueltas aquí mediante los patrones y
  restricciones existentes; no queda ningún `NEEDS CLARIFICATION`.

**Gate previo**: aprobado. No hay contradicción constitucional que requiera
enmienda o aprobación adicional.

## Investigación local y decisiones

La investigación se limita a la spec, la constitución, las instrucciones de
backend y la implementación/plan de `005-properties-create`; las decisiones se
basan en esos recursos locales. No se requiere investigación externa ni se
despachan agentes. Las decisiones de investigación, diseño y contrato se
consolidan en este archivo para respetar la lista constitucional de artefactos.

### Componentes y compatibilidad

- **Decisión**: Reutilizar `IPropertyImageStorage`/`PropertyImageStorage`,
  `CreatePropertyRequestBodyLimitMiddleware`, la publicación estática y el
  pipeline centralizado existentes. El slice y el handler nuevos se registran
  por descubrimiento automático; no se añaden registros individuales ni
  dependencias en `Program.cs`.
- **Fundamento**: `005-properties-create` ya verifica formatos con ImageSharp,
  estima memoria decodificada, crea nombres UUID con `FileMode.CreateNew`, usa
  `/assets/properties/` y compensa una escritura si falla la persistencia.
  La configuración actual limita el cuerpo de creación antes del enlace de
  parámetros; se
  ampliará su selección de rutas para incluir el `PUT` sin retirar el control
  de `POST`.
- **Alternativa descartada**: Duplicar el decoder, almacenamiento, publicación
  o middleware dentro de `UpdateProperty` produciría reglas divergentes y
  configuración duplicada.

### Imagen, contrato de errores y licencia

- `SixLabors.ImageSharp` ya está referenciado por la iniciativa 005
  (versión actual `4.1.3`). Se reutiliza el componente y la configuración
  existente: **no agregar ni actualizar paquetes**. La configuración de
  licencia disponible para `Debug` solo se considera utilizable en entornos no
  productivos; este plan no afirma autorización de producción ni introduce
  claves o secretos.
- Las imágenes admitidas son PNG y JPEG; extensión verificada `.png`, `.jpg`
  o `.jpeg` y tipo MIME deben corresponder al formato detectado. El nombre
  nuevo es UUID generado por el servidor más la extensión del formato
  verificado; la creación no sobrescribe un nombre existente.
- El servicio compartido ya distingue el archivo demasiado grande
  (`413`), el presupuesto de memoria (`400`) y la imagen inválida
  (`property.image_invalid`). Para conservar el contrato de creación de 005,
  el slice de actualización traducirá únicamente su resultado de imagen
  inválida a `415`; memoria excedida seguirá siendo `400`, y archivo excedido
  seguirá siendo `413`. No se cambiará el comportamiento HTTP de creación.
- **Alternativa descartada**: Hacer que el componente compartido cambie
  globalmente los errores de imagen inválida a `415` rompería la compatibilidad
  explícita de creación.

### Cuerpo, validación y orden de efectos

- El control agregado existente se aplica al `PUT /api/properties/{id}` antes
  del enlace de parámetros multipart. Rechaza un `Content-Length` mayor de
  6.291.456 bytes y, si falta o no es confiable, lee como máximo el límite más
  un byte. Un cuerpo de hasta 6.291.456 bytes inclusive se rebobina y queda
  disponible al enlazador;
  un byte adicional produce `413 ProblemDetails` sin invocar el endpoint.
  El cómputo incluye archivo, campos, boundaries y sobrecarga multipart, y es
  independiente del máximo individual de archivo, 5.242.880 bytes inclusive.
- La validación automática de FluentValidation comprueba los campos de negocio
  antes del handler. El handler localiza primero la propiedad mediante el
  identificador de ruta; si no existe, responde `404` sin mutar base de datos,
  escribir ni borrar archivos. No se almacena la imagen antes de comprobar
  existencia.
- Una solicitud debe ser multipart incluso sin archivo. El enlazador solo expone
  los ocho campos permitidos y `image`; `id`, `CreatedAt`, `UpdatedAt` e
  `ImageUrl` no forman parte del contrato de entrada ni se vinculan como datos:
  no se aceptan como fuentes editables y no prevalecen sobre la ruta o el estado
  administrado por el servidor.
- Reglas: `title`, `description` y `address` no vacíos; `price` y
  `areaSquareMeters` mayores que cero; cantidades de dormitorios y baños
  mayores o iguales a cero; `status` exactamente `Available`, `Rented` o
  `Maintenance`. Se respetan además las restricciones persistentes existentes
  (`Title` hasta 200 caracteres, `Address` hasta 500 y decimales `18,2`).

### Persistencia, limpieza y consistencia

- Sin imagen, conservar byte por byte/valor por valor la `imageUrl` previa y no
  llamar a almacenamiento o eliminación. `CreatedAt` y `Id` se conservan;
  `UpdatedAt` se establece en UTC al actualizar.
- Con imagen: validar y escribir el archivo nuevo sin sobrescritura; asignar su
  ruta pública relativa y guardar los campos mediante `AppDbContext`. Si falla
  la escritura, no guardar la propiedad. Si falla la persistencia después de
  escribir, mantener el estado de propiedad anterior e intentar borrar solo el
  nuevo archivo. Una falla de compensación se registra con `ILogger` y
  propiedades estructuradas, y la respuesta sigue siendo un `500
  ProblemDetails` genérico sin ruta física ni detalle interno.
- Después de persistir correctamente la nueva URL, considerar la limpieza de
  la URL anterior. Solo es elegible una ruta canónica del directorio
  administrado `/assets/properties/`, resoluble dentro del directorio físico
  administrado y no referenciada por ninguna otra propiedad. La verificación
  de referencias excluye el registro actualizado. Rechazar traversal,
  separadores codificados/ambiguos, rutas absolutas, rutas externas y rutas
  fuera del directorio; no aceptar una ruta del cliente. No borrar recursos
  seed, externos o compartidos. La eliminación segura usa el servicio de
  almacenamiento y no confía en un path físico recibido desde HTTP.
- Si falla la limpieza posterior, registrar el error estructuradamente, pero
  conservar el éxito HTTP 200 y el estado ya persistido. No intentar revertir
  la propiedad o la nueva imagen por ese fallo.
- Las actualizaciones concurrentes no tienen token ni detección de conflicto:
  los valores de la última persistencia exitosa prevalecen. No se añade control
  de concurrencia. No se impone límite de frecuencia en la aplicación; queda a
  cargo de infraestructura externa.
- **Alternativa descartada**: Transacción de base de datos que pretenda incluir
  el sistema de archivos no hace atómicos ambos recursos; se usa compensación
  explícita para la escritura nueva y limpieza posterior no bloqueante para la
  imagen anterior.

## Contrato de interfaz

### `PUT /api/properties/{id}`

**Entrada**: `multipart/form-data`, requerido aunque `image` se omita.
`id` es el GUID de la propiedad existente y proviene solo de la ruta.

| Campo | Tipo/validación |
|---|---|
| `title` | Texto requerido; respeta el máximo persistente de 200 caracteres. |
| `description` | Texto requerido y no vacío. |
| `address` | Texto requerido; respeta el máximo persistente de 500 caracteres. |
| `price` | Decimal requerido, mayor que cero y compatible con `decimal(18,2)`. |
| `status` | Uno de `Available`, `Rented` o `Maintenance`. |
| `bedroomCount` | Entero requerido, mayor o igual a cero. |
| `bathroomCount` | Entero requerido, mayor o igual a cero. |
| `areaSquareMeters` | Decimal requerido, mayor que cero y compatible con `decimal(18,2)`. |
| `image` | Archivo opcional; PNG/JPEG verificado, hasta 5.242.880 bytes y con costo estimado de decodificación hasta 268.435.456 bytes. |

`id`, `CreatedAt`, `UpdatedAt` e `ImageUrl` no son campos editables. No existe
una acción para retirar una imagen.

| HTTP | Respuesta |
|---|---|
| `200` | Representación explícita de la propiedad actualizada: `id`, ocho campos de negocio e `imageUrl` (incluido sin cambio si no se envió archivo). |
| `400` | `ValidationProblemDetails` para reglas de negocio o presupuesto de memoria decodificada superior a 268.435.456 bytes. |
| `404` | `ProblemDetails` para GUID sin propiedad; sin efectos de escritura/eliminación. |
| `413` | `ProblemDetails` si el archivo excede 5.242.880 bytes o el cuerpo completo excede 6.291.456 bytes antes del enlace de parámetros. |
| `415` | `ProblemDetails` para archivo vacío, formato no permitido, corrupto o discrepancia entre contenido, extensión y MIME. |
| `500` | `ProblemDetails` genérico ante fallos de escritura o persistencia; sin excepción, ruta física ni detalle interno. |

Las respuestas no exponen rutas físicas. Una imagen nueva se expresa como ruta
relativa pública `/assets/properties/{uuid}.{ext}`. La actualización no altera
el contrato del listado ni el de creación.

## Modelo de datos y estado

No se introducen entidades, relaciones, tablas, columnas ni cambios a
migraciones o seeds. Se reutiliza la entidad `Property` y su esquema vigente.

- **Solicitud `UpdatePropertyRequest`**: campos de negocio y `IFormFile? Image`;
  el GUID se liga desde la ruta, no desde el cuerpo.
- **Propiedad existente**: mantiene `Id` y `CreatedAt`; actualiza los ocho
  campos de negocio y `UpdatedAt`. Sin archivo, preserva `ImageUrl`; con archivo
  válido, reemplaza `ImageUrl` por la ruta relativa nueva.
- **Imagen almacenada**: recurso transitorio asociado por `ImageUrl`, con
  nombre servidor UUID, contenido verificado y almacenamiento público dedicado.
- **Transiciones**: no existe estado intermedio persistido. Antes de guardar,
  el estado previo es la fuente de verdad; un error de persistencia lo conserva.
  Después del guardado, la propiedad nueva es exitosa aunque falle la limpieza
  de la imagen anterior.

## Diseño y estructura

### Artefactos de documentación

La constitución permite `spec.md`, `plan.md`, `tasks.md` y el checklist de
requisitos autorizado. En esta fase solo se crea/actualiza `plan.md`; los
hallazgos de investigación, datos, contrato, diseño y guía de validación están
consolidados aquí. No se crearán `research.md`, `data-model.md`,
`contracts/`, `quickstart.md`, tareas u otros Markdown auxiliares.

### Estructura de código propuesta

```text
app/backend/src/RealtorApi/
├── Features/Properties/
│   ├── CreateProperty/                 # existente; mantener el contrato 005
│   └── UpdateProperty/
│       ├── UpdatePropertyRequest.cs
│       ├── UpdatePropertyResponse.cs
│       ├── UpdatePropertyValidator.cs
│       ├── UpdatePropertyMapping.cs
│       ├── UpdatePropertyHandler.cs
│       └── UpdatePropertySlice.cs
├── Features/Properties/CreateProperty/
│   ├── PropertyImageStorage.cs          # reutilizar; añadir solo capacidad
│   │                                     # segura necesaria para eliminar URL
│   └── CreatePropertyRequestBodyLimitMiddleware.cs # ampliar target PUT
└── Program.cs                            # sin registros individuales nuevos

app/backend/tests/RealtorApiTests/Properties/UpdateProperty/
├── UpdatePropertyValidatorTests.cs
├── UpdatePropertyMappingTests.cs
├── UpdatePropertyHandlerTests.cs
└── PropertyImageStorageUpdateTests.cs
```

Los nombres son estructura de diseño, no tareas de implementación. No se
crearán archivos vacíos: cada archivo existirá solo si el slice lo necesita.
Los slices/handlers/validators nuevos se incorporan mediante los scanners y
el filtro global existentes. No se mueve ni se refactoriza el slice 005 fuera
de los ajustes de reutilización indispensables.

### Responsabilidades

- `UpdatePropertyRequest`: binder multipart explícito que expone únicamente los
  campos de entrada permitidos y el archivo opcional.
- `UpdatePropertyValidator`: reglas de dominio/longitud del contrato, usando
  FluentValidation y el filtro automático; sin validación manual en endpoint.
- `UpdatePropertyMapping`: mapeo explícito de los campos editables y de
  respuesta; sin queries, validación, persistencia o efectos secundarios.
- `UpdatePropertyHandler`: cargar por GUID, devolver `404` antes de efectos,
  coordinar almacenamiento, asignación, persistencia, compensación y limpieza
  posterior mediante `AppDbContext` y el componente de almacenamiento.
- `UpdatePropertySlice`: mapear método/ruta, delegar al `IHandler`, traducir
  `Result<T>` y el error `property.image_invalid` a los contratos HTTP de esta
  feature. No contiene lógica de negocio.
- Middleware existente: aplicar el límite agregado al PUT antes de model
  binding y conservar el mismo control para POST.

## Pruebas unitarias

Todas las pruebas automatizadas se ubicarán por slice en
`app/backend/tests/RealtorApiTests/Properties/UpdateProperty/` y usarán xUnit.
No se levantarán hosts ASP.NET, `HttpClient`, `WebApplicationFactory`,
Testcontainers ni PostgreSQL; no se escribirán pruebas HTTP/integración.

- Validator: casos válidos/fronteras e inválidos para campos requeridos,
  cantidades, estado y límites de persistencia.
- Mapeo: todos los campos editables, aislamiento de `Id`/auditoría/`ImageUrl`
  del input y respuesta con la URL correcta.
- Almacenamiento: reuso/adaptación de pruebas existentes para PNG/JPEG,
  extensión/MIME/contenido no coincidentes, vacío/corrupto, límites inclusivos
  y excesos de 5.242.880 y 268.435.456 bytes, UUID, `CreateNew`, ruta pública
  y contención segura del path. Comprobar que los rechazos no escriben archivos.
- Handler: sin archivo conserva exactamente URL/archivo; inexistente no llama
  al almacenamiento; imagen válida guarda URL; fallo de escritura no persiste;
  fallo de persistencia intenta retirar solo el archivo nuevo y conserva la
  propiedad; fallo de compensación produce log estructurado y error; tras
  persistencia se limpia solo archivo administrado no compartido; fallos de
  limpieza no degradan el éxito. Las pruebas no simularán `DbSet` ni el
  proveedor LINQ de EF Core; se empleará únicamente el patrón de aislamiento
  unitario ya aceptado por las pruebas de 005, sin conexión a una base de datos
  de desarrollo o productiva.
- Middleware/límites HTTP: no se automatiza el pipeline HTTP, en cumplimiento
  de RF-015; los límites pre-binding y status codes se revisan manualmente
  según los escenarios siguientes.

No se actualizará una tarea o spec ni se afirmará cobertura no ejecutada. La
compilación y las pruebas unitarias no dependen de credenciales de base de
datos. Si una verificación requiere una API y una base local que no estén
disponibles, se documentará como no ejecutada y no se exigirá
crear/configurar credenciales para esta fase.

## Validación manual y rendimiento

Las comprobaciones de extremo a extremo y de latencia son manuales y
condicionales a disponer de un entorno **aislado no productivo**. No implican
pruebas HTTP automatizadas ni requieren una base de datos activa o credenciales
cuando no estén disponibles. No modificar datos iniciales para preparar el
entorno; usar
registros ya existentes en un entorno desechable o preparación manual fuera del
repositorio.

1. Ejecutar `dotnet build app/Realtor.sln` y
   `dotnet test app/backend/tests/RealtorApiTests/RealtorApiTests.csproj`.
   Resultados esperados: compilación correcta y suite unitaria completa
   aprobada, sin conexión a PostgreSQL.
2. Con API y base de datos de desarrollo disponibles, comprobar `PUT` multipart
   sin imagen (`200`, campos actualizados y URL previa sin cambios), con imagen
   (ruta pública UUID y contenido servido), GUID inexistente (`404` sin
   escrituras), validación de negocio (`400`), imagen inválida (`415`), exceso
   de memoria (`400`) y fallos técnicos (`500` genérico).
3. En el control de cuerpo anterior al enlace de parámetros, verificar que un
   cuerpo de 6.291.456 bytes se acepta por el límite agregado y uno de
   6.291.457 bytes se rechaza con `413 ProblemDetails` antes de ejecutar el
   enlazador o handler. Verificar también el límite inclusivo de archivo de
   5.242.880 bytes y el de memoria decodificada de 268.435.456 bytes; los
   excesos deben producir `413` y `400`, respectivamente.
4. Verificar intentos de limpieza con URL administrada exclusiva, URL de seed,
   externa, compartida, traversal y ruta fuera del directorio. Solo la primera
   se elimina. Forzar fallo de persistencia para observar compensación del
   archivo nuevo; forzar fallo al limpiar el anterior para confirmar éxito
   persistido con log y respuesta exitosa.
5. Para CE-005, documentar sistema operativo, CPU/RAM, versión SDK/runtime,
   configuración de compilación, ubicación/versión de base de datos,
   almacenamiento y ubicación del cliente. En la misma configuración no
   productiva, usar 100 propiedades
   preparadas en un entorno aislado, una solicitud por propiedad y ejecución
   secuencial desde el mismo cliente; alternar 50 multipart sin archivo y 50
   multipart con una imagen válida fija de 1 MiB y 1024×768. Medir de extremo
   a extremo
   después de cinco solicitudes de calentamiento; p95 de las 100 medidas debe
   ser <1 s. Registrar el resultado y las cargas utilizadas. Si no hay entorno
   o base de datos disponible, no ejecutar ni declarar aprobado esta medición y
   no exigir credenciales.

El criterio de aceptación es total: el 100 % de los comportamientos y límites
especificados debe aprobar sus verificaciones aplicables; los fallos no pueden
dejar una actualización parcial. La validación manual y de rendimiento no
sustituye las pruebas unitarias ni autoriza añadir pruebas HTTP.

## Comprobación de la constitución posterior al diseño

- [x] El diseño mantiene Minimal API, slice/handler/validator auto-descubiertos,
  mapeo explícito y ausencia de controllers.
- [x] El contrato HTTP permanece separado de `Property`; no hay cambios de UI.
- [x] Se reutilizan EF Core/Npgsql/ImageSharp, configuración de recursos,
  almacenamiento y límites existentes; no se agrega dependencia ni abstracción
  global especulativa.
- [x] La estrategia automatizada se limita a pruebas unitarias, sin HTTP ni
  integración; la validación manual y de rendimiento queda condicionada al
  entorno.
- [x] Se fijan respuestas, validaciones, límites inclusivos, seguridad de ruta,
  orden de efectos, compensación, limpieza y logging estructurado.
- [x] No hay cambios de esquema, migración, seed, creación, listado o límite de frecuencia
  de aplicación.
- [x] Todos los artefactos documentales de esta fase están en español y solo se
  genera `plan.md`; no se generan artefactos auxiliares.

**Gate posterior al diseño**: aprobado. No hay desviaciones constitucionales ni
decisiones pendientes que bloqueen el paso posterior de planificación.

## Registro de complejidad

| Decisión o complejidad | Necesidad que resuelve | Alternativa más simple descartada y motivo |
|---|---|---|
| Escribir nueva imagen, persistir y compensar explícitamente | Coordinar consistencia entre base de datos y sistema de archivos sin transacción común | Guardar primero el registro podría dejar una URL rota si falla el archivo; una transacción SQL no incluye el filesystem. |
| Eliminar imagen anterior solo tras persistencia y comprobar propiedad exclusiva | Evitar pérdida de recursos todavía referenciados y no revertir una actualización exitosa por limpieza | Borrar antes de persistir puede romper la propiedad ante rollback; borrar cualquier URL podría afectar seeds, externos o archivos compartidos. |
| Aplicar límite agregado previo al enlace de parámetros y máximo individual por separado | Proteger memoria y respetar los dos límites independientes especificados | Confiar solo en el límite multipart del servidor no limita necesariamente el cuerpo completo ni garantiza `ProblemDetails` antes del enlace de parámetros. |
