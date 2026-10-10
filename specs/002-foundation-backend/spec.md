# Especificación de funcionalidad: Infraestructura base del backend

**Rama de funcionalidad**: `002-foundation-backend`

**Creada**: 2026-10-10

**Estado**: Implementada

> El estado de la especificación se modifica automáticamente del flujo Speckit: Borrador → Aprobada → En implementación → Implementada. No editarlo manualmente fuera de ese flujo.

**Entrada**: Descripción del usuario: "Crear la especificación 002-foundation-backend para establecer la estructura base interna del backend RealtorApi y capacidades transversales, sin entidades de dominio ni funcionalidades de producto. La iniciativa parte de la solución y el proyecto RealtorApi existentes; incorpora descubrimiento centralizado de endpoints Minimal API mediante ISlice, auto-registro de handlers, validación automática con FluentValidation, conversión centralizada de Result a ProblemDetails, logging estructurado y una sonda /health. Program.cs se limita a configurar servicios y middleware y a registrar y mapear centralizadamente la infraestructura. Se requieren pruebas unitarias de descubrimiento, validación, errores y health endpoint."

## Clarifications

### Session 2026-10-10

- Q: ¿Qué política debe definir la especificación para el acceso a la sonda `/health`? → A: Sin autenticación de aplicación; solo accesible desde red/origen interno mediante ingress o reverse proxy.

## Escenarios de usuario y pruebas *(obligatorio)*

### Historia de usuario 1: Incorporar endpoints sin editar el arranque (Prioridad: P1)

El equipo de backend necesita organizar el código para futuras funcionalidades verticales y agregar endpoints sin mantener listas manuales ni modificar `Program.cs` por cada endpoint. La sonda de infraestructura `/health` debe servir como demostración funcional del descubrimiento y mapeo centralizados, sin introducir lógica de producto.

**Motivo de prioridad**: El descubrimiento de endpoints es la base para que las funcionalidades futuras sigan la arquitectura del proyecto sin acoplar cada slice al arranque.

**Prueba independiente**: Una clase `ISlice` de prueba, definida en el proyecto de tests, se descubre desde el assembly de pruebas, se registra una sola vez bajo `ISlice` y su endpoint puede mapearse. La sonda `/health` del backend se descubre y responde con el estado saludable de la aplicación.

**Escenarios de aceptación**:

1. **Dado** que existe una implementación de `ISlice` en el assembly inspeccionado, **Cuando** se registran los slices, **Entonces** la implementación queda registrada bajo el contrato `ISlice` sin registros duplicados.
2. **Dado** que se agrega un nuevo endpoint como implementación de `ISlice`, **Cuando** inicia la aplicación, **Entonces** el mecanismo centralizado lo descubre y mapea sin añadir un registro individual a `Program.cs`.
3. **Dado** que la aplicación está disponible, **Cuando** se consulta `/health`, **Entonces** la respuesta indica que la aplicación está saludable.

---

### Historia de usuario 2: Validar solicitudes y registrar handlers de forma automática (Prioridad: P1)

El equipo necesita que los handlers y validators futuros puedan incorporarse por convenciones de assembly, y que las solicitudes de Minimal API se validen cuando exista un validator para su tipo. La ausencia de validator no debe impedir el procesamiento.

**Motivo de prioridad**: El registro y la validación automáticos proporcionan una base uniforme para las features futuras y evitan configuración individual repetida.

**Prueba independiente**: Pruebas unitarias verifican el auto-registro de handlers marcados con `IHandler`, la ejecución del validator correspondiente, el paso sin validación cuando no hay validator y la respuesta HTTP 400 con `ValidationProblemDetails` ante errores.

**Escenarios de aceptación**:

1. **Dado** que existe un handler concreto marcado con `IHandler` en el assembly inspeccionado, **Cuando** se registra la infraestructura, **Entonces** queda registrado automáticamente sin registro individual.
2. **Dado** que un parámetro de endpoint tiene un validator registrado para su tipo, **Cuando** la solicitud incumple sus reglas, **Entonces** la validación se ejecuta y la respuesta contiene `ValidationProblemDetails` con HTTP 400.
3. **Dado** que no existe un validator para el tipo de solicitud, **Cuando** se ejecuta el filtro de validación, **Entonces** la solicitud continúa sin error atribuible a la ausencia de validator.
4. **Dado** que existen validators en el assembly de la aplicación, **Cuando** se configura la infraestructura, **Entonces** se descubren automáticamente sin registro individual de cada validator.

---

### Historia de usuario 3: Unificar errores esperados y su registro (Prioridad: P2)

El equipo necesita que los errores esperados representados mediante `Result` se transformen de manera coherente en respuestas `ProblemDetails`, y que las capacidades base permitan registrar eventos con información estructurada para facilitar su diagnóstico.

**Motivo de prioridad**: Una representación consistente de errores y registros estructurados evita que futuras slices inventen contratos incompatibles.

**Prueba independiente**: Una prueba unitaria convierte un resultado de error y comprueba que el `ProblemDetails` resultante conserva el código de estado y el contenido de error definidos por el resultado.

**Escenarios de aceptación**:

1. **Dado** un `Result` que representa un error esperado con código de estado y detalles, **Cuando** se convierte mediante el mecanismo centralizado, **Entonces** la respuesta `ProblemDetails` contiene el código y los detalles correspondientes.
2. **Dado** que una capacidad transversal necesita registrar un evento, **Cuando** lo hace, **Entonces** utiliza `ILogger` con propiedades estructuradas y no construye registros únicamente mediante interpolación de texto.

### Casos límite

- Si el assembly inspeccionado no contiene implementaciones concretas de `ISlice`, el descubrimiento no debe requerir una lista manual para completar el arranque.
- Si no existe un validator aplicable a un parámetro, el filtro debe permitir continuar la solicitud.
- Si un validator encuentra errores, todos los errores de validación aplicables deben poder comunicarse mediante `ValidationProblemDetails` y HTTP 400.
- El alcance no debe añadir una respuesta de salud dependiente de base de datos, ya que esta iniciativa no incorpora persistencia ni conexión a base de datos; `/health` debe permanecer sin autenticación de aplicación y limitarse a la red/origen interno mediante ingress o reverse proxy.
- Las implementaciones de `ISlice` y validators destinados a probar la infraestructura deben permanecer en el proyecto de tests y no incorporarse al ensamblado de producción.

## Requisitos *(obligatorio)*

### Requisitos funcionales

- **RF-001**: La iniciativa DEBE ampliar la estructura del proyecto `RealtorApi` ya existente y NO DEBE recrear la solución ni los proyectos establecidos por la iniciativa 001.
- **RF-002**: El backend DEBE organizar sus capacidades transversales de modo que las futuras funcionalidades puedan mantenerse en slices verticales; esta iniciativa NO DEBE crear una feature o caso de uso de negocio.
- **RF-003**: Cada endpoint del backend DEBE exponerse mediante una implementación del contrato `ISlice`.
- **RF-004**: El sistema DEBE descubrir las implementaciones de `ISlice` por assembly y registrarlas bajo el contrato, sin duplicados, sin listas o registries manuales y sin convenciones basadas en nombres de clase.
- **RF-005**: El sistema DEBE mapear los endpoints descubiertos mediante un mecanismo centralizado, de modo que agregar un endpoint no requiera modificar `Program.cs`.
- **RF-006**: El sistema DEBE registrar automáticamente, mediante inspección de assembly, los handlers concretos marcados con `IHandler`; NO DEBE implementar handlers de negocio ni requerir registros individuales.
- **RF-007**: El sistema DEBE descubrir automáticamente los validators disponibles mediante inspección de assembly y asociarlos por convención al tipo de solicitud validado, sin registros individuales de validators ni filtros.
- **RF-008**: Cuando exista un `IValidator<T>` aplicable a un parámetro de Minimal API, el filtro de validación DEBE ejecutarlo antes de procesar la solicitud.
- **RF-009**: Cuando no exista un validator aplicable, el filtro DEBE permitir que la solicitud continúe sin error por ausencia de validator.
- **RF-010**: Cuando la validación falle, el sistema DEBE responder con HTTP 400 y `ValidationProblemDetails` que exponga los errores de validación.
- **RF-011**: El backend DEBE ofrecer una conversión centralizada de errores esperados representados con `Result` a `ProblemDetails`, conservando el código de estado y los detalles asociados al error.
- **RF-012**: Las capacidades transversales de registro DEBEN utilizar `ILogger` y permitir eventos con propiedades estructuradas.
- **RF-013**: El endpoint `/health` DEBE ser la única sonda de infraestructura incluida en esta iniciativa, implementarse como `ISlice` y responder indicando el estado saludable de la aplicación, sin depender de persistencia. DEBE permanecer sin autenticación de aplicación y su acceso DEBE limitarse a la red/origen interno mediante ingress o reverse proxy.
- **RF-014**: `Program.cs` DEBE limitarse a configurar servicios y middleware, registrar la infraestructura transversal y mapear endpoints mediante el mecanismo centralizado; NO DEBE registrar endpoints, validators o handlers individuales.
- **RF-015**: El proyecto `app/backend/tests/RealtorApiTests` DEBE contar con pruebas unitarias para el descubrimiento y registro no duplicado de `ISlice`, la detección y ejecución de validators, el paso sin validator, la respuesta `ValidationProblemDetails` con HTTP 400, la conversión de `Result` a `ProblemDetails` y la respuesta del endpoint `/health`.
- **RF-016**: Las clases `ISlice` y validators de prueba usados para verificar la infraestructura DEBEN vivir exclusivamente en el proyecto de tests.
- **RF-017**: Esta iniciativa NO DEBE crear entidades de dominio, `AppDbContext`, configuraciones EF Core, migraciones, seeders, conexiones a base de datos, endpoints, handlers, validators ni mappings de negocio.
- **RF-018**: La versión de plataforma DEBE derivarse del `global.json` existente y esta iniciativa NO DEBE modificar ese archivo.

## Criterios de éxito *(obligatorio)*

### Resultados medibles

- **CE-001**: El 100 % de los escenarios de verificación definidos para incorporación de endpoints, validación y errores produce el resultado esperado.
- **CE-002**: Una consulta a `/health` informa que la aplicación está saludable sin depender de servicios de persistencia.
- **CE-003**: El equipo puede incorporar un endpoint nuevo sin mantener una lista manual ni modificar la configuración central de arranque.
- **CE-004**: Las solicitudes sin reglas de validación aplicables continúan su procesamiento; las que incumplen reglas son rechazadas con HTTP 400 y detalles estructurados.
- **CE-005**: Los errores esperados conservan su clasificación y detalles al comunicarse, y la solución existente compila con la versión de plataforma configurada en el repositorio.

## Supuestos

- La iniciativa 001 ya estableció `app/Realtor.sln` y `app/backend/src/RealtorApi`; este trabajo los amplía en lugar de recrearlos.
- La sonda `/health` es una comprobación de disponibilidad de la aplicación y no representa la salud de servicios externos o de una base de datos.
- El acceso a `/health` se restringe fuera de la aplicación, mediante ingress o reverse proxy, a la red/origen interno; no requiere autenticación de aplicación.
- La conversión de errores `Result` cubre errores esperados; el tratamiento de excepciones no controladas queda fuera del alcance de esta iniciativa.
- Las pruebas requeridas son unitarias y se mantienen dentro del proyecto backend de pruebas existente.
- La organización transversal descrita sirve a capacidades realmente compartidas y no sustituye la organización vertical de futuras funcionalidades.
