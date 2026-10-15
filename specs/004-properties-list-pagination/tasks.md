# Tareas: Listado paginado de propiedades

**Entrada**: Documentos de diseño de `/specs/004-properties-list-pagination/`

**Prerrequisitos**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `quickstart.md` y `contracts/` cumplen la base de la feature.

**Verificación**: Esta feature requiere comprobación del endpoint `GET /api/properties`, validación de `page` y `pageSize`, orden estable por título, metadatos de paginación y generaciónd de `imageUrl` absoluta. No se exige TDD, pero sí debe ejecutarse la verificación automatizada de compilación y pruebas del backend asociadas a la feature.

## Formato de tareas

- [ ] T001 [P] ...
- [ ] T002 [US1] ...

## Fase 1: Preparación y alineación del alcance

**Objetivo**: Confirmar la estructura del backend, los contratos de la paginación y los archivos a modificar para la feature.

- [ ] T001 [P] Revisar `spec.md`, `plan.md`, `research.md`, `data-model.md`, `quickstart.md` y `contracts/README.md` para confirmar los requisitos de `GET /api/properties`, la validación de `page`/`pageSize`, el orden por título y la generación de `imageUrl` en `app/backend/src/RealtorApi/`.
- [ ] T002 [P] Confirmar la estructura de carpetas del backend y los puntos de extensión del proyecto (`app/backend/src/RealtorApi/Features/Properties/`, `Infrastructure/Validation/`, `Program.cs`, `RealtorApiTests/`) para implementar la feature sin tocar otras capas no requeridas.

---

## Fase 2: Base del caso de uso de listado paginado

**Objetivo**: Preparar los contratos y la lógica base de la consulta para que la API pueda paginar propiedades, ordenar resultados y construir URLs públicas.

- [ ] T003 [US1] Crear el request y el validador del catálogo en `app/backend/src/RealtorApi/Features/Properties/GetPropertiesRequest.cs` y `app/backend/src/RealtorApi/Features/Properties/GetPropertiesValidator.cs`, con validación para `page` > 0 y `pageSize` en rango 1..100.
- [ ] T004 [US1] Crear los DTO de respuesta en `app/backend/src/RealtorApi/Features/Properties/GetPropertiesResponse.cs`, incluyendo `PropertySummaryResponse` y `PaginatedPropertiesResponse` con `page`, `pageSize`, `totalItems`, `totalPages`, `hasNext`, `hasPrevious` e `items`.
- [ ] T005 [US1] Crear la consulta de dominio en `app/backend/src/RealtorApi/Features/Properties/GetPropertiesQuery.cs` para ejecutar la búsqueda en `AppDbContext`, ordenar por `Title` ascendente y `Id` ascendente y calcular el total de elementos.
- [ ] T006 [US1] Implementar el slice del endpoint en `app/backend/src/RealtorApi/Features/Properties/PropertiesSlice.cs` con `GET /api/properties`, aplicando la validación del request y devolviendo la respuesta paginada con `IResult`.
- [ ] T007 [US1] Ajustar la composición de `imageUrl` en la capa de consulta para construir una URL absoluta pública con esquema y host actuales, asegurando el formato `http(s)://{host}/assets/properties/{fileName}` y sin exponer rutas físicas internas.

**Punto de control**: La consulta paginada está integrada y responde con los datos requeridos por la especificación, sin exponer la entidad persistente.

---

## Fase 3: Historia de usuario 1: Consultar el catálogo paginado (Prioridad: P1) 🎯 MVP

**Objetivo**: Obtener el listado de propiedades paginado, ordenado por título y con metadatos coherentes para el consumidor.

**Prueba independiente**: Consultar la primera página sin filtros y otra página con `pageSize=6`, comprobar el orden, los elementos y los valores de metadatos para el conjunto de datos.

### Verificación de la historia de usuario 1

- [ ] T008 [P] [US1] Crear pruebas del caso feliz en `app/backend/tests/RealtorApiTests/Properties/GetPropertiesTests.cs` para validar la primera página por defecto, la segunda página con `pageSize=6` y la ordenación por `Title` + `Id`.
- [ ] T009 [P] [US1] Ejecutar las pruebas del caso feliz para comprobar `totalItems`, `totalPages`, `hasNext`, `hasPrevious` y la composición de `imageUrl` con el host actual.

### Implementación de la historia de usuario 1

- [ ] T010 [US1] Integrar la consulta con `AppDbContext` y el patrón del proyecto para reutilizar la infraestructura existente de slices y validación automática en `app/backend/src/RealtorApi/Program.cs` y `app/backend/src/RealtorApi/Infrastructure/Validation/ValidationFilterFactory.cs`.
- [ ] T011 [US1] Confirmar que la respuesta devuelva una colección vacía y metadatos coherentes cuando la página supera el rango disponible, sin error de servidor, según los requisitos de la spec.

**Punto de control**: La historia 1 cumple los criterios del catálogo paginado con imágenes públicas absolutas y un orden estable.

---

## Fase 4: Historia de usuario 2: Manejar parámetros inválidos y páginas fuera de rango (Prioridad: P2)

**Objetivo**: Garantizar respuestas claras y consistentes ante errores de entrada en la paginación.

**Prueba independiente**: Enviar valores no enteros, negativos, fuera de rango o `pageSize` mayor a 100 y comprobar que cada solicitud invalida responde con `400` y detalles de validación.

### Verificación de la historia de usuario 2

- [ ] T012 [P] [US2] Crear pruebas de validación en `app/backend/tests/RealtorApiTests/Properties/GetPropertiesTests.cs` para `page` no entero, `page` <= 0, `pageSize` <= 0, `pageSize` > 100 y una página fuera del rango.
- [ ] T013 [P] [US2] Ejecutar las pruebas de validación y confirmar que cada caso inválido responde con `400 Bad Request` y `ValidationProblemDetails` o `ProblemDetails` del backend base.

### Implementación de la historia de usuario 2

- [ ] T014 [US2] Ajustar la validación del request para distinguir casos no enteros, valores nulos o fuera de rango y devolver mensajes específicos para `page` y `pageSize` en `app/backend/src/RealtorApi/Features/Properties/GetPropertiesValidator.cs`.
- [ ] T015 [US2] Revisar la ruta `GET /api/properties` en `PropertiesSlice.cs` para que una página fuera del rango devuelva `200` con colección vacía y metadatos coherentes, sin transformar el caso en error de servidor.

**Punto de control**: La validación y el tratamiento de páginas fuera de rango cumplen la expectativa de UX y error del backend.

---

## Fase 5: Documentación operativa y cierre

**Objetivo**: Completar la documentación de uso y verificar la feature conforme al plan y la guía rápida.

- [ ] T016 [P] Crear y actualizar ejemplos de consulta para `GET /api/properties` y `GET /api/properties?page=1&pageSize=6` en `app/backend/src/RealtorApi/RealtorApi.http` para reflejar la API pública de la feature.
- [ ] T017 [P] Revisar `specs/004-properties-list-pagination/quickstart.md` y confirmar que los pasos de validación cubren la compilación, la ejecución del endpoint y las comprobaciones de paginación y validación.
- [ ] T018 Ejecutar `dotnet build app/backend/src/RealtorApi/RealtorApi.csproj` para validar que la feature compila sin errores.
- [ ] T019 Ejecutar `dotnet test app/backend/tests/RealtorApiTests/RealtorApiTests.csproj --logger "console;verbosity=minimal"` para validar el comportamiento del endpoint, la paginación y la validación del caso inválido.
- [ ] T020 Revisar que la feature cumpla la constitución y los criterios de aceptación de la spec: backend minimal API, no controladores, no cambio de esquema, validación consistente, URL pública absoluta y metadatos coherentes.

---

## Dependencias y orden de ejecución

### Dependencias entre fases

- **Preparación (Fase 1)**: sin dependencias; se ejecuta al inicio.
- **Base del caso de uso (Fase 2)**: depende de la preparación y bloquea la historia de usuario 1.
- **Historia de usuario 1 (Fase 3)**: requiere completar la base del caso de uso.
- **Historia de usuario 2 (Fase 4)**: depende de la base del caso de uso y puede validarse en paralelo con la fase 3 si los archivos no se solapan.
- **Cierre y documentación (Fase 5)**: depende de que las historias estén validadas.

### Dependencias entre historias

- **[US1]**: no tiene dependencias previas; es la base del MVP.
- **[US2]**: depende de la validación y la respuesta base del endpoint, pero puede validarse con pruebas aisladas del contrato.

### Oportunidades de paralelismo

- T001 y T002 pueden ejecutarse en paralelo porque son revisión y preparación del alcance.
- T008 y T009 pueden ejecutarse en paralelo con T010 y T011 si se les separa la creación de pruebas y la implementación de la respuesta.
- T012 y T013 pueden ejecutarse en paralelo con la validación de la historia 1 si los tests no comparten archivos de prueba.
- T016 y T017 son tareas independientes de documentación y verificación.

### Estrategia de implementación

#### Primero el MVP

1. Completar la preparación y la base del caso de uso.
2. Implementar la consulta y el endpoint de `GET /api/properties` con paginación por defecto.
3. Validar la primera página y la segunda página con metadatos y orden por título.
4. Añadir validación y manejo de páginas fuera de rango.

#### Entrega incremental

1. Completar el caso feliz del catálogo.
2. Añadir validación y páginas fuera de rango.
3. Revisar documentación y ejecutar la verificación final del proyecto.

---

## Revisión final

La feature queda completa cuando:
- `GET /api/properties` responde con el contrato requerido y sin romper el backend minimal API.
- El orden por `Title` + `Id` queda estable en todas las páginas.
- La validación de `page`/`pageSize` rechaza entradas inválidas con `400`.
- `imageUrl` es público y absoluto con host y esquema actuales.
- La documentación y las pruebas reflejan el comportamiento esperado y la verificación del plan se ejecuta sin errores.
