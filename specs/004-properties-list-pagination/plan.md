# Plan de implementación: Listado paginado de propiedades

**Rama**: `004-properties-list-pagination` | **Fecha**: 2026-10-15 | **Especificación**: [spec.md](./spec.md)

**Entrada**: Especificación de funcionalidad de `/specs/004-properties-list-pagination/spec.md`

## Resumen

Esta iniciativa añade el endpoint `GET /api/properties` para consultar el catálogo de propiedades paginado, ordenado por título ascendente y con metadatos de paginación consistentes. El acceso se realiza a través del backend minimal API existente, reutilizando la entidad `Property` persistida, validando `page` y `pageSize` con la infraestructura de validación centralizada, y construyendo un `imageUrl` público absoluto con el esquema y host de la petición actual.

## Contexto técnico

**Lenguaje/versión**: C# con .NET 11, siguiendo `global.json` y la arquitectura base del repositorio.

**Dependencias principales**: ASP.NET Core Minimal APIs, `FluentValidation`, `IValidator`, `ValidationFilterFactory`, EF Core con `AppDbContext`, `Npgsql`/PostgreSQL y la infraestructura de slices existente (`ISlice`, `AddSlices`, `MapSlices`).

**Persistencia**: No se modifica el esquema; esta feature consulta la entidad `Property` ya creada en la iniciativa previa y se apoya en el modelo persistente actual, sin nuevas migraciones ni cambios de base de datos.

**Pruebas**: Validación con pruebas unitarias del backend en `app/backend/tests/RealtorApiTests` y pruebas de integración ligeras del endpoint cuando la especificación aplique. El plan prioriza validación de orden, límites, metadatos y generación de URLs públicas.

**Plataforma objetivo**: Backend `RealtorApi` y consumo desde frontend Blazor con la API pública `GET /api/properties`.

**Tipo de proyecto**: Aplicación web full stack con backend minimal API y consultas paginadas del catálogo inmobiliario.

**Objetivos de rendimiento**: Mantener la consulta en base de datos eficiente con `Skip`/`Take` sobre un orden estable por título + identificador, sin cargar todo el conjunto ni introducir lógica fuera del endpoint.

**Restricciones**: No se permiten controladores MVC ni cambios de esquema; la respuesta debe ser una proyección de lectura sin exponer la entidad persistente; el valor `imageUrl` debe ser público y absoluto; la validación debe ser compatible con `ProblemDetails`/`ValidationProblemDetails` del sistema base.

**Escala/alcance**: Esta feature está acotada al listado paginado de propiedades y no introduce entidades ni cambios funcionales ajenos al catálogo.

## Comprobación de la constitución

- [x] El backend propuesto usa ASP.NET Core Minimal APIs, sin controladores MVC, y organiza la funcionalidad verticalmente por caso de uso.
- [x] La interfaz web propuesta usa Blazor; la lógica de negocio del backend y los contratos entre frontend y backend tienen límites explícitos.
- [x] Cada capa, proyecto, abstracción y dependencia nueva responde a una necesidad del alcance y está justificada.
- [x] Los requisitos y criterios de aceptación son verificables; el plan identifica cómo comprobar los comportamientos afectados y documenta si se requieren pruebas automatizadas según la especificación.
- [x] Se identifican las validaciones de entrada, necesidades de seguridad y tratamiento de datos sensibles pertinentes.
- [x] Los artefactos de especificación, planificación y tareas se redactan en español, conservando los identificadores técnicos cuando sea necesario.
- [x] Las decisiones no fijadas por la constitución quedan documentadas aquí, sin asumir tecnologías, patrones o modos de ejecución no aprobados.

## Estructura del proyecto

### Documentación de esta funcionalidad

```text
specs/004-properties-list-pagination/
├── spec.md
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── README.md
└── tasks.md
```

### Código fuente (raíz del repositorio)

```text
app/
├── backend/
│   ├── src/
│   │   └── RealtorApi/
│   │       ├── Domain/
│   │       │   └── Properties/
│   │       │       └── Property.cs
│   │       ├── Features/
│   │       │   └── Properties/
│   │       │       ├── GetPropertiesQuery.cs
│   │       │       ├── GetPropertiesRequest.cs
│   │       │       ├── GetPropertiesResponse.cs
│   │       │       ├── GetPropertiesValidator.cs
│   │       │       └── PropertiesSlice.cs
│   │       ├── Infrastructure/
│   │       │   └── Validation/
│   │       └── Program.cs
│   └── tests/
│       └── RealtorApiTests/
│           └── Properties/
│               └── GetPropertiesTests.cs
├── frontend/
│   └── src/
│       └── RealtorWeb/
└── Realtor.sln
```

**Decisión de estructura**: La funcionalidad se implementará en una feature específica dentro del backend `RealtorApi`, manteniendo la lógica de uso dentro del slice de propiedades y separando la validación del contrato de respuesta. Se reutiliza la entidad `Property` ya creada y se evita crear repositorios globales o controladores; la lógica se concentra en el caso de uso de consulta paginada, con DTOs de request/response y un validador dedicado que se integra con el filtro de validación centralizado.

## Registro de complejidad

| Decisión o complejidad | Necesidad que resuelve | Alternativa más simple descartada y motivo |
|-------------------------|------------------------|--------------------------------------------|
| Validación automática de `page` y `pageSize` con `FluentValidation` | Asegura que se rechacen parámetros no enteros, nulos o fuera de rango con respuestas HTTP 400 estándar | Validar manualmente dentro del endpoint: se descarta porque duplicaría la lógica en cada slice y no aprovecha la infraestructura centralizada |
| Orden estable por `Title` + `Id` | Mantiene el orden determinista y asegura coherencia entre páginas cuando varios títulos coinciden | Ordenar solo por título: se descarta porque puede causar páginas inconsistentes y resultados no repetibles |
| Construcción de `imageUrl` basada en `Request.Scheme/Host` | Garantiza que la URL sea absoluta pública y coincida con el host actual de la solicitud | Guardar rutas relativas o físicas del servidor: se descarta porque rompería el requisito de URL pública servida por la API |
| Metadatos calculados a partir del total y `ceil(total/pageSize)` | Produce un contrato de paginación consistente, con `hasNext` y `hasPrevious` coherentes | Calcular los metadatos con lógica ad hoc por página: se descarta porque puede desalinear la respuesta y crear inconsistencias entre páginas |
