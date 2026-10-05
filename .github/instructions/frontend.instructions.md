# Instrucciones de frontend

Estas instrucciones aplican a todo trabajo frontend del proyecto.

El frontend DEBE implementarse con:

- Blazor Web App.
- Razor Components.
- CSS propio centralizado en `wwwroot/app.css`.
- Refit para comunicación con backend.
- Sistema visual canónico definido en `wwwroot/app.css`.
- Skill visual personalizado para generar y mantener el diseño base.
- Lucide Icons como sistema principal de íconos.

Está prohibido usar frameworks CSS como Bootstrap CSS, Tailwind, Bulma o similares.

Está prohibido usar Bootstrap Icons como fuente principal de íconos.

---

## Relación con specs

Toda pantalla, componente, flujo, validación visual o cambio de UI DEBE estar descrito en la spec vigente.

Está prohibido implementar UI no descrita en `spec.md`.

Antes de modificar frontend:

1. Leer `spec.md`.
2. Leer `plan.md`.
3. Leer `tasks.md`.
4. Identificar la tarea exacta.
5. Implementar solo lo solicitado.
6. Usar únicamente estilos existentes o tokens definidos en `wwwroot/app.css`.
7. Usar Lucide Icons cuando la UI requiera íconos.
8. Marcar la tarea como `[X]` solo cuando esté completada y verificada.

Si el cambio visual no tiene tarea en `tasks.md`, la implementación DEBE detenerse.

---

## Sistema visual del frontend

El sistema visual base del frontend DEBE generarse y mantenerse siguiendo el skill:

```text
.github/skills/blazor-app-css-design-system/SKILL.md
```

El archivo canónico de estilos es:

```text
wwwroot/app.css
```

Si `wwwroot/app.css` todavía no existe, DEBE crearse únicamente cuando una spec aprobada lo solicite y exista una tarea correspondiente en `tasks.md`.

Está prohibido crear componentes visuales con estilos inline mientras `wwwroot/app.css` no exista.

Está prohibido inventar colores, espaciado, bordes, sombras o patrones visuales directamente dentro de componentes Razor.

---

## Sistema de íconos

El sistema principal de íconos del frontend DEBE ser Lucide Icons.

Lucide Icons DEBE usarse para:

- navegación principal
- acciones de botones
- estados visuales
- tarjetas
- formularios
- mensajes informativos
- dashboards
- elementos de gestión

Los íconos DEBEN renderizarse como SVG o componentes Razor.

Los íconos DEBEN heredar color mediante `currentColor` siempre que sea posible.

Los íconos DEBEN usar clases CSS definidas en `wwwroot/app.css`.

Está prohibido hardcodear colores, tamaños o estilos de íconos dentro de componentes Razor.

Está prohibido mezclar Lucide Icons, Bootstrap Icons, Fluent Icons, Font Awesome u otras librerías dentro de la misma interfaz sin una spec aprobada.

Bootstrap Icons solo puede usarse como excepción temporal si una spec lo justifica explícitamente.

### Instalación recomendada

Cuando la spec requiera habilitar íconos en Blazor, se recomienda usar una integración compatible con Lucide Icons para Blazor, por ejemplo un paquete de componentes Razor basado en Lucide.

La instalación exacta del paquete DEBE estar descrita en `plan.md` y trazada en `tasks.md`.

Ejemplo de tarea esperada:

```md
- [ ] Instalar y registrar la librería de Lucide Icons para Blazor.
- [ ] Crear clases CSS reutilizables para íconos en `wwwroot/app.css`.
- [ ] Reemplazar cualquier ícono temporal por Lucide Icons.
```

### Clases CSS esperadas para íconos

`wwwroot/app.css` DEBE contener clases reutilizables para íconos.

Ejemplo:

```css
.icon {
  width: 1.25rem;
  height: 1.25rem;
  color: currentColor;
  flex-shrink: 0;
}

.icon-sm {
  width: 1rem;
  height: 1rem;
}

.icon-md {
  width: 1.25rem;
  height: 1.25rem;
}

.icon-lg {
  width: 1.5rem;
  height: 1.5rem;
}

.icon-muted {
  color: var(--color-text-muted);
}

.icon-primary {
  color: var(--color-primary);
}

.icon-success {
  color: var(--color-success);
}

.icon-warning {
  color: var(--color-warning);
}

.icon-danger {
  color: var(--color-danger);
}
```

### Uso esperado en componentes Razor

Ejemplo conceptual correcto:

```razor
<button class="btn btn-primary">
    <LucideIcon Name="Plus" class="icon icon-sm" />
    Nueva propiedad
</button>
```

Ejemplo prohibido:

```razor
<i class="bi bi-plus" style="color: #2563eb; font-size: 20px;"></i>
```

---

## Estilo visual objetivo

La aplicación DEBE tener una apariencia moderna, limpia y profesional, inspirada visualmente en:

- Notion.
- Google Keep.
- Dashboards administrativos contemporáneos.
- Paneles modernos de gestión inmobiliaria.

La interfaz debe transmitir:

- claridad
- orden
- amplitud visual
- simplicidad
- foco en tarjetas de contenido
- navegación lateral clara

No se debe crear una interfaz recargada, oscura, saturada o con exceso de bordes.

---

## Layout principal

La aplicación DEBE usar una composición desktop first.

En escritorio, la estructura principal debe ser:

```text
sidebar fija a la izquierda + contenido principal amplio a la derecha
```

La sidebar DEBE permanecer fija o visualmente estable mientras el usuario navega por la aplicación.

El contenido principal DEBE tener suficiente espacio horizontal para mostrar grillas, formularios, paneles de gestión y tarjetas.

Clases esperadas en `wwwroot/app.css`:

```css
.app-shell {}
.app-sidebar {}
.app-main {}
.app-content {}
.page-container {}
.page-header {}
.page-title {}
.page-subtitle {}
.page-actions {}
```

---

## Sidebar

La sidebar DEBE:

- estar ubicada a la izquierda en escritorio
- tener ancho fijo
- usar fondo claro o ligeramente contrastado
- mostrar navegación vertical
- tener estados hover y active
- mantener separación visual con el contenido principal
- usar Lucide Icons para los ítems de navegación cuando se requieran íconos

Clases esperadas:

```css
.app-sidebar {}
.sidebar-brand {}
.sidebar-nav {}
.sidebar-link {}
.sidebar-link-active {}
```

En móvil, la sidebar puede convertirse en navegación superior, drawer o bloque colapsado, siempre que la spec lo permita.

---

## Contenido principal

El contenido principal DEBE:

- ocupar el espacio restante disponible
- tener padding amplio
- usar fondo general suave
- permitir layouts de dashboard y grillas
- evitar que los elementos queden pegados a los bordes

Clases esperadas:

```css
.app-main {}
.app-content {}
.page-container {}
.page-header {}
.page-actions {}
```

---

## Componentes Razor

Los componentes Razor deben ser pequeños, claros y orientados a una sola responsabilidad.

Evitar componentes grandes con demasiada lógica.

La lógica de negocio NO debe vivir en componentes UI.

Los componentes pueden contener:

- lógica de presentación
- estado visual local
- llamadas a servicios de aplicación frontend
- validaciones visuales simples
- composición de componentes hijos

No deben contener:

- reglas de negocio complejas
- lógica de persistencia
- acceso directo a `HttpClient`
- estilos inline
- colores hardcodeados
- íconos con estilos hardcodeados
- contratos duplicados manualmente sin spec

Ejemplo prohibido:

```razor
<button style="background-color: #2563eb; padding: 12px;">
    Guardar
</button>
```

Ejemplo correcto:

```razor
<button class="btn btn-primary">
    Guardar
</button>
```

---

## Comunicación con backend

El frontend DEBE consumir el backend mediante Refit.

Usar interfaces tipadas por módulo o feature.

Ejemplo:

```csharp
public interface IPropertiesApi
{
    [Get("/api/properties")]
    Task<IReadOnlyList<PropertySummaryResponse>> GetPropertiesAsync();
}
```

Está prohibido usar `HttpClient` directamente en componentes Razor.

Está prohibido usar `RestService.For<T>()` fuera del registro con `IHttpClientFactory`.

Las interfaces Refit deben estar organizadas por módulo o feature, no en un único archivo global enorme.

---

## CSS

Todo el CSS del proyecto debe estar centralizado en:

```text
wwwroot/app.css
```

`wwwroot/app.css` es la fuente técnica de verdad para:

- colores
- tipografías
- espaciado
- bordes
- sombras
- tamaños
- layout base
- sidebar
- botones
- formularios
- cards
- tablas
- badges
- íconos
- estados visuales
- utilidades reutilizables
- grillas responsive

Está prohibido crear archivos CSS adicionales por componente salvo que una spec aprobada lo indique explícitamente.

Está prohibido definir estilos inline dentro de componentes Razor.

Está prohibido usar colores hardcodeados en `.razor`, `.cs` o archivos CSS nuevos.

Ejemplos prohibidos:

```css
color: #2563eb;
background: blue;
border-radius: 10px;
margin: 17px;
```

Ejemplo correcto:

```css
color: var(--color-primary);
background: var(--color-surface);
border-radius: var(--radius-md);
margin: var(--space-4);
```

---

## Tokens visuales obligatorios

`wwwroot/app.css` DEBE contener tokens CSS dentro de `:root`.

Los componentes NO deben depender de valores hardcodeados.

### Colores base

```css
:root {
  --color-bg: #f8fafc;
  --color-surface: #ffffff;
  --color-surface-muted: #f1f5f9;

  --color-text: #0f172a;
  --color-text-muted: #64748b;

  --color-primary: #2563eb;
  --color-primary-hover: #1d4ed8;

  --color-success: #16a34a;
  --color-warning: #d97706;
  --color-danger: #dc2626;

  --color-border: #e2e8f0;
  --color-focus: #93c5fd;

  --color-sidebar: #ffffff;
}
```

### Espaciado

```css
:root {
  --space-1: 0.25rem;
  --space-2: 0.5rem;
  --space-3: 0.75rem;
  --space-4: 1rem;
  --space-6: 1.5rem;
  --space-8: 2rem;
  --space-12: 3rem;
}
```

### Bordes

```css
:root {
  --radius-sm: 0.375rem;
  --radius-md: 0.5rem;
  --radius-lg: 0.75rem;
  --radius-xl: 1rem;
}
```

### Sombras

```css
:root {
  --shadow-sm: 0 1px 2px rgb(15 23 42 / 0.08);
  --shadow-md: 0 8px 24px rgb(15 23 42 / 0.10);
}
```

### Layout

```css
:root {
  --sidebar-width: 260px;
  --content-max-width: 1440px;
}
```

---

## Clases reutilizables obligatorias

El agente DEBE preferir clases reutilizables existentes antes de crear nuevas clases.

`wwwroot/app.css` debe contener clases base para patrones comunes.

### Layout

```css
.app-shell {}
.app-sidebar {}
.app-main {}
.app-content {}
.page-container {}
.page-header {}
.page-title {}
.page-subtitle {}
.page-actions {}
```

### Botones

```css
.btn {}
.btn-primary {}
.btn-secondary {}
.btn-danger {}
.btn-ghost {}
```

Los botones deben usar tokens visuales, no valores hardcodeados.

### Cards

```css
.card {}
.card-header {}
.card-body {}
.card-footer {}
.property-card {}
```

Las tarjetas DEBEN tener:

- fondo blanco
- bordes redondeados
- sombras suaves
- borde sutil
- padding consistente
- separación visual clara entre tarjetas

Las tarjetas NO deben usar sombras agresivas, colores saturados ni bordes pesados.

### Formularios

```css
.form {}
.form-group {}
.form-label {}
.form-control {}
.form-error {}
.form-actions {}
```

### Estados de pantalla

```css
.loading-state {}
.empty-state {}
.error-state {}
.success-state {}
```

---

## Grilla de propiedades

En la página de propiedades, `properties.html` o su equivalente Razor, la grilla DEBE mostrar 3 cards por fila en escritorio.

La clase principal recomendada es:

```css
.properties-grid {}
```

Regla obligatoria en escritorio:

```css
.properties-grid {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: var(--space-6);
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

---

## Reglas para nuevos componentes visuales

Cuando una spec requiera un nuevo componente visual, el agente DEBE seguir este orden:

1. Revisar si ya existe una clase reutilizable en `wwwroot/app.css`.
2. Reutilizar la clase existente si aplica.
3. Si no existe, crear una clase nueva en `wwwroot/app.css`.
4. La nueva clase DEBE usar tokens existentes.
5. El componente Razor DEBE consumir la clase, no definir estilos inline.
6. Si el componente requiere íconos, DEBE usar Lucide Icons.
7. La creación de la clase DEBE estar trazada en `tasks.md`.

Ejemplo correcto:

```razor
<section class="property-card">
    <h3 class="property-card-title">@Property.Name</h3>
    <p class="property-card-meta">@Property.Address</p>
</section>
```

Ejemplo CSS correcto:

```css
.property-card {
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  box-shadow: var(--shadow-sm);
  padding: var(--space-4);
}

.property-card-title {
  color: var(--color-text);
  margin: 0 0 var(--space-2);
}

.property-card-meta {
  color: var(--color-text-muted);
  margin: 0;
}
```

---

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

---

## Accesibilidad

Toda UI debe considerar:

- contraste suficiente
- labels visibles o accesibles
- navegación por teclado
- estados focus
- mensajes de error claros
- botones con texto entendible
- estructura semántica HTML correcta
- íconos decorativos marcados como no relevantes para lectores de pantalla cuando aplique
- íconos informativos acompañados de texto visible o texto accesible

No usar color como único indicador de estado.

Ejemplo incorrecto:

```razor
<span class="status-dot status-danger"></span>
```

Ejemplo correcto:

```razor
<span class="badge badge-danger">
    Error de validación
</span>
```

---

## Formularios

Los formularios deben:

- mostrar errores cerca del campo correspondiente
- evitar validaciones contradictorias con backend
- usar mensajes claros
- deshabilitar acciones durante envío cuando aplique
- mostrar feedback de éxito o error
- usar clases reutilizables definidas en `wwwroot/app.css`

Los formularios deben preferir clases como:

```text
.form
.form-group
.form-label
.form-control
.form-error
.form-actions
```

---

## Estados de pantalla

Toda pantalla que consuma datos debe considerar:

- loading
- empty state
- error state
- success state
- estado con datos

No crear pantallas que solo funcionen en el caso feliz.

Ejemplo de estados esperados:

```razor
@if (isLoading)
{
    <div class="loading-state">Cargando propiedades...</div>
}
else if (hasError)
{
    <div class="error-state">No se pudieron cargar las propiedades.</div>
}
else if (!properties.Any())
{
    <div class="empty-state">No hay propiedades registradas.</div>
}
else
{
    <PropertyList Items="properties" />
}
```

---

## Prohibiciones visuales

Está prohibido:

- Usar Bootstrap CSS.
- Usar Bootstrap Icons como sistema principal de íconos.
- Usar Tailwind.
- Usar estilos inline.
- Usar colores hardcodeados.
- Crear CSS no trazado a `tasks.md`.
- Cambiar tokens visuales sin spec aprobada.
- Crear variantes visuales por preferencia estética.
- Duplicar estilos existentes con nombres nuevos.
- Introducir librerías UI sin aprobación en spec.
- Mezclar múltiples librerías de íconos sin spec aprobada.
- Colocar lógica de negocio en componentes UI.
- Crear una grilla de propiedades que no respete 3 cards por fila en escritorio.

---

## Checklist antes de finalizar una tarea frontend

Validar:

- La UI está descrita en `spec.md`.
- La tarea existe en `tasks.md`.
- No se agregó framework CSS.
- No se usó `HttpClient` directamente.
- Se usó Refit para consumir backend.
- Se usó Lucide Icons para íconos cuando aplica.
- No se usó Bootstrap Icons como sistema principal.
- No hay estilos inline.
- No hay colores hardcodeados.
- Los íconos usan clases CSS y `currentColor` cuando aplica.
- Se respetaron los tokens visuales de `wwwroot/app.css`.
- Se reutilizaron clases existentes cuando fue posible.
- Toda clase nueva fue agregada a `wwwroot/app.css`.
- La app tiene sidebar izquierda en escritorio cuando aplica.
- El contenido principal es amplio y legible.
- Las tarjetas son blancas, redondeadas y con sombra suave.
- La grilla de propiedades usa 3 cards por fila en escritorio.
- La grilla baja a 2 columnas en tablet.
- La grilla baja a 1 columna en móvil.
- La pantalla contempla loading, empty y error state cuando aplica.
- No hay lógica de negocio dentro del componente.
- El proyecto compila.
