# Investigación: infraestructura base del backend

## Decisiones

- **Decisión 1**: Usar `ISlice` como contrato central para registrar endpoints.
  - **Racional**: la especificación exige que cada endpoint se exponga mediante una implementación del contrato `ISlice`, con descubrimiento automático y mapeo centralizado.
  - **Alternativas consideradas**: registrar endpoints directamente en `Program.cs` o crear un registry estático manual. Ambas se descartan porque no cumplen la auto-descubierta ni la reducción del acoplamiento.

- **Decisión 2**: Registrar handlers concretos por inspección del assembly y convención por tipo.
  - **Racional**: la iniciativa requiere auto-registro de `IHandler` y considera el descubrimiento de validators como una capacidad transversal base.
  - **Alternativas consideradas**: instanciación manual de handlers o un `ServiceCollection` lleno de llamadas por feature. Se descarta porque rompe la obligación de no mantener registro individual.

- **Decisión 3**: Implementar un filtro de validación automático con `ValidationFilterFactory` o equivalente.
  - **Racional**: la especificación exige que la validación se ejecute cuando exista un validator para el tipo de solicitud, y que se omita sin error cuando no exista.
  - **Alternativas consideradas**: validar en cada endpoint o invocar validadores manualmente. Se descarta porque no escala ni mantiene un patrón único.

- **Decisión 4**: Centralizar la transformación de `Result` a `ProblemDetails` en un componente de infraestructura compartida.
  - **Racional**: la especificación exige un mecanismo uniforme para errores esperados y respuestas HTTP consistentes.
  - **Alternativas consideradas**: devolver strings o `Results.Problem` directos desde endpoints. Se descarta porque dispersa la lógica de errores y rompe la trazabilidad.

- **Decisión 5**: Exponer `/health` como endpoint de infraestructura sin autenticación y sin checks de base de datos.
  - **Racional**: la especificación fija la política de acceso y la ausencia de persistencia en esta iniciativa.
  - **Alternativas consideradas**: incluir validación de DB o seguridad de aplicación. Se descarta porque excede el alcance y contradice la política de acceso de la sonda.

## Requisitos y dependencias resueltas

- Se confirma que la infraestructura de backend debe mantenerse en `app/backend/src/RealtorApi` sin introducir controllers ni features productivas.
- Se confirma que la validación debe depender de `FluentValidation` y de la resolución de validators por tipo.
- Se confirma que la infraestructura debe permitir `ILogger` con propiedades estructuradas, no solo interpolación textual.
- Se confirma que el proyecto de prueba debe enfocarse en infraestructura y no incluir clases de dominio o endpoints de negocio.

## Riesgos y límites

- Si se introduce una entidad de dominio o una feature productiva en esta iniciativa, la solución quedará fuera del alcance aprobado.
- Si el registro automático se basa en nombres de clase en lugar de contrato, la implementación contradice la especificación.
- Si la validación se vuelve obligatoria aunque no exista validator, los endpoints no podrán seguir el comportamiento esperado definido por la spec.
- Si `/health` depende de persistencia, se romperá la promesa de no añadir acceso a datos en esta fase.

## Resultado esperado

La infraestructura del backend queda preparada para auto-descubrir tipos, registrar slices y handlers, validar entradas, convertir resultados esperados a `ProblemDetails`, y exponer una sonda de salud sin acoplar la aplicación a datos o identidad.
