# Contratos de la feature

## Manifest de seed de propiedades

El seed debe resolverse a partir de un manifest central que defina el origen de los JSON y los assets externos a publicar. El contrato funcional esperado es:

```json
{
  "version": "1.0",
  "propertiesFile": "support/seed-data/properties.json",
  "statusFile": "support/seed-data/properties.statuses.json",
  "assets": [
    {
      "source": "support/seed-data/images/1.png",
      "target": "wwwroot/images/1.png",
      "publicPath": "/images/1.png"
    }
  ]
}
```

Reglas del contrato:
- El manifiesto es la única fuente de verdad para localizar los datos de seed y los activos externos.
- `publicPath` debe apuntar a una ruta servida por la aplicación, nunca a una ruta local del sistema de archivos.
- La ruta de la imagen persistida en `ImageUrl` debe coincidir con el valor de runtime expuesto por la API.
- El manifest debe integrarse en el build y publicarse con el proyecto para que los assets queden accesibles en ejecución.
