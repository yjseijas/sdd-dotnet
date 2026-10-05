# Skill: Blazor App CSS Design System

## Cuándo usar este skill

Usar este skill cuando una spec requiera crear o modificar el sistema visual del frontend Blazor, especialmente el archivo:

```text
wwwroot/app.css
```

Este skill aplica para:

- layout principal de la aplicación
- sidebar fija
- contenido principal
- tarjetas
- grillas
- formularios
- botones
- estados visuales
- adaptación responsive
- páginas como `properties.html` o componentes equivalentes en Razor

## Regla principal

El archivo `wwwroot/app.css` es la fuente técnica de verdad del sistema visual del frontend.

Está prohibido crear estilos inline dentro de componentes Razor.

Está prohibido usar frameworks CSS como Bootstrap CSS, Tailwind, Bulma o similares.

El sistema principal de íconos DEBE ser Lucide Icons. Está prohibido usar Bootstrap Icons, Font Awesome, Fluent Icons u otras librerías de íconos como fuente principal. Los íconos DEBEN renderizarse como SVG o componentes Razor, heredar color mediante `currentColor` y usar clases CSS de íconos definidas en `wwwroot/app.css`.

Toda clase nueva DEBE estar trazada a una tarea en `tasks.md`.

## Estilo visual objetivo

La aplicación DEBE tener una apariencia moderna, limpia y profesional, inspirada visualmente en:

- Notion
- Google Keep
- dashboards administrativos contemporáneos
- paneles de gestión inmobiliaria modernos

La interfaz debe transmitir:

- claridad
- orden
- amplitud visual
- simplicidad
- foco en tarjetas de contenido
- navegación lateral clara

No se debe crear una interfaz recargada, oscura, saturada o con exceso de bordes.

## Layout principal

La aplicación DEBE usar una composición desktop first.

En escritorio, la estructura principal debe ser:

```text
sidebar fija a la izquierda + contenido principal amplio a la derecha
```

La sidebar DEBE permanecer fija o visualmente estable mientras el usuario navega por la aplicación.

El contenido principal DEBE tener suficiente espacio horizontal para mostrar grillas, formularios y paneles de gestión.

Clases esperadas:

```css
.app-shell
.app-sidebar
.app-main
.app-content
.page-container
.page-header
.page-title
.page-subtitle
```

## Sidebar

La sidebar DEBE:

- estar ubicada a la izquierda en escritorio
- tener ancho fijo
- usar fondo claro o ligeramente contrastado
- mostrar navegación vertical
- tener estados hover y active
- mantener separación visual con el contenido principal

Clases esperadas:

```css
.app-sidebar
.sidebar-brand
.sidebar-nav
.sidebar-link
.sidebar-link-active
```

## Contenido principal

El contenido principal DEBE:

- ocupar el espacio restante disponible
- tener padding amplio
- usar fondo general suave
- permitir layouts de dashboard y grillas
- evitar que los elementos queden pegados a los bordes

Clases esperadas:

```css
.app-main
.app-content
.page-container
.page-header
.page-actions
```

## Tarjetas

Las tarjetas DEBEN tener:

- fondo blanco
- bordes redondeados
- sombras suaves
- borde sutil
- padding consistente
- separación visual clara entre tarjetas

Clases esperadas:

```css
.card
.card-header
.card-body
.card-footer
.property-card
```

Las tarjetas NO deben usar sombras agresivas, colores saturados ni bordes pesados.

## Grilla de propiedades

En la página de propiedades, `properties.html` o su equivalente Razor, la grilla DEBE mostrar 3 cards por fila en escritorio.

La clase principal recomendada es:

```css
.properties-grid
```

Regla obligatoria en escritorio:

```css
.properties-grid {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
}
```

En pantallas medianas puede reducirse a 2 columnas.

En pantallas móviles debe reducirse a 1 columna.

Ejemplo esperado:

```css
.properties-grid {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: var(--space-6);
}

@media (max-width: 1024px) {
  .properties-grid {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}

@media (max-width: 640px) {
  .properties-grid {
    grid-template-columns: 1fr;
  }
}
```

## Responsive design

La composición es desktop first.

En escritorio:

- sidebar fija a la izquierda
- contenido principal amplio
- grilla de propiedades con 3 columnas

En tablet:

- sidebar puede mantenerse compacta
- grillas pueden bajar a 2 columnas

En móvil:

- sidebar puede convertirse en navegación superior, drawer o bloque colapsado
- contenido principal debe usar una sola columna
- cards deben ocupar todo el ancho disponible

## Tokens requeridos

El archivo `wwwroot/app.css` DEBE definir tokens en `:root` para:

- colores
- texto
- fondos
- bordes
- sombras
- espaciado
- radios
- transiciones
- ancho de sidebar
- ancho máximo de contenido

Ejemplo base:

```css
:root {
  --color-bg: #f8fafc;
  --color-surface: #ffffff;
  --color-surface-muted: #f1f5f9;

  --color-text: #0f172a;
  --color-text-muted: #64748b;

  --color-primary: #2563eb;
  --color-primary-hover: #1d4ed8;

  --color-border: #e2e8f0;
  --color-sidebar: #ffffff;

  --space-2: 0.5rem;
  --space-3: 0.75rem;
  --space-4: 1rem;
  --space-6: 1.5rem;
  --space-8: 2rem;

  --radius-md: 0.5rem;
  --radius-lg: 0.75rem;
  --radius-xl: 1rem;

  --shadow-sm: 0 1px 2px rgb(15 23 42 / 0.08);
  --shadow-md: 0 8px 24px rgb(15 23 42 / 0.10);

  --sidebar-width: 260px;
  --content-max-width: 1440px;
}
```

## Proceso obligatorio

Cuando una spec requiera crear el sistema visual base:

1. Confirmar que existe una tarea en `tasks.md`.
2. Crear o actualizar `wwwroot/app.css`.
3. Definir tokens visuales en `:root`.
4. Crear layout principal con sidebar y contenido.
5. Crear clases base para cards.
6. Crear clases base para botones.
7. Crear clases base para formularios.
8. Crear clases base para estados de pantalla.
9. Crear grilla `.properties-grid` con 3 columnas en escritorio.
10. Validar adaptación a tablet y móvil.
11. Confirmar que los componentes Razor usan clases CSS y no estilos inline.
12. Marcar la tarea como `[X]` solo cuando el resultado esté verificado.

## Prohibiciones

Está prohibido:

- Usar estilos inline.
- Usar colores hardcodeados dentro de componentes Razor.
- Crear CSS fuera de `wwwroot/app.css` sin spec aprobada.
- Cambiar tokens por criterio estético personal.
- Usar Bootstrap CSS.
- Usar Tailwind.
- Usar Bootstrap Icons, Font Awesome, Fluent Icons u otra librería de íconos distinta de Lucide Icons como fuente principal.
- Hardcodear colores, tamaños o estilos de íconos dentro de componentes Razor.
- Crear una grilla de propiedades que no respete 3 cards por fila en escritorio.
- Crear componentes visuales sin tarea en `tasks.md`.
- Implementar cambios visuales no descritos en `spec.md`.

## Checklist de revisión

Antes de finalizar una tarea visual, validar:

- [ ] Existe tarea en `tasks.md`.
- [ ] `wwwroot/app.css` existe.
- [ ] Los tokens base están definidos en `:root`.
- [ ] La app tiene sidebar izquierda en escritorio.
- [ ] El contenido principal es amplio y legible.
- [ ] Las tarjetas son blancas, redondeadas y con sombra suave.
- [ ] La grilla de propiedades usa 3 cards por fila en escritorio.
- [ ] La grilla baja a 2 columnas en tablet.
- [ ] La grilla baja a 1 columna en móvil.
- [ ] No hay estilos inline.
- [ ] No hay colores hardcodeados en componentes Razor.
- [ ] No se agregó ningún framework CSS.
