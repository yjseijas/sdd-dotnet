---
name: 002-foundation-backend
agent: speckit.specify
---

/speckit.specify

Crear la especificación 002-foundation-backend.

Objetivo: establecer la estructura base interna del backend RealtorApi y los
cross-cutting concerns transversales, sin crear entidades de dominio ni features
funcionales de producto. Esta iniciativa parte de la base ya creada en la spec 001
(solución app/Realtor.sln y proyecto app/backend/src/RealtorApi ya existen) y NO
debe recrear la solución ni los proyectos.

Alcance incluido:

1. Estructura interna de carpetas del backend siguiendo Vertical Slice Architecture,
   preparada para features futuras pero sin ninguna feature todavía. Debe existir la
   organización transversal en Infrastructure (por ejemplo Endpoints, Validation,
   Handlers) según backend.instructions.md, sin implementar casos de uso de negocio.

2. Infraestructura de descubrimiento y mapeo de endpoints Minimal API mediante el
   patrón ISlice con reflection:
   - Contrato ISlice.
   - RegisterSlices por assembly.
   - MapSliceEndpoints centralizado.
   - Agregar un endpoint no debe requerir modificar Program.cs.
   - Sin registries manuales ni descubrimiento por nombre de clase.

3. Infraestructura de auto-registro de handlers de casos de uso mediante el marker
   interface IHandler y assembly scanning, sin implementar ningún handler concreto.

4. Validación automática de Minimal APIs con FluentValidation:
   - Registro de validators por assembly scanning.
   - ValidationFilterFactory basada en convención que detecta IValidator<T>.
   - Ausencia de validator no produce error (pass-through).
   - Errores de validación devuelven ValidationProblemDetails con HTTP 400.
   - Sin registrar validators, filtros ni handlers de forma individual.

5. Manejo consistente de errores esperados con Result y conversión centralizada a
   ProblemDetails, y logging estructurado mediante ILogger, como capacidades base
   disponibles para futuras features.

6. Migrar el endpoint /health existente al patrón ISlice, como única sonda de
   infraestructura de extremo a extremo. Es infraestructura, no una feature de
   negocio: no introduce entidades, base de datos ni lógica de dominio. Sirve para
   demostrar que el descubrimiento y el mapeo centralizado funcionan sin tocar
   Program.cs.

7. Program.cs debe limitarse a: configurar servicios, middleware, registrar la
   infraestructura anterior y mapear endpoints mediante el mecanismo centralizado.

Pruebas requeridas (unit tests, en app/backend/tests/RealtorApiTests):

- RegisterSlices descubre una clase ISlice de prueba definida en el proyecto de
  tests y la registra bajo el contrato ISlice, sin duplicados.
- ValidationFilterFactory detecta IValidator<T> cuando existe y ejecuta la
  validación; cuando no existe validator, hace pass-through sin error.
- Un fallo de validación produce ValidationProblemDetails con HTTP 400.
- El mapeo de Result de error a ProblemDetails produce el status code y el payload
  correctos.
- El endpoint /health migrado a ISlice responde correctamente su estado.

Restricciones y exclusiones explícitas:

- NO crear entidades de dominio.
- NO crear AppDbContext, configuraciones EF Core, migraciones ni seeders.
- NO crear conexión a base de datos.
- NO crear ningún endpoint, handler, validator ni mapping de negocio.
- NO introducir lógica de negocio ni features de producto.
- NO usar controllers.
- Las clases ISlice y validators de prueba usados para verificar la infraestructura
  DEBEN vivir en el proyecto de tests, nunca en el proyecto de producción.
- La versión de plataforma se deriva de global.json existente, sin modificarlo.

Criterios de éxito verificables:

- El proyecto compila con la infraestructura transversal registrada.
- El endpoint /health queda implementado como ISlice y es descubierto y mapeado sin
  registro manual en Program.cs.
- Un request con IValidator<T> registrado sería validado automáticamente; sin
  validator, pasa sin error.
- Los unit tests de la infraestructura transversal pasan.
- No existe ninguna entidad, base de datos, feature ni caso de uso de negocio en el
  backend.