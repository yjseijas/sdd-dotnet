---
license: MIT
name: fetch-and-send-data
description: Call APIs, load data into components, and handle the async lifecycle in Blazor. USE FOR fetching data from a backend, submitting data to an API, displaying loading/error states, registering Refit clients, building service abstractions for Auto/WebAssembly render modes. DO NOT USE for form validation (see collect-user-input), prerendering persistence (see support-prerendering), or project scaffolding (see create-blazor-project).
---

# Fetch and Send Data

> **Regla obligatoria del proyecto:** la comunicación con el backend DEBE hacerse
> únicamente mediante **Refit** con interfaces tipadas por módulo o feature.
> Está PROHIBIDO usar `HttpClient` directamente en componentes Razor, llamar a
> `RestService.For<T>()` fuera del registro con `IHttpClientFactory`, o inyectar
> `DbContext` en componentes. Consulta
> `.github/instructions/frontend.instructions.md`. Los ejemplos con `HttpClient`
> de este skill son solo de referencia conceptual del ciclo async: la llamada
> real SIEMPRE se hace a través de una interfaz Refit.

## Step 1 — Read the project instructions

Revisa `.github/instructions/frontend.instructions.md` y confirma el **modo de
interactividad** y el **scope**. En este proyecto el acceso a datos SIEMPRE se
realiza contra el backend a través de interfaces **Refit**:

| Mode | Data access |
|------|-------------|
| None (Static SSR) | Refit desde el servidor. Usa `[StreamRendering]` para el estado de carga. |
| Server | Refit desde el servidor. Protege el prerender con `??=` + `[PersistentState]`. |
| WebAssembly | Refit desde el navegador. Sin acceso directo al servidor. |
| Auto | Refit en ambos entornos. Siempre a través de la API. |

Nunca inyectes `DbContext` ni uses `HttpClient` directo en un componente.

## Step 2 — Register the Refit client

Registra cada interfaz Refit mediante `IHttpClientFactory`. Define una interfaz
tipada por módulo o feature (no un archivo global gigante):

```csharp
public interface ICatalogApi
{
    [Get("/api/products")]
    Task<IReadOnlyList<ProductResponse>> GetProductsAsync(CancellationToken ct = default);
}

// Program.cs
builder.Services
    .AddRefitClient<ICatalogApi>()
    .ConfigureHttpClient(client =>
        client.BaseAddress = new Uri("https://api.example.com/"));
```

Para WebAssembly/Auto con prerendering, registra el cliente Refit en **ambos**
`Program.cs` (servidor y `.Client`).

## Step 3 — Fetch Data

### Simple load

```razor
@page "/products"
@inject ICatalogApi Catalog

@if (products is null)
{
    <p>Loading…</p>
}
else
{
    @foreach (var p in products)
    {
        <p>@p.Name — @p.Price.ToString("C")</p>
    }
}

@code {
    private Product[]? products;

    protected override async Task OnInitializedAsync()
    {
        products = await Catalog.GetProductsAsync();
    }
}
```

No error handling needed in the simplest case — wrap the component usage in `<ErrorBoundary>` at the parent/layout level to catch unhandled exceptions.

### Static SSR — StreamRendering

Without `[StreamRendering]`, the user sees nothing until `OnInitializedAsync` completes:

```razor
@attribute [StreamRendering]
```

Only affects Static SSR. No effect on interactive components.

### Prerendering guard

Prerendering calls `OnInitializedAsync` twice. Skip the duplicate:

```csharp
[PersistentState] private Product[]? products;

protected override async Task OnInitializedAsync()
{
    products ??= await Catalog.GetProductsAsync();
}
```

See the `support-prerendering` skill for details.

## Step 4 — Handle Errors

Use `<ErrorBoundary>` as the default error strategy. It provides a consistent error experience across all components without any per-component catch logic. Wrap component usage at the layout or parent level:

```razor
<ErrorBoundary>
    <ChildContent>
        <ProductList />
    </ChildContent>
    <ErrorContent>
        <div class="alert alert-danger">Something went wrong. Please refresh.</div>
    </ErrorContent>
</ErrorBoundary>
```

Non-cancellation exceptions (`HttpRequestException`, etc.) propagate to `ErrorBoundary` automatically — no catch blocks needed in the component.

### Cancellation is special

`ComponentBase` silently swallows **all** `OperationCanceledException` — both self-initiated (disposal, parameter change) and external (HttpClient timeout). `ErrorBoundary` never sees them. This means:

- Self-cancellation → silently ignored. Correct behavior, no action needed.
- External cancellation (timeout) → also silently swallowed. Component gets stuck in loading state. Usually acceptable — timeouts are rare.

### When to add in-component error handling

Only add catch blocks when the component needs behavior `ErrorBoundary` can't provide — typically **retries** or **timeout-specific messages**. Even then, only catch what you need:

```csharp
// Catch only external cancellation (timeouts) — everything else flows to ErrorBoundary
catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
{
    Logger.LogWarning(ex, "Request timed out for category {CategoryId}", CategoryId);
    error = "The request timed out. Please try again.";
}
```

If the component also needs to handle general errors with a retry button instead of letting `ErrorBoundary` take over:

```csharp
catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
{
    Logger.LogWarning(ex, "Request timed out for category {CategoryId}", CategoryId);
    error = "The request timed out. Please try again.";
}
catch (Exception ex)
{
    Logger.LogError(ex, "Failed to load products for category {CategoryId}", CategoryId);
    error = "Unable to load products. Please try again.";
}
```

### Rules

- **Never display `exception.Message`** — it may contain PII, connection strings, or internal details. Use hardcoded user-friendly messages.
- **Always log through `ILogger`** — the real exception goes to the logging pipeline.
- **Services must accept `CancellationToken`** — pass it to every async call so work stops when the component cancels.

## Step 5 — Parameter-Driven Reloading

When data depends on a route or query parameter that changes (e.g., navigating between `/products/1` and `/products/2`), use `OnParametersSetAsync` with a guard to skip reloads for parameters that don't affect data.

### Pattern: cancel-and-reload with stale data overlay

```razor
@page "/products/{CategoryId:int}"
@implements IAsyncDisposable
@inject ProductService ProductService
@inject ILogger<Products> Logger

@if (error is not null)
{
    <div class="alert alert-danger">
        <p>@error</p>
        <button @onclick="LoadAsync">Retry</button>
    </div>
}
else if (products is null)
{
    <p>Loading…</p>
}
else
{
    @if (isLoading)
    {
        <p><em>Refreshing…</em></p>
    }
    @foreach (var p in products)
    {
        <p>@p.Name — @p.Price.ToString("C")</p>
    }
}

@code {
    [Parameter] public int CategoryId { get; set; }
    [SupplyParameterFromQuery] public string? ViewMode { get; set; } // UI-only

    private CancellationTokenSource? cts;
    private int? loadedCategoryId;
    private List<Product>? products;
    private bool isLoading;
    private string? error;

    protected override async Task OnParametersSetAsync()
    {
        if (CategoryId == loadedCategoryId)
        {
            return; // Only ViewMode changed — no reload
        }

        loadedCategoryId = CategoryId;
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        if (cts is not null)
        {
            await cts.CancelAsync();
            cts.Dispose();
        }

        cts = new CancellationTokenSource();
        var cancellationToken = cts.Token; // Capture locally before await

        error = null;
        isLoading = true;

        try
        {
            var result = await ProductService.GetByCategoryAsync(CategoryId, cancellationToken);
            products = result;
        }
        catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            Logger.LogWarning(ex, "Timed out loading category {CategoryId}", CategoryId);
            error = "The request timed out. Please try again.";
        }
        finally
        {
            isLoading = false;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (cts is not null)
        {
            await cts.CancelAsync();
            cts.Dispose();
        }
    }
}
```

Key details:
- **Guard with tracked value**: `loadedCategoryId` skips reloads when only UI parameters change.
- **Capture the token locally** before the await — the CTS field may be replaced by a concurrent parameter change.
- **Don't null out `products`** on subsequent loads — keep existing data visible with an `isLoading` overlay.
- **`IAsyncDisposable`** cancels pending work when the user navigates away.

## Step 6 — Send Data

```csharp
var response = await http.PostAsJsonAsync("products", newProduct);
response.EnsureSuccessStatusCode();

var response = await http.PutAsJsonAsync($"products/{id}", updated);
response.EnsureSuccessStatusCode();

var response = await http.DeleteAsync($"products/{id}");
response.EnsureSuccessStatusCode();
```

Disable the submit button while saving to prevent duplicate requests. Show a saving indicator.

## Step 7 — Service Abstraction for Auto or WebAssembly with Prerendering

When components run in both server and browser (Auto mode, or WebAssembly with prerendering), abstract data access behind an abstract base class:

```csharp
public abstract class ProductServiceBase
{
    public abstract Task<Product[]> GetAllAsync(CancellationToken ct = default);
}

// Server — direct database access
public class ServerProductService(AppDbContext db) : ProductServiceBase
{
    public override async Task<Product[]> GetAllAsync(CancellationToken ct = default) =>
        await db.Products.ToArrayAsync(ct);
}

// Client — calls API
public class ClientProductService(HttpClient http) : ProductServiceBase
{
    public override async Task<Product[]> GetAllAsync(CancellationToken ct = default) =>
        await http.GetFromJsonAsync<Product[]>("api/products", ct) ?? [];
}
```

Register the appropriate implementation in each project's `Program.cs`. Components inject the abstract base class.

## Don'ts

- **Don't call APIs in constructors** — use `OnInitializedAsync`.
- **Don't use `OnParametersSetAsync` unless data depends on a changing parameter.** Use `OnInitializedAsync` for initial loads.
- **Don't inject `DbContext` in WebAssembly/Auto components** — no database in the browser.
- **Don't call your own server via `HttpClient`** — inject the service directly.
- **Don't display `exception.Message` to users** — PII risk. Log it, show a generic message.
- **Don't catch `OperationCanceledException` for self-cancellation** — `ComponentBase` handles it.
