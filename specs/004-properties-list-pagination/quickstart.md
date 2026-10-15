# Guía rápida de validación

## Prerrequisitos

- SDK .NET 11 definido en `global.json`.
- Proyecto `RealtorApi` compilable y datos de propiedades disponibles en la base configurada.
- Si se valida endpoint real, arrancar la aplicación localmente con PostgreSQL o SQLite de pruebas.

## Validación de compilación y ensamblaje

```bash
dotnet restore
dotnet build app/backend/src/RealtorApi/RealtorApi.csproj
```

Resultado esperado:
- La aplicación compila sin errores.
- El slice de propiedades queda registrado con la infraestructura base `AddSlices` / `MapSlices`.
- El endpoint `GET /api/properties` queda disponible sin introducir controladores ni nuevas capas globales.

## Validación del comportamiento de paginación

```bash
dotnet run --project app/backend/src/RealtorApi/RealtorApi.csproj
```

Luego consultar:

```bash
curl -i "http://localhost:5000/api/properties"
curl -i "http://localhost:5000/api/properties?page=2&pageSize=6"
curl -i "http://localhost:5000/api/properties?page=999&pageSize=6"
curl -i "http://localhost:5000/api/properties?page=0&pageSize=3"
curl -i "http://localhost:5000/api/properties?page=1&pageSize=101"
```

Resultado esperado:
- La primera consulta devuelve la página 1, con máximo 6 elementos y metadatos `page`, `pageSize`, `totalItems`, `totalPages`, `hasNext`, `hasPrevious`.
- La segunda consulta devuelve la página 2 con las propiedades correctas y un orden por `Title` + `Id`.
- Si la página supera el rango, devuelve `200` con la colección vacía y metadatos coherentes.
- Si `page` o `pageSize` son inválidos, devuelve `400` con `ValidationProblemDetails`.

## Validación de URL pública de imagen

Verificar que cada `imageUrl` responda a una URL absoluta del tipo:

```text
http(s)://{host}/assets/properties/{fileName}
```

Resultado esperado:
- No aparecen rutas locales del sistema de archivos.
- No aparecen rutas relativas sin host.
- La respuesta del contrato mantiene el valor del `imageUrl` servible por la aplicación.

## Validación del orden y los metadatos

Para un conjunto de 13 propiedades con `pageSize=6`:
- la primera página devuelve 6 elementos,
- la segunda 6 elementos,
- la tercera 1 elemento,
- `totalItems` es 13 y `totalPages` es 3.

La consulta debe mantenerse estable en las páginas por `Title` ascendente y `Id` ascendente como desempate.

## Criterios de cierre

La feature se considera lista cuando:
- compilación exitosa,
- endpoint `GET /api/properties` responde con las páginas esperadas,
- validación de parámetros con `400` para casos inválidos,
- `imageUrl` usa host y esquema actuales,
- orden, metadatos y paginación se mantienen consistentes y verificables.
