# Especificación de funcionalidad: Actualización de propiedades

**Rama de funcionalidad**: `006-properties-update`

**Creada**: 2026-11-03

**Estado**: En implementación

> El estado de la especificación se modifica automáticamente del flujo Speckit: Borrador → Aprobada → En implementación → Implementada. No editarlo manualmente fuera de ese flujo.

**Entrada**: Descripción del usuario: "Crear la especificación 006-properties-update para definir un nuevo caso de uso de actualización de propiedades mediante un slice Minimal API en backend, con carga opcional de imagen y el comportamiento de imágenes de la especificación 005."

## Clarificaciones

### Sesión 2026-11-03

- P: ¿Qué comportamiento debe observarse si dos solicitudes actualizan la misma propiedad al mismo tiempo? → A: Prevalece la última actualización persistida; no se detecta ni rechaza el conflicto concurrente.
- P: ¿Debe esta funcionalidad imponer un límite de frecuencia propio para solicitudes anónimas? → A: No; no se aplica límite en la aplicación y queda a cargo de infraestructura externa.

## Escenarios de usuario y pruebas *(obligatorio)*

### Historia de usuario 1: Actualizar una propiedad sin imagen (Prioridad: P1)

Como persona que administra el catálogo, quiero actualizar los datos de una propiedad existente sin adjuntar una imagen, para corregir su información sin perder la imagen ya publicada.

**Motivo de prioridad**: La edición de los datos de negocio es el flujo esencial y no debe exigir una imagen ni alterar la imagen existente.

**Prueba independiente**: Verificar unitariamente una actualización válida de una propiedad existente sin archivo y comprobar que se persisten los campos editables y que `imageUrl` conserva exactamente su valor previo.

**Escenarios de aceptación**:

1. **Dado** que existe una propiedad y envío sus datos editables válidos como `multipart/form-data` sin archivo, **Cuando** actualizo la propiedad mediante `PUT /api/properties/{id}`, **Entonces** recibo HTTP 200 con los datos actualizados y `imageUrl` sin cambios.
2. **Dado** que la solicitud no incluye imagen, **Cuando** se completa la actualización, **Entonces** no se reemplaza ni se elimina la imagen existente y no se ejecuta una acción para quitarla.
3. **Dado** que el cliente incluye campos `id`, `CreatedAt`, `UpdatedAt` o `ImageUrl`, **Cuando** se procesa la solicitud, **Entonces** esos valores no son aceptados como datos editables ni alteran los valores administrados por el sistema.

---

### Historia de usuario 2: Reemplazar la imagen de una propiedad (Prioridad: P2)

Como persona que administra el catálogo, quiero adjuntar una imagen nueva y válida al actualizar una propiedad, para mantener vigente su presentación pública.

**Motivo de prioridad**: Reemplazar una imagen es una extensión valiosa del flujo de edición, con controles de seguridad y consistencia que preservan la propiedad y sus recursos.

**Prueba independiente**: Verificar unitariamente una actualización con imagen válida y comprobar que se asigna la URL pública nueva, se preserva el estado ante fallos y solo se limpia una imagen anterior que sea administrada y no compartida.

**Escenarios de aceptación**:

1. **Dado** que existe una propiedad y la solicitud contiene una imagen PNG o JPG/JPEG válida que respeta los límites, **Cuando** se completa la actualización, **Entonces** la imagen se guarda con un nombre UUID generado por el servidor, se persiste la nueva URL pública relativa y la actualización devuelve éxito.
2. **Dado** que la imagen nueva se escribió pero falla la persistencia, **Cuando** termina la operación, **Entonces** se conserva el estado anterior de la propiedad y se intenta eliminar la imagen nueva; si también falla la compensación, se registra un error estructurado y la respuesta sigue siendo un error.
3. **Dado** que se persiste la nueva URL, **Cuando** se intenta limpiar la imagen previa, **Entonces** solo se elimina si está dentro del almacenamiento administrado de propiedades y no está referenciada por otra propiedad; los archivos de seed, externos o compartidos no se eliminan.
4. **Dado** que falla la limpieza de una imagen previa elegible después de guardar la nueva URL, **Cuando** se completa la actualización, **Entonces** la operación y el estado de base de datos siguen siendo exitosos y el fallo se registra estructuradamente.

---

### Historia de usuario 3: Rechazar actualizaciones inválidas o inexistentes (Prioridad: P3)

Como persona que consume el servicio, quiero recibir errores previsibles cuando la propiedad no existe o los datos y archivos no son aceptables, para corregir la solicitud sin dejar cambios parciales ni obtener detalles internos.

**Motivo de prioridad**: Las respuestas inequívocas y la ausencia de efectos ante un rechazo protegen la consistencia de la información y de los recursos de imagen.

**Prueba independiente**: Verificar unitariamente los resultados para identificador inexistente, datos de negocio inválidos, imagen rechazada, límites excedidos y fallos de almacenamiento o persistencia, comprobando el estado y los recursos resultantes.

**Escenarios de aceptación**:

1. **Dado** que `{id}` es un GUID válido pero no identifica una propiedad, **Cuando** envío una solicitud de actualización, **Entonces** recibo HTTP 404 y no ocurre ninguna escritura de imagen, modificación de propiedad ni otra mutación.
2. **Dado** que `{id}` no tiene un formato GUID válido, **Cuando** envío la actualización, **Entonces** recibo HTTP 400 con detalles de validación antes de consultar o modificar la propiedad.
3. **Dado** que la propiedad existe pero los campos de negocio incumplen una regla, **Cuando** envío la actualización, **Entonces** recibo HTTP 400 con errores de validación y no se modifica la propiedad ni se escribe una imagen.
4. **Dado** que la imagen está vacía, usa un formato no permitido, es inválida o no coincide su contenido real con su formato declarado, **Cuando** envío la actualización, **Entonces** recibo HTTP 415 y no se persiste ningún cambio.
5. **Dado** que el archivo supera 5.242.880 bytes o el cuerpo multipart total supera 6.291.456 bytes, **Cuando** envío la actualización, **Entonces** recibo HTTP 413 y no se modifica la propiedad; el límite total se aplica antes del model binding.
6. **Dado** que la imagen válida excede el presupuesto de memoria decodificada de 268.435.456 bytes, **Cuando** envío la actualización, **Entonces** recibo HTTP 400 y no se modifica la propiedad ni se escribe la imagen.
7. **Dado** que falla la escritura de una imagen o la persistencia de la actualización, **Cuando** se procesa la solicitud, **Entonces** recibo HTTP 500 con `ProblemDetails` sin detalles internos ni rutas físicas, y se conserva el estado previo de la propiedad.

### Casos límite

- La solicitud debe usar `multipart/form-data`, tanto si incluye una imagen como si no; `image` es un archivo opcional.
- Se aceptan PNG y JPG/JPEG, con extensiones `.png`, `.jpg` o `.jpeg` sin distinguir mayúsculas de minúsculas, solo cuando el contenido real es una imagen válida y coincide con el formato declarado.
- Un archivo vacío, corrupto, de formato no permitido o con extensión o tipo de contenido incompatible recibe HTTP 415. Esta regla corresponde explícitamente al caso de actualización y difiere de la especificación 005, donde ciertas imágenes inválidas de creación reciben HTTP 400.
- Un archivo de exactamente 5.242.880 bytes y una imagen con costo estimado exactamente de 268.435.456 bytes se aceptan si son válidos; cualquier exceso individual de archivo sobre 5.242.880 bytes recibe HTTP 413, y cualquier exceso del presupuesto de memoria decodificada recibe HTTP 400.
- El cuerpo multipart completo, incluidos archivo, campos, boundaries y sobrecarga, puede medir hasta 6.291.456 bytes inclusive; un byte adicional se rechaza con HTTP 413 antes del model binding. Este límite agregado es independiente del límite de 5.242.880 bytes para el archivo.
- Una solicitud sin archivo conserva exactamente la `imageUrl` previa; no existe una acción de quitar la imagen.
- Una colisión de nombre no sobrescribe archivos: el nombre UUID se genera en el servidor y la escritura no reemplaza archivos existentes.
- Una ruta física nunca forma parte de una respuesta pública ni puede ser suministrada por el cliente.
- Si dos actualizaciones de la misma propiedad se procesan concurrentemente, prevalecen los valores de la última actualización persistida; no se detecta ni se rechaza el conflicto.
- Si dos actualizaciones concurrentes reemplazan la imagen, el archivo de una solicitud puede quedar sin referencias cuando su URL sea reemplazada por la última actualización persistida. Se acepta ese archivo huérfano; esta iniciativa no realiza un barrido global ni elimina archivos cuyo estado de referencias pueda ser concurrente.
- Ante identificador inexistente no se escribe un archivo ni se modifica la propiedad. Antes de efectos externos se validan los datos y se comprueba la existencia de la propiedad.
- Si falla la escritura de la imagen nueva, la base de datos permanece sin cambios. Si falla la persistencia tras escribir la imagen nueva, se intenta retirar únicamente ese archivo nuevo; un fallo de compensación se registra estructuradamente.
- Después de guardar la nueva URL, la limpieza de la imagen previa es posterior y no revierte la operación. Solo aplica a archivos bajo almacenamiento administrado de propiedades no referenciados por otras propiedades; nunca elimina archivos de seed, externos o compartidos.
- La actualización pública y anónima no concede acceso a rutas físicas ni habilita acceso a archivos fuera del almacenamiento público previsto.
- La aplicación no impone un límite de frecuencia propio a las solicitudes de actualización; cualquier limitación corresponde a la infraestructura externa.
- La actualización reutiliza la entidad y persistencia actuales; no agrega entidades ni relaciones ni requiere cambios de esquema.

## Requisitos *(obligatorio)*

### Requisitos funcionales

- **RF-001**: El sistema DEBE ofrecer una actualización pública y anónima de una propiedad existente por identificador mediante `PUT /api/properties/{id}` o una ruta equivalente que preserve esa semántica. Una actualización completada DEBE responder HTTP 200 con la representación actualizada de la propiedad, incluidos sus campos de negocio e `imageUrl`. Si hay actualizaciones concurrentes de una misma propiedad, DEBE prevalecer la última actualización persistida; el sistema NO DEBE detectar ni rechazar el conflicto.
- **RF-002**: El endpoint DEBE recibir `multipart/form-data`, incluso si no se envía imagen, con los campos editables `title`, `description`, `address`, `price`, `status`, `bedroomCount`, `bathroomCount` y `areaSquareMeters`, y un archivo opcional `image`.
- **RF-003**: El sistema DEBE rechazar como no editables los valores enviados por el cliente para `id`, `CreatedAt`, `UpdatedAt` e `ImageUrl`; el identificador de ruta y los valores administrados por el sistema prevalecen.
- **RF-004**: El sistema DEBE exigir `title`, `description` y `address` no vacíos; `price` y `areaSquareMeters` mayores que cero; `bedroomCount` y `bathroomCount` mayores o iguales a cero; y un `status` igual a `Available`, `Rented` o `Maintenance`. Si incumple una regla de negocio, DEBE responder HTTP 400 con `ValidationProblemDetails` y no producir efectos de actualización.
- **RF-005**: Antes de modificar datos o escribir archivos, el sistema DEBE validar que el identificador de ruta tenga formato GUID y buscar la propiedad. Un identificador malformado DEBE responder HTTP 400 con detalles de validación, antes de consultar o modificar la propiedad. Si el GUID es válido pero no existe la propiedad, DEBE responder HTTP 404 sin efectos de escritura sobre archivos o persistencia.
- **RF-006**: Si la solicitud no incluye `image`, el sistema DEBE conservar exactamente el valor previo de `imageUrl`, sin modificarlo ni eliminar la imagen y sin ofrecer una acción implícita de quitarla.
- **RF-007**: Si la solicitud incluye una imagen, el sistema DEBE aceptar únicamente contenido PNG o JPG/JPEG válido y coherente con la extensión y el tipo de contenido declarados. Un archivo vacío, un formato no permitido, contenido real inválido o discrepancia de formato DEBE responder HTTP 415 con `ProblemDetails`, sin cambios en la propiedad.
- **RF-008**: El sistema DEBE aceptar un archivo individual de hasta 5.242.880 bytes inclusive y rechazar un archivo mayor con HTTP 413 y `ProblemDetails`. La estimación de memoria decodificada DEBE ser como máximo 268.435.456 bytes inclusive; si es superior, DEBE responder HTTP 400 con errores de validación. El exceso de memoria no debe generar escritura ni actualización.
- **RF-009**: El sistema DEBE limitar el cuerpo multipart completo, incluidos campos y sobrecarga, a 6.291.456 bytes inclusive y rechazar cualquier exceso con HTTP 413 y `ProblemDetails` antes del model binding. El control agregado DEBE ser independiente del límite de tamaño del archivo.
- **RF-010**: Para una imagen válida, el sistema DEBE usar un nombre UUID generado por el servidor y la extensión derivada del formato verificado, almacenarla bajo el directorio público dedicado a propiedades publicado como `/assets/properties/` y evitar toda sobrescritura. La respuesta y la persistencia DEBEN contener la ruta pública relativa y NUNCA una ruta física.
- **RF-011**: Al completar la actualización, el sistema DEBE persistir los campos editables y la nueva `imageUrl` si se recibió una imagen válida. Una colisión de nombre DEBE resolverse sin sobrescribir el archivo existente.
- **RF-012**: Si falla la escritura de la imagen nueva, el sistema DEBE responder HTTP 500 con `ProblemDetails` genérico y mantener sin cambios la propiedad. Si falla la persistencia después de escribir la imagen nueva, DEBE conservarse el estado previo y el sistema DEBE intentar eliminar únicamente el archivo nuevo; cualquier fallo de compensación DEBE registrarse estructuradamente. Los errores internos y las rutas físicas NO DEBEN revelarse.
- **RF-013**: Después de persistir una nueva URL, el sistema DEBE intentar eliminar la imagen anterior únicamente si está dentro del almacenamiento administrado de propiedades y no está referenciada por otra propiedad. NO DEBE eliminar archivos de seed, externos o compartidos. Un fallo de limpieza DEBE registrarse estructuradamente y NO DEBE revertir la actualización exitosa ni el estado actualizado en base de datos. Por la política de última actualización persistida, un archivo nuevo que quede sin referencias debido a una actualización concurrente puede permanecer huérfano; esta iniciativa NO DEBE ejecutar un barrido global de archivos.
- **RF-014**: La actualización DEBE seguir la arquitectura Vertical Slice existente en el alcance de esta funcionalidad: un slice, handler, mapping y validator propios; descubrimiento y registro automáticos mediante `ISlice` e `IHandler`; y mapping puro, sin validación ni lógica de negocio.
- **RF-015**: La validación y los errores DEBEN integrarse con la infraestructura de FluentValidation y `ProblemDetails` vigente. Las pruebas automatizadas de esta funcionalidad DEBEN limitarse a pruebas unitarias; no se incluyen pruebas HTTP ni de integración.
- **RF-016**: La actualización DEBE reutilizar la entidad y persistencia de propiedades y conservar compatibilidad con las especificaciones 001–005, incluido el contrato del listado y el comportamiento de creación. NO DEBE introducir cambios de esquema.
- **RF-017**: La aplicación NO DEBE imponer un límite de frecuencia propio a las solicitudes de actualización; la limitación de solicitudes, si se configura, corresponde a la infraestructura externa.

### Entidades principales *(si la funcionalidad maneja datos)*

- **Solicitud de actualización de propiedad**: Datos editables de la propiedad —título, descripción, dirección, precio, estado, dormitorios, baños y superficie— con una imagen opcional. El identificador se obtiene de la ruta; los campos de auditoría e imagen no son controlables por el cliente.
- **Propiedad existente**: Registro persistido que se identifica por el tipo de identificador vigente y cuyos campos de negocio se actualizan. Mantiene sus valores y `imageUrl` previos si la operación no se confirma.
- **Imagen de propiedad**: Archivo PNG o JPG/JPEG validado, sujeto a límites de archivo, cuerpo y memoria decodificada, publicado mediante una URL relativa bajo `/assets/properties/` y asociado a una propiedad.

## Criterios de éxito *(obligatorio)*

### Resultados medibles

- **CE-001**: El 100 % de las actualizaciones válidas sin imagen actualiza los campos de negocio y conserva exactamente la `imageUrl` previa, sin crear ni eliminar archivos.
- **CE-002**: El 100 % de las actualizaciones válidas con imagen dentro de los límites guarda la nueva imagen sin sobrescribir archivos y persiste su URL pública relativa; la imagen previa solo se elimina cuando cumple las condiciones de propiedad exclusiva y almacenamiento administrado. Un archivo nuevo que quede huérfano por concurrencia puede permanecer sin limpieza global.
- **CE-003**: El 100 % de las solicitudes rechazadas por identificador malformado, validación, imagen inválida, tamaño, memoria o propiedad inexistente devuelve el código establecido (400, 415, 413 o 404, según corresponda) y no produce cambios parciales.
- **CE-004**: El 100 % de los fallos de escritura o persistencia devuelve HTTP 500 sin revelar rutas físicas o detalles internos; cuando falla la persistencia después de escribir la imagen nueva, el estado previo de la propiedad se mantiene y se intenta compensar el archivo.
- **CE-005**: En una muestra de 100 solicitudes válidas, el percentil 95 del tiempo de finalización es inferior a 1 segundo. El entorno de prueba y el tamaño de las imágenes de carga, cuando se incluyan, se mantendrán controlados y se documentarán en el plan posterior.
- **CE-006**: La propiedad actualizada continúa siendo compatible con las consultas y el contrato público definidos en las especificaciones 001–005, sin cambios de esquema.

## Supuestos

- El tipo de `{id}` es el identificador existente de la entidad propiedad; esta funcionalidad no cambia dicho tipo.
- La solicitud usa `multipart/form-data` sin imagen para conservarla; no se incorpora una acción para retirar una imagen.
- La atomicidad requiere validar los datos y localizar la propiedad antes de efectos externos, escribir la imagen nueva antes de persistir su URL, y compensar el archivo nuevo si falla la persistencia. La limpieza de una imagen previa elegible ocurre después de persistir y no revierte el éxito.
- Se consideran imágenes previas eliminables únicamente las que pertenezcan al almacenamiento administrado de propiedades y no estén referenciadas por otra propiedad; los assets de seed, externos y compartidos se preservan.
- Las dependencias funcionales son las especificaciones [001](../001-realtor-solution-foundation/spec.md), [002](../002-foundation-backend/spec.md), [003](../003-properties-persistence-seeding/spec.md), [004](../004-properties-list-pagination/spec.md) y [005](../005-properties-create/spec.md).
- Las pruebas automatizadas quedan limitadas a pruebas unitarias por la constitución vigente; no se aprueban pruebas HTTP ni de integración en esta especificación.
- El plan posterior documentará el entorno controlado y el tamaño de carga usado para verificar el criterio de latencia.
