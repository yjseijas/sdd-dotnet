# Plan de implementación: Alta de propiedades

**Rama**: `005-properties-create` | **Fecha**: 2026-11-01 | **Especificación**: [spec.md](./spec.md)

**Entrada**: Especificación de funcionalidad de `/specs/005-properties-create/spec.md`

## Resumen

Añadir `POST /api/properties` como slice Minimal API auto-descubrible, con entrada `multipart/form-data`, validación FluentValidation y handler `IHandler` para persistir la propiedad. La imagen será opcional; cuando se incluya, se validará su contenido PNG/JPEG, se guardará con nombre generado en `wwwroot/assets/properties` y se publicará su ruta relativa. El modelo y la migración permitirán `ImageUrl` nulo, preservando el contrato vigente del listado.

## Contexto técnico

**Lenguaje/versión**: C# con .NET 11; SDK fijado en `global.json` (`11.0.100-rc.1.26425.128`).

**Dependencias principales**: ASP.NET Core Minimal APIs, FluentValidation, EF Core, `AppDbContext`, `ILogger`, auto-registro existente `ISlice`/`IHandler` y `SixLabors.ImageSharp` para identificar y decodificar imágenes.

**Persistencia**: PostgreSQL mediante EF Core/Npgsql. Requiere una migración coherente que solo permita `ImageUrl` nulo; los registros y seeds existentes conservan sus rutas.

**Pruebas**: xUnit y pruebas unitarias únicamente, conforme a RF-015 y la constitución. No se añadirán pruebas HTTP con `WebApplicationFactory` ni pruebas de integración. Las pruebas cubrirán validación, mapeos y almacenamiento de archivos; no se simularán `DbSet` ni el proveedor LINQ de EF Core.

**Plataforma objetivo**: Backend `RealtorApi` bajo ASP.NET Core. El directorio público de assets debe existir y ser escribible en los entornos donde se ejecute el alta.

**Tipo de proyecto**: API web Minimal API con EF Core y publicación de assets estáticos; no hay cambios de interfaz Blazor en esta funcionalidad.

**Objetivos de rendimiento**: Sin objetivos cuantitativos adicionales en la spec. Rechazar archivos mayores a 5.242.880 bytes antes de decodificarlos o escribirlos; usar operaciones asíncronas y propagar `CancellationToken`.

**Restricciones**: No usar controllers, repositorios genéricos, registros individuales de slice/handler en `Program.cs`, rutas suministradas por el cliente, rutas físicas en respuestas ni lógica de archivos en mapeos. La respuesta del listado existente no cambia.

**Escala/alcance**: Un caso de uso de alta; una imagen opcional por solicitud. Autenticación, edición, borrado y reemplazo de imágenes quedan fuera del alcance.

## Comprobación de la constitución

### Punto de control previo a investigación

- [x] Minimal API y organización vertical; no se proponen controllers.
- [x] No hay cambios de UI. El contrato de alta permanece separado de la entidad persistente.
- [x] Se reutiliza el registro existente; la única dependencia nueva se justifica para validar el contenido de imágenes.
- [x] Requisitos verificables; RF-015 limita las pruebas automatizadas a pruebas unitarias.
- [x] Se identifican validación, carga anónima, límites de tamaño, nombres seguros, publicación pública y manejo de errores.
- [x] La documentación de la iniciativa se mantiene en español.
- [x] Las decisiones no fijadas se justifican en este plan y se evita ampliar el alcance.

## Investigación y decisiones

### Decodificación de imagen

**Decisión**: Usar `SixLabors.ImageSharp` para detectar el formato y decodificar el contenido, además de contrastar extensión y tipo MIME declarados. La imagen original validada se guarda sin transformaciones.

**Fundamento**: La BCL no ofrece decodificación PNG/JPEG multiplataforma suficiente para rechazar contenido corrupto. La documentación oficial de ImageSharp recomienda identificar primero la imagen y limitar su costo para entradas no confiables. Antes de decodificar se comprobará que la estimación de memoria de los píxeles no supere 256 MiB; la identificación y decodificación usarán APIs asíncronas con `CancellationToken`, y el archivo original se almacenará sin transformaciones.

**Licencia**: El usuario aceptó ImageSharp bajo su licencia aplicable. La información oficial indica que la licencia depende del tipo de uso y de la condición de la organización, y que se requiere una clave para compilar. Antes de agregar la dependencia, confirmar la elegibilidad/licencia y disponer de la clave mediante configuración segura del entorno; no se debe guardar la clave en el repositorio. Referencias: [licenciamiento de Six Labors](https://sixlabors.com/pricing/), [términos de licencia](https://sixlabors.com/pricing/license) y [consideraciones de seguridad de ImageSharp](https://docs.sixlabors.com/articles/imagesharp/security.html).

**Alternativas consideradas**:

- `SkiaSharp`: se descarta para esta iniciativa porque requiere gestionar dependencias nativas de despliegue y el usuario eligió ImageSharp.
- Validar solo firmas o encabezados: se descarta porque no demuestra que el contenido completo sea una imagen decodificable y no detecta de forma suficiente archivos corruptos.

### Persistencia de archivos y compensación

- Antes del model binding, aplicar al `POST /api/properties` un límite agregado inclusivo de 6.291.456 bytes al cuerpo HTTP completo, incluidos archivo, campos, boundaries y encabezados multipart. Un middleware de la aplicación, situado antes del binding, debe rechazar inmediatamente un `Content-Length` superior y, para los demás casos, leer y almacenar como máximo el límite más un byte antes de continuar; si se excede, debe responder HTTP 413 con `ProblemDetails` sin invocar el endpoint ni crear la propiedad. Cuando el cuerpo no exceda el máximo, debe dejarse disponible desde el inicio para el model binding. No usar `MultipartBodyLengthLimit` como único control del total, ya que no representa el límite agregado del cuerpo. La configuración del servidor o proxy no debe interceptar estas solicitudes con un error que omita el `ProblemDetails` requerido.
- Después del parseo multipart y antes de decodificar o escribir la imagen, comprobar que el archivo individual no supere 5.242.880 bytes; un exceso responde HTTP 413 con `ProblemDetails`. Este control es independiente del límite agregado de 6 MiB.
- Determinar el formato real por el contenido, cotejarlo con `.png`, `.jpg` o `.jpeg` y el MIME correspondiente, y rechazar archivos vacíos, incompatibles o corruptos mediante `ValidationProblemDetails`.
- Usar `Image.Identify()` y su estimación de memoria decodificada antes de cargar píxeles. Rechazar con HTTP 400 cualquier archivo cuya estimación supere 268.435.456 bytes; no se define un límite independiente de dimensiones.
- No decodificar ni escribir archivos que excedan su límite individual; mantener separado ese control del middleware que limita el cuerpo total antes del model binding.
- Generar el nombre en el servidor (GUID más extensión derivada del formato detectado) y crear el archivo sin sobrescritura; nunca usar el nombre original como ruta.
- Guardar primero la imagen y luego la propiedad. Si falla la persistencia, intentar eliminar exactamente el archivo recién escrito; registrar estructuradamente cualquier error de limpieza y devolver HTTP 500 genérico.
- Publicar solo el directorio previsto. Crear `wwwroot/assets/properties` antes de configurar su `PhysicalFileProvider` y añadir la ruta estática `/assets/properties`; mantener sin cambios los proveedores de assets de seed existentes.

## Contratos y modelo de datos

### Solicitud `POST /api/properties`

`multipart/form-data` con los campos `title`, `description`, `address`, `price`, `status`, `bedroomCount`, `bathroomCount`, `areaSquareMeters` y `image` opcional. `status` acepta los valores exactos `Available`, `Rented` o `Maintenance`. Una imagen debe medir como máximo 5.242.880 bytes y su costo estimado de memoria decodificada no puede superar 268.435.456 bytes.

| Respuesta | Cuerpo |
|---|---|
| 201 | Propiedad creada: `id`, campos recibidos y `imageUrl` nullable. |
| 400 | `ValidationProblemDetails` para datos, imagen inválida o presupuesto estimado de memoria decodificada excedido. |
| 413 | `ProblemDetails` cuando el archivo exceda 5.242.880 bytes o el cuerpo multipart total exceda 6.291.456 bytes. |
| 500 | `ProblemDetails` genérico para fallos de almacenamiento o persistencia; sin detalle interno. |

No se define un `Location` ni se altera el contrato de `GET /api/properties`.

### Entidad persistente y relaciones

No se agregan entidades ni relaciones. Se reutiliza `Property` y sus restricciones actuales (`Title` hasta 200 caracteres, `Address` hasta 500, `ImageUrl` hasta 500 y valores decimales `18,2`). `ImageUrl` pasa a ser `string?` y nullable en EF Core. La migración no modifica los valores existentes ni el seeding. La creación asigna `CreatedAt` en UTC y `UpdatedAt` nulo, conforme al modelo actual.

`GetPropertiesQuery` deberá aceptar `ImageUrl` nullable y conservar su respuesta actual: URL absoluta para una ruta disponible y cadena vacía si no hay imagen. La respuesta del alta sí expresa `imageUrl` nulo.

## Diseño y estructura

### Documentación de la iniciativa

La constitución permite únicamente `spec.md`, `plan.md` y `tasks.md`, además del checklist aprobado de requisitos. Por ello, investigación, contrato, modelo y guía de validación se integran en este plan; no se crearán archivos Markdown auxiliares ni `contracts/`.

### Código fuente

```text
app/backend/src/RealtorApi/
├── Domain/Properties/Property.cs
├── Features/Properties/
│   ├── CreateProperty/
│   │   ├── CreatePropertyRequest.cs
│   │   ├── CreatePropertyResponse.cs
│   │   ├── CreatePropertyValidator.cs
│   │   ├── CreatePropertyHandler.cs
│   │   ├── CreatePropertyMapping.cs
│   │   └── CreatePropertySlice.cs
│   ├── GetPropertiesQuery.cs
│   └── PropertiesSlice.cs
├── Infrastructure/Persistence/
│   ├── Configurations/PropertyConfiguration.cs
│   └── Migrations/
├── wwwroot/assets/properties/
├── Program.cs
└── RealtorApi.csproj

app/backend/tests/RealtorApiTests/Properties/CreateProperty/
└── pruebas unitarias del validator, mapeo y almacenamiento de imágenes
```

**Decisión de estructura**: `CreatePropertySlice` define el mapping HTTP; el handler coordina ImageSharp, el sistema de archivos y `AppDbContext`. El validator y el mapping permanecen separados. El slice y el handler se descubren con los registries existentes; `Program.cs` solo recibe el ajuste necesario para publicar el directorio de imágenes, sin registrar individualmente el endpoint ni el handler.

### Guía de validación integrada

Prerequisitos: SDK definido en `global.json`, licencia/clave de ImageSharp configurada fuera del repositorio y conexión de base de datos configurada según `appsettings`.

1. Ejecutar `dotnet build app/Realtor.sln`.
2. Ejecutar `dotnet test app/backend/tests/RealtorApiTests/RealtorApiTests.csproj`.
3. Las pruebas unitarias cubrirán límites de validación de datos, formatos y extensiones admitidos, contenido corrupto, ausencia de imagen, el tamaño inclusivo de 5.242.880 bytes, la estimación inclusiva de 268.435.456 bytes, rechazo de valores superiores, nombre generado y limpieza del archivo tras un fallo de persistencia cuando sea aislable sin pruebas de integración.
4. Revisar la migración y el snapshot: el cambio `Up` solo vuelve nullable `ImageUrl`; comprobar su `Down` y que no introduce cambios en otras columnas. El `Down` volverá a exigir un valor y puede fallar si ya existen propiedades creadas sin imagen; no eliminar datos automáticamente.
5. En una ejecución manual con base de datos configurada, verificar las respuestas 201/400/413/500 y que `/assets/properties/{nombre-generado}` sirve la imagen. Verificar además que un cuerpo multipart de exactamente 6.291.456 bytes no sea rechazado por el límite agregado y que uno de 6.291.457 bytes responda HTTP 413 con `ProblemDetails` antes del model binding, sin invocar el handler ni crear la propiedad. Estas comprobaciones no autorizan agregar pruebas de integración automatizadas.

## Comprobación de la constitución posterior al diseño

- [x] La estructura sigue siendo Minimal API vertical y no introduce controllers ni registros manuales de handlers.
- [x] No se propone UI ni se mezclan contratos con la entidad persistente.
- [x] ImageSharp responde al requisito de verificar contenido real; la licencia y la clave quedan como prerrequisito explícito.
- [x] La migración está limitada a la nulabilidad aprobada y se revisará antes de cerrar su tarea.
- [x] La estrategia automatizada sigue limitada a pruebas unitarias; las validaciones manuales se identifican como tales.
- [x] Se especifican límites de carga, rutas públicas, tratamiento de fallos y compatibilidad con el listado.
- [x] Solo se propone `plan.md` como artefacto nuevo de esta fase.

## Registro de complejidad

| Decisión o complejidad | Necesidad que resuelve | Alternativa más simple descartada y motivo |
|---|---|---|
| ImageSharp con verificación de licencia | Decodificar realmente PNG/JPEG y rechazar corrupción | Revisar solo extensiones o firmas no valida el contenido; SkiaSharp añade dependencias nativas |
| Nombre único y compensación de archivo | Evitar sobrescrituras, rutas del cliente y archivos huérfanos | Usar el nombre original o conservar el archivo cuando falla la base de datos incumple RF-006 y la aclaración |
| Hacer nullable `ImageUrl` y actualizar la lectura existente | Permitir altas sin imagen sin romper el contrato del listado | Guardar cadena vacía o ruta ficticia contradice RF-004 |
| Publicar un directorio dedicado | Servir cargas del usuario sin exponer directorios físicos ajenos | Reutilizar el proveedor de seed mezcla archivos dinámicos con assets de datos iniciales |
