# Especificación de funcionalidad: Persistencia y seeding de propiedades

**Rama de funcionalidad**: `003-properties-persistence-seeding`

**Creada**: 2026-10-11

**Estado**: Implementada

> El estado de la especificación se modifica automáticamente del flujo Speckit: Borrador → Aprobada → En implementación → Implementada. No editarlo manualmente fuera de ese flujo.

**Entrada**: Descripción del usuario: "Crear la especificación 003-properties-persistence-seeding. Definir e implementar el modelo persistente inicial de propiedades y estados de propiedad, incluyendo migración EF Core, seeding idempotente automático y manejo de archivos externos para que queden consumibles por el proceso de inserción y por futuras consultas de propiedades con imagen."

## Escenarios de usuario y pruebas *(obligatorio)*

### Historia de usuario 1: Persistir la información base de propiedades y su estado (Prioridad: P1)

El equipo de negocio necesita que la propiedad que se inserta o consulta en el sistema quede almacenada con sus datos esenciales y con un estado operativo consistente. La base de datos debe reflejar el catálogo inicial de propiedades y permitir que las decisiones de disponibilidad, arriendo y mantenimiento se representen de forma estable para uso futuro.

**Motivo de prioridad**: Sin este modelo base, no es posible guardar ni consultar propiedades con trazabilidad ni coherencia operativa en el sistema.

**Prueba independiente**: Verificar que la entidad `Property` y la colección de estados de `PropertyStatus` quedan persistidos con las propiedades mínimas requeridas, el tipo de estado correcto y la migración del esquema asociada.

**Escenarios de aceptación**:

1. **Dado** que una propiedad necesita almacenarse, **Cuando** se registra en el modelo persistente, **Entonces** debe conservar título, descripción, dirección, precio, número de dormitorios, número de baños, área, URL pública de imagen, fechas de creación y actualización y el estado de propiedad correspondiente.
2. **Dado** que el estado de una propiedad debe expresarse de forma segura en base de datos, **Cuando** se persiste el valor, **Entonces** debe guardarse como texto mediante conversión y no como valor numérico o enumeración sin transformación.
3. **Dado** que el dominio de propiedades tiene estados definidos, **Cuando** se modela la propiedad, **Entonces** deben existir los valores `Available`, `Rented` y `Maintenance` dentro del tipo de dominio que represente la situación operativa.

---

### Historia de usuario 2: Cargar datos iniciales y activos externos de forma automática (Prioridad: P1)

El equipo necesita que al arrancar la aplicación se carguen los datos iniciales del catálogo de propiedades y los activos externos necesarios sin depender de ejecuciones manuales ni rutas dispersas. El sistema debe registrar la información de seed y los archivos estáticos en un único punto de gestión para asegurar consistencia entre entorno local y runtime.

**Motivo de prioridad**: El seeding automático elimina la dependencia de pasos manuales y garantiza que el sistema arranque con datos coherentes y activos disponibles para futuras consultas e inserciones.

**Prueba independiente**: Comprobar que `UseSeeding` y `UseAsyncSeeding` ejecutan seeding idempotente, que el manifiesto `seed-manifest.json` define las rutas correctas y que los assets quedan accesibles en runtime.

**Escenarios de aceptación**:

1. **Dado** que la aplicación se inicia con la base de datos disponible, **Cuando** EF Core ejecuta la inicialización, **Entonces** debe aplicar migraciones pendientes y ejecutar el seeding de forma automática sin que el arranque dependa de una llamada manual al seeder.
2. **Dado** que existen archivos de seed y assets externos en el proyecto, **Cuando** la aplicación se despliega, **Entonces** los JSON y el manifiesto deben integrarse en el build y quedar accesibles en runtime para resolver rutas sin código duplicado o hardcodeado.
3. **Dado** que el sistema requiere imágenes para propiedades, **Cuando** se resuelven los assets, **Entonces** los archivos deben copiarse a la ubicación final consumible por la API y la URL persistida debe ser la ruta pública servida por la aplicación, no una ruta física de soporte.
4. **Dado** que el seeding puede ejecutarse varias veces, **Cuando** la base de datos ya contiene los datos iniciales, **Entonces** la operación debe ser idempotente y no duplicar información ni generar inconsistencias.

---

### Historia de usuario 3: Preparar la capa de consultas para propiedades con imágenes y estados (Prioridad: P2)

Los equipos de producto y consulta necesitan que la información de propiedades quede lista para futuras búsquedas e inspecciones con imágenes, sin dejar rutas técnicas internas en la base de datos. La plataforma debe ofrecer un catastro estable y consumible por el proceso de alta y por futuras capas de consulta.

**Motivo de prioridad**: La base de datos debe soportar un modelo legible y consistente para futuras consultas, evitando que se almacenen rutas de archivos de soporte que no sean servidas por la API.

**Prueba independiente**: Validar que la ruta registrada en `ImageUrl` apunta a una URL pública servida por la API y que los datos de estado e propiedad están listos para consultas futuras sin requerir ajustes de esquema adicionales.

**Escenarios de aceptación**:

1. **Dado** que una propiedad incluye una imagen, **Cuando** se persiste la entidad, **Entonces** `ImageUrl` debe guardar la ruta final pública servida por la API y no la ruta local de `support`.
2. **Dado** que se quiere consumir la propiedad desde consultas futuras, **Cuando** la API expone la imagen o el resumen de la propiedad, **Entonces** la información ya estará preparada para devolver directamente el contenido esperado sin conversiones de ruta ni lógica adicional.
3. **Dado** que la solución incluye assets y datos iniciales, **Cuando** se ejecuta la aplicación en contextos de runtime, **Entonces** la información queda accesible mediante la estructura de archivos publicada y no depende de rutas hardcodeadas dispersas por el código.

---

### Casos límite

- ¿Qué ocurre si la aplicación arranca sin migraciones pendientes? La inicialización debe seguir ejecutándose con `MigrateAsync()`, porque el seeding debe activarse aun cuando el esquema ya esté actualizado.
- ¿Qué ocurre si el seed se ejecuta más de una vez? Debe mantenerse idempotente y evitar registros duplicados.
- ¿Qué ocurre si los archivos externos no se incluyen en el build? La aplicación no podrá resolver el manifest y la ruta final de imágenes, y la iniciativa debe bloquearse hasta corregir la integración del proyecto.
- ¿Qué ocurre si una entidad intenta guardar rutas físicas de soporte en `ImageUrl`? Esa práctica queda fuera del alcance y debe corregirse para guardar rutas públicas servidas por la API.

## Requisitos *(obligatorio)*

### Requisitos funcionales

- **RF-001**: La iniciativa DEBE definir el modelo persistente inicial de propiedades dentro del dominio de `RealtorApi`, con el archivo `Property.cs` y el tipo `PropertyStatus.cs` bajo la estructura correspondiente de `Domain/Properties`.
- **RF-002**: El sistema DEBE soportar los estados de propiedad `Available`, `Rented` y `Maintenance` y almacenar este estado como texto en base de datos mediante conversión EF Core.
- **RF-003**: La entidad `Property` DEBE incluir al menos `Id`, `Title`, `Description`, `Address`, `Price`, `Status`, `BedroomCount`, `BathroomCount`, `AreaSquareMeters`, `ImageUrl`, `CreatedAt` y `UpdatedAt`.
- **RF-004**: El modelo persistente DEBE registrarse con `DbSet<T>` y configurarse exclusivamente mediante `IEntityTypeConfiguration<T>` en `Infrastructure/Persistence/Configurations` sin mapping inline en `OnModelCreating`.
- **RF-005**: La solución DEBE generar una sola migración coherente para el cambio funcional completo de esta spec y no debe crear una migración por entidad separada.
- **RF-006**: El sistema DEBE configurar `UseSeeding()` y `UseAsyncSeeding()` en el `DbContext` y delegarlos a `DatabaseSeeder` para que la lógica de seed no se duplique ni se invoque manualmente.
- **RF-007**: El arranque de la aplicación DEBE ejecutar siempre `await app.MigrateAsync();` antes de `app.Run()` y no debe condicionar la llamada a la existencia de migraciones pendientes.
- **RF-008**: El proyecto DEBE integrar automáticamente los JSON, el manifiesto y los assets de `support` dentro de `RealtorApi.csproj` para que estén disponibles en runtime.
- **RF-009**: El seeding DEBE resolver rutas a partir de `support/seed-data/seed-manifest.json` y no con rutas hardcodeadas dispersas en el código.
- **RF-010**: La URL persistida en `ImageUrl` DEBE apuntar a la ruta pública servida por la API y no a ubicaciones físicas de soporte.
- **RF-011**: La iniciativa DEBE respetar la arquitectura de la solución, manteniendo la organización por features y evitando controladores o capas paralelas para endpoints de negocio.
- **RF-012**: Esta iniciativa DEBE usar `DatabaseSeeder` con versiones síncrona y asíncrona equivalentes y idempotentes, sin recurrir a `HasData()` para datos condicionales o dinámicos.
- **RF-013**: La especificación DEBE mantener la compatibilidad funcional con la infraestructura existente, sin introducir cambios ajenos a la persistencia, seeding y assets de propiedades.

### Entidades principales

- **PropertyStatus**: Enumeración operativa que representa los estados vigentes de una propiedad: `Available`, `Rented` y `Maintenance`.
- **Property**: Entidad persistente que representa una propiedad de la cartera, con atributos mínimos de identificación, descripción, ubicación, valor, estado, métricas físicas, imagen pública y marcas de auditoría de creación y actualización.

## Criterios de éxito *(obligatorio)*

### Resultados medibles

- **CE-001**: El modelo persistente de propiedades compila y cumple con las propiedades mínimas definidas por la iniciativa sin introducir cambios no previstos.
- **CE-002**: La migración única de la spec queda generada, revisada y lista para aplicar cambios del esquema estableciendo la base funcional del catálogo.
- **CE-003**: El seeding automático funciona con `UseSeeding` y `UseAsyncSeeding`, evita duplicados y se ejecuta al iniciar la aplicación incluso sin migraciones pendientes nuevas.
- **CE-004**: Los archivos de `support` y el manifiesto quedan integrados en el build y publicados en runtime, y las imágenes están disponibles en la ruta final consumible por la API.
- **CE-005**: La URL `ImageUrl` se guarda como ruta pública de servicio, no como ruta física interna, permitiendo uso directo en consultas futuras.

## Supuestos

- La aplicación ya cuenta con una estructura base compatible con Minimal APIs y vertical slices, por lo que esta iniciativa se centra exclusivamente en persistencia, migración y seeding de propiedades.
- Los datos de ejemplo y los archivos externos se alojan en un directorio de soporte del repositorio y deben resolverse desde un manifiesto central para mantener consistencia con el runtime.
- La ruta pública de la imagen debe coincidir con la forma de servir activos desde la aplicación y no debe depender de rutas de trabajo locales del desarrollador.
- La migración y el seeding deben ser idempotentes para soportar reinicios recurrentes de la aplicación y validaciones automáticas de la infraestructura.

