---
name: 005-properties-create
agent: speckit.specify
---

/speckit.specify

Crear la especificación 005-properties-create para definir un nuevo caso de uso de alta de propiedades mediante un slice Minimal API en backend. La especificación debe cubrir un endpoint público de creación que reciba los datos de la propiedad y permita de forma opcional el upload de imagen. Si no se envía imagen, la propiedad debe persistirse correctamente con imageUrl en null. Si se envía imagen válida, debe almacenarse en app/backend/src/RealtorApi/wwwroot/assets/properties y persistirse la URL pública/relativa correspondiente en la propiedad para su consumo posterior por clientes. Incluir criterios de validación funcional del upload (formatos PNG/JPG y tamaño máximo 5MB), reglas de respuesta y errores esperados, escenarios independientes por prioridad (P1/P2/P3), requisitos funcionales testables y criterios de éxito medibles. Mantener alineación con la arquitectura existente de Vertical Slice (slice + handler + mapping + validator), auto-registro por IHandler e ISlice, y mapeos explícitos por slice sin lógica de negocio en mapping. Respetar gobernanza Spec-Driven del repositorio y dependencias con specs previas de foundation, persistencia y listado.