---
name: 006-properties-update
agent: speckit.specify
---

/speckit.specify


Crear la especificación 006-properties-update para definir un nuevo caso de uso de actualización de propiedades mediante un slice Minimal API en backend.

La especificación debe cubrir un endpoint público de actualización por id (por ejemplo PUT /api/properties/{id}), que reciba los datos editables de la propiedad y permita de forma opcional el upload de imagen, manteniendo la misma lógica implementada en 005-properties-create para manejo de imágenes.

Reglas funcionales obligatorias:
1. Antes de actualizar, el sistema debe buscar la propiedad por id en base de datos.
2. Si la propiedad no existe, debe responder Not Found (404) y no realizar ningún cambio.
3. Si existe, debe aplicar la actualización de los campos de negocio válidos.
4. Si el request incluye imagen válida (PNG/JPG, máximo 5 MB), debe almacenarse en el directorio de propiedades (wwwroot/assets/properties o file:properties según la convención vigente), con nombre interno único (UUID + extensión válida), y debe persistirse la nueva imageUrl pública/relativa en la base de datos.
5. Si el request no incluye imagen, no se debe modificar la imageUrl existente; debe conservarse el valor anterior.
6. Si falla el guardado de imagen durante la actualización, la operación debe ser atómica: no debe quedar actualización parcial inconsistente.
7. Mantener semántica de errores consistente con 005:
   - 400 para validaciones de negocio inválidas.
   - 415 para formato no permitido, archivo vacío (0 bytes) o contenido real inválido.
   - 413 para imagen mayor a 5 MB.
   - 500 para fallo interno de almacenamiento de imagen.
   - 404 cuando la entidad no exista.

La spec debe incluir:
1. Historias por prioridad (P1/P2/P3) con escenarios independientes:
   - P1: actualización sin imagen (conservando imageUrl previa).
   - P2: actualización con imagen válida (reemplazando imageUrl).
   - P3: rechazos por validación y manejo de errores.
2. Requisitos funcionales testables (FR) equivalentes en detalle a 005.
3. Edge cases, incluyendo:
   - id inexistente.
   - imagen vacía o con contenido real inválido.
   - colisión de nombre resuelta por UUID.
   - no alteración de imageUrl cuando no se envía imagen.
4. Criterios de éxito medibles (SC), incluyendo latencia p95 en muestra funcional de solicitudes válidas.
5. Alineación explícita con arquitectura Vertical Slice existente:
   - slice + handler + mapping + validator.
   - auto-registro por IHandler e ISlice.
   - mapping sin lógica de negocio.
6. Compatibilidad con specs previas de foundation, persistencia, listado y create (001-005), y con gobernanza Spec-Driven del repositorio.

