# Especificación de funcionalidad: Alta de propiedades

**Rama de funcionalidad**: `005-properties-create`

**Creada**: 2026-10-31

**Estado**: Borrador

> El estado de la especificación se modifica automáticamente del flujo Speckit: Borrador → Aprobada → En implementación → Implementada. No editarlo manualmente fuera de ese flujo.

**Entrada**: Descripción del usuario: "Definir un caso de uso público para dar de alta propiedades mediante un slice Minimal API, con carga opcional de imagen y persistencia de su URL pública."

## Clarificaciones

### Sesión 2026-11-01

- Q: ¿Qué hacer con una imagen ya guardada si falla la persistencia de la propiedad? → A: Intentar eliminarla y registrar el fallo si la limpieza también falla.
- Q: ¿Cuál es el presupuesto máximo de memoria decodificada para imágenes cargadas? → Sin respuesta específica; se adopta el valor predeterminado recomendado de 256 MiB estimados para limitar el riesgo de agotamiento de recursos.

## Escenarios de usuario y pruebas *(obligatorio)*

### Historia de usuario 1: Crear una propiedad sin imagen (Prioridad: P1)

Quiero registrar una propiedad con sus datos principales aunque todavía no tenga una imagen, para que quede disponible en el catálogo sin exigir un archivo.

**Motivo de prioridad**: La creación de una propiedad es el resultado principal del caso de uso; la imagen es opcional y no debe bloquear el alta.

**Prueba independiente**: Enviar una solicitud válida sin archivo de imagen y comprobar la respuesta de creación y que la propiedad persistida tenga `imageUrl` nulo.

**Escenarios de aceptación**:

1. **Dado** que proporciono los datos requeridos de una propiedad válida sin archivo, **Cuando** envío la solicitud de creación, **Entonces** recibo HTTP 201 con la propiedad creada y `imageUrl` nulo.
2. **Dado** que se crea una propiedad sin imagen, **Cuando** consulto su registro persistido, **Entonces** `imageUrl` sigue siendo nulo y no se asigna una ruta ficticia, vacía ni a un archivo de seed.

---

### Historia de usuario 2: Crear una propiedad con imagen pública (Prioridad: P2)

Quiero adjuntar una imagen válida al crear una propiedad, para que quede publicada y pueda ser consumida por clientes del catálogo.

**Motivo de prioridad**: La imagen enriquece la ficha y su consumo posterior, sin ser requisito para registrar la propiedad.

**Prueba independiente**: Crear una propiedad con una imagen PNG o JPG válida de hasta 5 MB y con un costo estimado de memoria decodificada que no supere 256 MiB; comprobar que el archivo queda en la ubicación pública acordada, que la respuesta identifica la propiedad y que `imageUrl` corresponde a la ruta pública del archivo.

**Escenarios de aceptación**:

1. **Dado** que envío una propiedad válida y una imagen PNG o JPG válida cuyo tamaño no supera 5 MB y cuyo costo estimado de memoria decodificada no supera 256 MiB, **Cuando** se completa el alta, **Entonces** recibo HTTP 201 y la imagen queda disponible bajo `/assets/properties/`.
2. **Dado** que la imagen se almacena correctamente, **Cuando** se persiste la propiedad y se devuelve su representación, **Entonces** `imageUrl` contiene la ruta pública relativa `/assets/properties/{nombre-de-archivo}` y no una ruta física del servidor.
3. **Dado** que la propiedad creada aparece posteriormente en el listado existente, **Cuando** el cliente consulta `GET /api/properties`, **Entonces** el listado conserva el contrato vigente de presentar la imagen como URL pública absoluta.

---

### Historia de usuario 3: Recibir errores claros ante datos o archivos inválidos (Prioridad: P3)

Quiero conocer qué dato o archivo impide el alta, para corregir la solicitud sin crear una propiedad incompleta ni recibir detalles internos del servidor.

**Motivo de prioridad**: Rechazar solicitudes inválidas con respuestas previsibles protege la consistencia del catálogo y permite a los clientes corregir sus entradas.

**Prueba independiente**: Enviar solicitudes separadas con datos inválidos, formatos no permitidos, archivos corruptos, tamaños o presupuestos de memoria superiores a sus límites; comprobar el código y los detalles de error, y verificar que no se crea una propiedad.

**Escenarios de aceptación**:

1. **Dado** que falta un dato requerido o incumple una regla de validación, **Cuando** envío el alta, **Entonces** recibo HTTP 400 con `ValidationProblemDetails` asociado a los campos inválidos y no se crea una propiedad.
2. **Dado** que adjunto un archivo que no es una imagen PNG o JPG válida o que supera el presupuesto de memoria decodificada, **Cuando** envío el alta, **Entonces** recibo HTTP 400 con detalles de validación y no se crea una propiedad.
3. **Dado** que el archivo adjunto supera 5 MB, **Cuando** envío el alta, **Entonces** recibo HTTP 413 con `ProblemDetails` y no se crea una propiedad.
4. **Dado** que no se puede almacenar la imagen o persistir la propiedad, **Cuando** se procesa el alta, **Entonces** recibo HTTP 500 con `ProblemDetails`, sin exponer rutas físicas ni detalles internos y sin recibir una respuesta de éxito.

### Casos límite

- Una imagen cuyo tamaño sea exactamente 5 MB se acepta si su formato es válido; cualquier tamaño superior se rechaza.
- Una imagen cuyo costo estimado de memoria decodificada sea exactamente 256 MiB se acepta si su formato es válido; si supera ese presupuesto, se rechaza como inválida mediante HTTP 400.
- Se aceptan extensiones `.png`, `.jpg` y `.jpeg`, sin distinguir mayúsculas de minúsculas, siempre que el contenido corresponda a una imagen válida del formato declarado.
- Un archivo vacío, corrupto, con extensión o tipo de contenido que no corresponda a PNG/JPG, o de otro formato, se rechaza como inválido.
- La solicitud sin archivo sigue el mismo caso de uso y persiste explícitamente `imageUrl` como `null`.
- Los nombres originales no deben permitir sobrescritura de imágenes existentes ni convertirse en rutas fuera de la ubicación pública de propiedades.
- Ante un fallo de almacenamiento de imagen, no se debe crear una propiedad que afirme tener una imagen disponible.
- Si la imagen se guardó pero falla la persistencia de la propiedad, se intenta eliminar el archivo; si también falla la limpieza, se registra el error y se mantiene la respuesta HTTP 500.
- La creación es pública y no requiere autenticación de aplicación; el endpoint no concede acceso a rutas físicas ni a archivos fuera del directorio público previsto.
- La ruta de creación `POST /api/properties` convive con el endpoint de consulta `GET /api/properties` definido en la especificación 004.

## Requisitos *(obligatorio)*

### Requisitos funcionales

- **RF-001**: El sistema DEBE ofrecer el caso de uso de alta mediante `POST /api/properties`, disponible sin autenticación de aplicación.
- **RF-002**: El endpoint DEBE recibir en una solicitud `multipart/form-data` los campos `title`, `description`, `address`, `price`, `status`, `bedroomCount`, `bathroomCount` y `areaSquareMeters`, y DEBE permitir omitir el archivo opcional `image`.
- **RF-003**: El sistema DEBE exigir valores no vacíos para `title`, `description` y `address`; un `price` y un `areaSquareMeters` mayores que cero; cantidades `bedroomCount` y `bathroomCount` iguales o mayores que cero; y un `status` perteneciente a `Available`, `Rented` o `Maintenance`.
- **RF-004**: Cuando no se adjunte `image`, el sistema DEBE crear la propiedad con `imageUrl` igual a `null`, tanto en la persistencia como en la representación de respuesta. El almacenamiento persistente DEBE permitir ese valor nulo.
- **RF-005**: Cuando se adjunte una imagen, el sistema DEBE aceptar únicamente imágenes PNG o JPG/JPEG válidas, con tamaño máximo inclusivo de 5 MB (5.242.880 bytes) y costo estimado de memoria decodificada no superior a 256 MiB (268.435.456 bytes). DEBE rechazar archivos corruptos, cuyo contenido no corresponda al formato permitido o que excedan el presupuesto de memoria.
- **RF-006**: El sistema DEBE almacenar las imágenes aceptadas en `app/backend/src/RealtorApi/wwwroot/assets/properties` y publicarlas bajo la ruta `/assets/properties/`; los nombres de archivo DEBEN evitar sobrescrituras y no DEBEN permitir rutas controladas por el cliente.
- **RF-007**: Si la imagen se almacena correctamente, el sistema DEBE persistir en `imageUrl` y devolver en la respuesta la ruta pública relativa `/assets/properties/{nombre-de-archivo}`. NO DEBE persistir ni revelar una ruta física del sistema de archivos.
- **RF-008**: Un alta válida DEBE responder HTTP 201 con una representación de la propiedad creada que incluya `id`, todos los campos de propiedad recibidos y `imageUrl`, nullable. El endpoint NO DEBE alterar el contrato existente de `GET /api/properties`, que devuelve URL absoluta pública para imágenes disponibles.
- **RF-009**: La ausencia de campos requeridos, los valores fuera de las reglas funcionales, el formato de imagen inválido, el contenido corrupto o el costo estimado de memoria decodificada superior a 256 MiB DEBEN responder HTTP 400 con `ValidationProblemDetails`, identificando los errores aplicables y sin crear la propiedad.
- **RF-010**: Un archivo que supere el máximo de 5 MB DEBE responder HTTP 413 con `ProblemDetails` y no DEBE crear la propiedad.
- **RF-011**: Si falla el almacenamiento del archivo o la persistencia, el endpoint DEBE responder HTTP 500 con `ProblemDetails`, sin exponer rutas físicas, mensajes internos ni secretos. Un fallo al almacenar la imagen NO DEBE dejar creada una propiedad que la referencia. Si la imagen se guardó pero falla la persistencia de la propiedad, el sistema DEBE intentar eliminar el archivo y DEBE registrar el error si la limpieza también falla.
- **RF-012**: El caso de uso DEBE seguir la arquitectura vertical existente con un slice `ISlice`, un handler concreto registrado mediante `IHandler`, un validator y mapeos explícitos propios del slice para sus contratos y datos de propiedad.
- **RF-013**: El sistema DEBE descubrir y registrar automáticamente el slice mediante `ISlice` y el handler mediante `IHandler`; agregar el caso de uso NO DEBE requerir un registro individual en `Program.cs`.
- **RF-014**: Los mapeos explícitos del slice DEBEN limitarse a transformar contratos y datos; NO DEBEN contener validación, decisiones de negocio, persistencia ni gestión de archivos.
- **RF-015**: La validación y los errores del caso de uso DEBEN integrarse con la infraestructura existente de FluentValidation y `ProblemDetails`, y las pruebas de esta funcionalidad DEBEN ser unitarias salvo aprobación explícita posterior de otro tipo de prueba.
- **RF-016**: La implementación DEBE reutilizar la entidad y persistencia de propiedades de la especificación 003 y coexistir con el listado de la especificación 004; cualquier ajuste de nulabilidad necesario para cumplir RF-004 DEBE permanecer limitado al soporte de `ImageUrl` nulo.

### Entidades principales *(si la funcionalidad maneja datos)*

- **Solicitud de alta de propiedad**: Datos de entrada de la nueva propiedad —título, descripción, dirección, precio, estado, dormitorios, baños y superficie— con un archivo de imagen opcional.
- **Propiedad creada**: Propiedad persistida con sus datos de dominio, identificador y `imageUrl` nullable; si hay imagen, referencia su ubicación pública relativa.
- **Imagen de propiedad**: Archivo PNG o JPG/JPEG válido que cumple los límites de tamaño y memoria decodificada, queda disponible en la ruta pública de assets de propiedades y se asocia a una propiedad.

## Criterios de éxito *(obligatorio)*

### Resultados medibles

- **CE-001**: El 100 % de las solicitudes válidas sin imagen devuelve HTTP 201 y persiste una propiedad cuyo `imageUrl` es `null`.
- **CE-002**: El 100 % de las solicitudes válidas con imagen PNG/JPG de hasta 5 MB y costo estimado de memoria decodificada no superior a 256 MiB devuelve HTTP 201, almacena un archivo accesible bajo `/assets/properties/` y persiste la ruta pública relativa correspondiente.
- **CE-003**: El 100 % de las solicitudes con datos inválidos, imágenes corruptas/no permitidas o costo estimado de memoria decodificada superior a 256 MiB recibe HTTP 400 con errores de validación; el 100 % de los archivos mayores a 5 MB recibe HTTP 413; ninguna de estas solicitudes crea una propiedad.
- **CE-004**: El 100 % de los fallos de almacenamiento o persistencia devuelve HTTP 500 con `ProblemDetails` sin revelar detalles internos ni declarar el alta como exitosa.
- **CE-005**: Una propiedad creada con imagen se puede consumir en el listado existente mediante una URL pública absoluta, y una propiedad sin imagen se representa con `imageUrl` nulo.

## Supuestos

- **Dependencias previas**: La especificación [001-realtor-solution-foundation](../001-realtor-solution-foundation/spec.md) proporciona la solución y el backend; [002-foundation-backend](../002-foundation-backend/spec.md) establece Minimal APIs, `ISlice`, auto-registro de `IHandler`, validación y errores; [003-properties-persistence-seeding](../003-properties-persistence-seeding/spec.md) define la entidad, persistencia y publicación de assets; [004-properties-list-pagination](../004-properties-list-pagination/spec.md) define la consulta y su contrato de URL de imagen.
- El modelo actual declara `Property.ImageUrl` como no nullable; para cumplir el requisito explícito de crear propiedades sin imagen, esta iniciativa requiere hacer nullable ese dato en el modelo y en el almacenamiento persistente, con el ajuste de esquema limitado necesario.
- La solicitud de alta utiliza `multipart/form-data` incluso cuando se omite la imagen, con los mismos campos de propiedad en ambos casos.
- Para hacer verificable el límite solicitado, 5 MB se interpreta como 5 × 1.048.576 bytes (5.242.880 bytes), incluidos.
- El costo estimado de memoria decodificada se calcula antes de decodificar la imagen y no puede superar 256 × 1.048.576 bytes (268.435.456 bytes).
- La aceptación de PNG/JPG se determina por la validez y el contenido del archivo además de la extensión declarada; las imágenes de otros formatos no se convierten.
- La ruta guardada en `imageUrl` es relativa y pública. El listado de la especificación 004 mantiene su comportamiento de construir una URL absoluta pública a partir de esa ruta.
- La nulabilidad de `ImageUrl` se limita al caso de propiedad sin imagen; no se alteran los campos requeridos restantes, los datos de seed ni el alcance de listado.
- No se definen en esta iniciativa un flujo de edición, eliminación de propiedades, reemplazo de imágenes ni restricciones de unicidad para el catálogo.
