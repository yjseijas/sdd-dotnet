# Guía de validación: Listado paginado de propiedades

**Fecha**: 2026-10-13  
**Especificación**: [spec.md](./spec.md)  
**Contrato HTTP**: [properties-list.openapi.json](./contracts/properties-list.openapi.json)

## Requisitos previos

- SDK .NET 11 compatible con la versión fijada por el repositorio.
- Acceso a la configuración de base de datos del entorno; la aplicación aplica las migraciones y ejecuta el seed durante el arranque.
- Datos de propiedades y assets del manifiesto de seed disponibles en el repositorio.

## Iniciar el backend

Desde la raíz del repositorio:

```powershell
dotnet run --project app\backend\src\RealtorApi\RealtorApi.csproj
```

Usa la URL base indicada por el perfil local de ASP.NET Core. Si se valida específicamente el host `http://localhost:5023`, inicia la aplicación con ese host configurado para el entorno local.

## Solicitudes manuales

Abrir `app/backend/src/RealtorApi/RealtorApi.http`, configurar la variable de host con la URL del proceso y ejecutar las solicitudes:

```http
GET {{RealtorApi_HostAddress}}/api/properties
Accept: application/json

###

GET {{RealtorApi_HostAddress}}/api/properties?page=1&pageSize=6
Accept: application/json

###

GET {{RealtorApi_HostAddress}}/api/properties?page=2&pageSize=6
Accept: application/json
```

## Resultados esperados

1. Ambas solicitudes responden `200 OK`.
2. La primera devuelve `page: 1`, `pageSize: 6`, `items` con hasta seis elementos, ordenados por `title` ascendente y por `id` ascendente cuando los títulos coinciden.
3. La segunda incluye el mismo conjunto de primera página y los metadatos `totalItems`, `totalPages`, `hasNext` y `hasPrevious` correctos.
4. La tercera devuelve la página 2. Con 13 propiedades disponibles, contiene 6 elementos, y el total es 13 con 3 páginas.
5. El estado de cada elemento es una cadena. Cada `imageUrl` es absoluta y tiene el formato `http(s)://{host}/assets/properties/{fileName}`.
6. Las URLs de imagen devueltas cargan los assets servidos por la API. La ruta previa `/images/properties/{fileName}` permanece disponible.
7. Las solicitudes con `page=0`, `pageSize=0`, `pageSize=101` o valores no enteros responden `400` con detalles de validación.
8. Una página más allá de la última responde `200` con `items: []` y metadatos calculados del total.

## Verificación unitaria

Desde la raíz, ejecutar el proyecto de pruebas:

```powershell
dotnet test app\backend\tests\RealtorApiTests\RealtorApiTests.csproj
```

Las pruebas de esta funcionalidad verifican el validator, la conversión del estado y URL y el cálculo de metadatos. El recorrido HTTP completo se valida con las solicitudes anteriores y no se agrega una prueba de integración automatizada.
