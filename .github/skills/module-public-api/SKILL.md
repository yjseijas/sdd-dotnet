---
name: module-public-api
description: Define contratos públicos para comunicación entre módulos cuando existen límites modulares reales.
---

# Skill: Module Public API

## Estado

Skill condicional. No usar en una aplicación sin módulos funcionales explícitos.

## Objetivo

Evitar acceso directo a internals de otro módulo y mantener contratos estables de comunicación.

## Cuándo usar

Solo cuando la spec o el plan aprueben límites modulares reales.

## Reglas

Un módulo NO debe acceder directamente a:

- entidades internas de otro módulo
- su DbContext interno
- handlers internos
- servicios internos
- infraestructura interna

La comunicación debe utilizar:

```text
PublicApi
```

o:

```text
Events
```

`PublicApi` puede exponer:

- interfaces
- requests
- responses
- resultados públicos
- contratos necesarios entre módulos

## Prohibiciones

- crear proyectos PublicApi especulativamente
- compartir entidades internas
- resolver el DbContext de otro módulo
- invocar handlers internos directamente
- filtrar infraestructura interna a consumidores externos

## Checklist

- [ ] Existen límites modulares reales aprobados.
- [ ] El contrato público es mínimo.
- [ ] No expone entidades internas.
- [ ] No expone DbContext ni infraestructura.
- [ ] No se creó infraestructura especulativa.
