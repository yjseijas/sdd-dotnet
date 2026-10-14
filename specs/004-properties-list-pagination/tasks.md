# Tareas: Listado paginado de propiedades

**Entrada**: Documentos de diseño de `/specs/004-properties-list-pagination/`

**Prerrequisitos**: `plan.md`, `spec.md`, `research.md`, `data-model.md` y
`contracts/properties-list.openapi.json`.

**Verificación**: Pruebas unitarias xUnit para validación, mapping y metadatos;
verificación manual del endpoint con `RealtorApi.http` y los escenarios de
`quickstart.md`. No se agregan pruebas de integración.

**Organización**: Las tareas se agrupan por historia de usuario y siguen las
prioridades P1 y P2 de `spec.md`. Se reutilizan los proyectos, dependencias y
persistencia existentes; no se agregan paquetes ni migraciones.

## Fase 1: Preparación (estructura compartida)

**Objetivo**: Confirmar la base existente que utilizará la funcionalidad, sin
crear proyectos ni estructuras paralelas.

- [ ] T001 Verificar que `global.json`, `app/backend/src/RealtorApi/RealtorApi.csproj` y `app/backend/tests/RealtorApiTests/RealtorApiTests.csproj` proporcionan el SDK, la API y el proyecto xUnit previstos en `plan.md`; reutilizar esos proyectos sin agregar dependencias ni cambios de persistencia.

---

## Fase 2: Base (prerrequisitos bloqueantes)

**Objetivo**: Conectar la validación automática compartida que requiere el
endpoint antes de implementar las historias.

- [ ] T002 Conectar `ValidationFilterFactory` al mapeo de slices en `app/backend/src/RealtorApi/Infrastructure/Endpoints/SliceRegistry.cs` y resolver validators desde `HttpContext.RequestServices` en `app/backend/src/RealtorApi/Infrastructure/Validation/ValidationFilterFactory.cs`, conservando `ValidationProblemDetails` y la cancelación de la solicitud.

**Punto de control**: El registro común de slices aplica el filtro de validación
y resuelve dependencias dentro del scope HTTP.

---

## Fase 3: Historia de usuario 1: Consultar el catálogo paginado (Prioridad: P1) 🎯 MVP

**Objetivo**: Consultar las propiedades mediante páginas ordenadas de manera
estable, con sus datos de catálogo y URLs absolutas de imagen servidas por la
aplicación.

**Prueba independiente**: Consultar el endpoint sin parámetros y con
`page=2&pageSize=6`; comprobar los elementos, el orden por título e identificador,
las URLs públicas y todos los metadatos de paginación.

### Verificación de la historia de usuario 1

- [ ] T003 [P] [US1] Añadir en `app/backend/tests/RealtorApiTests/Features/Properties/ListPropertiesTests.cs` pruebas unitarias xUnit del mapping de estado y URL absoluta, y del cálculo de `totalPages`, `hasNext` y `hasPrevious`, incluyendo una colección vacía.
- [ ] T004 [P] [US1] Añadir en `app/backend/src/RealtorApi/RealtorApi.http` solicitudes manuales para `GET /api/properties` y `GET /api/properties?page=1&pageSize=6`, según RF-010.

### Implementación de la historia de usuario 1

- [ ] T005 [P] [US1] Definir en `app/backend/src/RealtorApi/Features/Properties/ListProperties/ListPropertiesSlice.cs` el contrato explícito del slice y el endpoint `GET /api/properties`, con valores predeterminados independientes para `page` y `pageSize` y una respuesta paginada con `items` y sus seis metadatos.
- [ ] T006 [P] [US1] Implementar en `app/backend/src/RealtorApi/Features/Properties/ListProperties/ListPropertiesHandler.cs` la consulta de solo lectura a `AppDbContext.Properties`, con `AsNoTracking`, proyección de los campos requeridos, orden por título e identificador, conteo, `Skip`/`Take` y propagación de `CancellationToken`.
- [ ] T007 [P] [US1] Implementar en `app/backend/src/RealtorApi/Features/Properties/ListProperties/ListPropertiesMapping.cs` el mapping al contrato de respuesta, serializando `status` como texto y construyendo `imageUrl` absoluta desde el esquema y host de la solicitud con la ruta `/assets/properties/{fileName}`; calcular los metadatos conforme a `data-model.md`.
- [ ] T008 [P] [US1] Configurar en `app/backend/src/RealtorApi/Program.cs` el alias estático `/assets` sobre el mismo proveedor de archivos que `/images`, manteniendo disponible la ruta existente.

**Punto de control**: La consulta devuelve la página solicitada, metadatos
coherentes y URLs de imagen públicas, sin exponer entidades ni rutas físicas.

---

## Fase 4: Historia de usuario 2: Recibir errores claros ante parámetros inválidos (Prioridad: P2)

**Objetivo**: Rechazar entradas de paginación no enteras o fuera de rango con
HTTP 400 y detalles de validación, y responder correctamente a páginas fuera
del rango disponible.

**Prueba independiente**: Enviar valores no enteros, `page=0`, `pageSize=0` y
`pageSize=101`; comprobar HTTP 400 con errores asociados a los campos. Solicitar
una página posterior a la última y comprobar HTTP 200, `items` vacío y metadatos
coherentes.

### Verificación de la historia de usuario 2

- [ ] T009 [P] [US2] Extender `app/backend/tests/RealtorApiTests/Features/Properties/ListPropertiesTests.cs` con pruebas unitarias xUnit del validator para valores no enteros, límites válidos, `page` no positivo y `pageSize` fuera del rango 1–100.
- [ ] T010 [P] [US2] Añadir en `app/backend/src/RealtorApi/RealtorApi.http` solicitudes manuales para entradas de paginación inválidas y una página posterior a la última, de modo que se puedan comprobar las respuestas 400 y la página vacía descritas en `quickstart.md`.

### Implementación de la historia de usuario 2

- [ ] T011 [P] [US2] Implementar `app/backend/src/RealtorApi/Features/Properties/ListProperties/ListPropertiesValidator.cs` con FluentValidation para enteros positivos y el rango de `pageSize` 1–100; ajustar `app/backend/src/RealtorApi/Features/Properties/ListProperties/ListPropertiesSlice.cs` para conservar las entradas no enteras hasta la validación y aplicar cada valor predeterminado solo cuando falte su parámetro.

**Punto de control**: Las entradas inválidas producen `ValidationProblemDetails`
HTTP 400; una página fuera de rango produce una colección vacía sin error de
servidor.

---

## Fase 5: Cierre y aspectos transversales

**Objetivo**: Validar el incremento completo según las decisiones de diseño y
la guía de verificación.

- [ ] T012 Ejecutar `dotnet test app/backend/tests/RealtorApiTests/RealtorApiTests.csproj` y realizar las comprobaciones manuales de `specs/004-properties-list-pagination/quickstart.md` contra el backend; registrar los resultados de ambas verificaciones en esa guía.

---

## Dependencias y orden de ejecución

### Grafo de dependencias

```text
T001 → T002 → T003, T004, T005 [P] (preparación paralela de US1)
                    T005 → T006, T007, T008 [P] (implementación paralela)
T003, T004, T006, T007, T008 → US1 completa
US1 completa → T009, T010, T011 [P] → T012
```

- **Preparación**: T001 no tiene prerrequisitos.
- **Base**: T002 depende de T001 y debe completarse antes de las historias.
- **US1 (P1)**: Tras T002, T003, T004 y T005 pueden prepararse en paralelo. Una
  vez definido el contrato por T005, T006, T007 y T008 pueden avanzar en
  paralelo. T003 verifica el mapping y los metadatos definidos en T007 al
  completar la implementación.
- **US2 (P2)**: Depende de US1 porque amplía el mismo endpoint y comprueba el
  comportamiento de las páginas fuera de rango. Tras US1, T009, T010 y T011
  pueden ejecutarse en paralelo en archivos distintos.
- **Cierre**: T012 depende de las dos historias y sus pruebas.

### Oportunidades de paralelismo

- Tras T002, T003, T004 y T005 pueden prepararse en paralelo. Después de T005,
  T006, T007 y T008 son paralelizables.
- Tras US1, T009, T010 y T011 son paralelizables entre sí: modifican el archivo
  de pruebas, el archivo HTTP y los archivos de implementación,
  respectivamente.

## Ejemplo de paralelismo: historia de usuario 1

```text
Antes de la implementación del slice, en paralelo:
Tarea: Preparar pruebas unitarias de mapping y metadatos (T003)
Tarea: Añadir solicitudes válidas en RealtorApi.http (T004)

Después de definir el contrato del slice (T005), en paralelo:
Tarea: Implementar la consulta paginada en ListPropertiesHandler.cs (T006)
Tarea: Implementar mapping y metadatos en ListPropertiesMapping.cs (T007)
Tarea: Configurar el alias de assets en Program.cs (T008)

Después de completar US1, en paralelo:
Tarea: Añadir pruebas del validator en ListPropertiesTests.cs (T009)
Tarea: Añadir solicitudes de error en RealtorApi.http (T010)
Tarea: Implementar el validator y el binding (T011)
```

## Estrategia de implementación

### Primero el MVP (solo historia de usuario 1)

1. Completar preparación y base (T001–T002).
2. Preparar verificaciones unitarias y solicitudes HTTP válidas (T003–T004).
3. Implementar el contrato, la consulta y las URLs públicas (T005–T008).
4. Verificar el incremento P1 antes de iniciar la ampliación de validación.

### Entrega incremental

1. Completar y verificar US1 como incremento útil del catálogo.
2. Implementar validación de entradas y solicitudes de error de US2.
3. Ejecutar las pruebas unitarias y los escenarios manuales del quickstart
   mediante T012.

### Estrategia de equipo en paralelo

1. Completar juntos la base común y el contrato del slice.
2. Asignar las tareas independientes de US1 indicadas en el grafo.
3. Integrar US1 antes de extender los archivos compartidos para US2.

## Notas

- Todas las tareas usan checkbox, identificador secuencial, etiqueta `[P]`
  únicamente cuando hay paralelismo válido, etiqueta de historia en las fases
  US1/US2 y rutas concretas.
- No se incluyen pruebas de integración, migraciones, cambios de modelo
  persistente, endpoints de controller, paquetes nuevos ni cambios de frontend.
- Los artefactos y descripciones están en español; se mantienen en inglés solo
  los identificadores técnicos y los nombres de protocolo requeridos.
