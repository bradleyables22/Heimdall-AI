---
name: heimdall-mvc-integration
description: Use when integrating Heimdall with ASP.NET Core MVC or Razor, including MVC templates, Razor views with Heimdall attributes, rendering MVC partials through IHeimdallMvcRenderer, controller-local ContentInvocation methods, NonAction, and MVC-heavy app migration.
---

# Heimdall MVC Integration

Use this skill when Heimdall is used inside an MVC or Razor app.

Official docs: https://heimdall-framework.org/mvc

Heimdall does not care whether HTML came from FluentHtml, Razor views, MVC partials, or another template layer. The runtime sees rendered DOM attributes and swaps returned HTML.

## Template

For a new MVC-based Heimdall app:

```bash
dotnet new install HeimdallFramework.Templates.MvcApp
dotnet new heimdall-mvc -n MyHeimdallMvcApp
```

## Native Attributes In Razor

Use Heimdall attributes directly in Razor views when that is the clearest local style:

```cshtml
@model OrderFilter

<form
  id="order-filter"
  heimdall-content-submit="orders.filter"
  heimdall-payload-from="closest-form"
  heimdall-content-target="#orders-table"
  heimdall-content-swap="inner">
  <label for="order-search">Search</label>
  <input
    id="order-search"
    name="Search"
    type="search"
    value="@Model.Search"
    autocomplete="off"
    heimdall-content-input="orders.filter"
    heimdall-payload-from="closest-form"
    heimdall-debounce="300"
    heimdall-content-target="#orders-table"
    heimdall-content-swap="inner" />

  <button type="submit">Apply</button>
</form>

<div id="orders-table">
  @await Html.PartialAsync("_OrderList", Model)
</div>
```

The submit and input triggers can call the same content action, send the closest form as the payload, and update the table with returned partial HTML.

## Controller-Local Actions

A content action can live beside the related MVC controller. Use `[NonAction]` so MVC routing does not expose it as a normal controller action.

```csharp
[ContentInvocationPrefix("orders")]
public sealed class OrdersController(
    IOrderRepository orders,
    IHeimdallMvcRenderer views) : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    [NonAction]
    [ContentInvocation("filter")]
    public async Task<IHtmlContent> FilterRows(
        OrderFilter filter,
        CancellationToken ct)
    {
        var results = await orders.SearchAsync(filter, ct);
        return await views.PartialAsync("_OrderList", results, ct);
    }
}
```

## Separate Action Classes

Use a separate action class when the controller would become too crowded:

```csharp
[ContentInvocationPrefix("orders")]
public sealed class OrderActions(
    IOrderRepository orders,
    IHeimdallMvcRenderer views)
{
    [ContentInvocation("filter")]
    public async Task<IHtmlContent> FilterRows(
        OrderFilter filter,
        CancellationToken ct)
    {
        var results = await orders.SearchAsync(filter, ct);
        return await views.PartialAsync("_OrderList", results, ct);
    }
}
```

## MVC Renderer

`IHeimdallMvcRenderer` uses the real MVC view engine. It works well for:

- shared partials such as `_Header` and `_OrderList`
- application-relative partial paths
- models
- `ViewData`
- `TempData`
- tag helpers
- Razor dependency injection
- nested partials

## Guidance

- Keep MVC views as views and use Heimdall to progressively enhance interactions.
- Use native Heimdall attributes in Razor when FluentHtml would fight the local codebase.
- Return partial HTML from content actions, not JSON view state.
- Keep action IDs stable and dotted.
- Use controller-local actions for discoverability in MVC-heavy apps.
- Use separate action classes for cross-controller or domain-oriented interactions.
