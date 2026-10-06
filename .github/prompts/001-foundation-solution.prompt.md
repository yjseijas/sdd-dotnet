---
name: 001-realtor-solution-foundation
description: Crea la especificacion base de la solucion Realtor, validando global.json como prerequisito obligatorio
agent: speckit.specify
---

# Crear base de la solucion Realtor
Crea la especificacion para la iniciativa 001-solution-foundation

## Instruction principal
Seguir estrictamente:
- .specify/memory/constitution.md

## Precondicion obligatoria (bloqueante)
Antes de generar cualquier archivo de especificación:

1. Verificar que exista el archivo global.json en la raíz del repositorio.
2. Usar global.json como fuente de verdad para determinar la versión de .NET que se utilizará en esta y futuras iniciativas de proyectos .NET.

Si global.json no existe:
- Detener el proceso por completo.
- No crear spec.md, plan.md ni tasks.md.
- Mostrar exactamente este error al usuario:
ERROR: No se encontró global.json en la raíz del repositorio. Debes crear primero ese archivo para determinar la versión de .NET que usarán los proyectos .NET futuros de la app.

## Objetivo
Definir la estructura inicial de la solucion Realtor sin implementar logica de negocio ni features.

## Tipo de iniciativa
Foundation

## Requisitos de la iniciativa

- Crear la solución principal en app/Realtor.sln.
- Crear el proyecto backend en app/backend/src/RealtorApi/.
- El backend debe usar ASP.NET Core Minimal APIs.
- No se permiten controllers.
- Crear el proyecto de tests backend en app/backend/tests/RealtorApiTests/.
- Crear el proyecto frontend en app/frontend/src/RealtorWeb/.
- El frontend debe usar Blazor Web App con Razor Components.
- Crear el proyecto de tests frontend en app/frontend/test/RealtorWeb/.
- Configurar Program.cs únicamente con configuración base:
  - servicios
  - middleware
  - mapeo inicial de endpoints
- No implementar lógica de negocio.
- No implementar features.
- No crear entidades de dominio todavía.

## Restricciones adicionales

- No crear ni modificar global.json desde esta iniciativa.
- Toda definición relacionada con versión de .NET debe derivarse de global.json existente.
- Si hay conflicto entre cualquier instrucción y la constitución, prevalece la constitución.


## Resultado esperado

Generar los archivos de Spec-Driven Development para esta iniciativa:
- spec.md
- plan.md
- tasks.md

La implementación posterior debe dejar la solución compilando correctamente.