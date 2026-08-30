---
name: heimdall-payloads
description: Use when choosing Heimdall payload sources and binding data to content actions, including closest-form JSON or multipart data, file inputs and FormData, inline objects, empty payloads, closest-state, selector/self/ref payloads, ContentPayload, FromForm, and payload-source debugging.
---

# Heimdall Payloads

Use this skill when choosing what data a Heimdall trigger sends to a content action.

Official docs: https://heimdall-framework.org/payloads

Payloads should be explicit and close to the interaction boundary. Send the minimum data the action needs to render the next HTML state.

## Closest Form

Use for normal form submission and validation:

```csharp
form.Heimdall()
    .Submit("todos.add")
    .PayloadFromClosestForm()
    .Target("#todo-panel")
    .SwapOuter()
    .PreventDefault();
```

## Inline Payload

Use for stable server-rendered context:

```csharp
button.Heimdall()
    .Click("orders.archive")
    .Payload(new { Id = order.Id })
    .Target("#orders-list")
    .SwapOuter();
```

## Empty Payload

Use when the action needs no request body:

```csharp
button.Heimdall()
    .Click("orders.refresh")
    .PayloadEmptyObject()
    .Target("#orders-list")
    .SwapOuter();
```

## Closest State

Use when the nearest rendered state host owns the interaction context:

```csharp
host.Heimdall().State(new CounterState { Count = count });

button.Heimdall()
    .Click("counter.increment")
    .PayloadFromClosestState()
    .Target("#counter")
    .SwapOuter();
```

## Global Reference

Use only for explicit browser-owned values:

```csharp
button.Heimdall()
    .Click("search.run")
    .PayloadFromRef("window.App.search")
    .Target("#results")
    .SwapOuter();
```

## Action Binding

Use one payload parameter:

```csharp
[ContentInvocation("orders.archive")]
public static IHtmlContent Archive([ContentPayload] ArchiveOrderRequest request)
{
    return OrdersPanel.RenderArchived(request.Id);
}
```

The payload parameter may be marked with `[ContentPayload]`. Avoid ambiguous multiple body-like parameters.

## Multipart Payloads

A closest form containing file inputs is sent as `multipart/form-data`; a form without files remains JSON. The runtime preserves field names and file values through browser `FormData`.

```csharp
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

[ContentInvocation("documents.upload")]
public static IHtmlContent Upload(
    [FromForm] DocumentRequest request,
    [FromForm(Name = "document")] IFormFile file)
{
    return DocumentStatus.Render(request.Title, file.Length);
}
```

Programmatic invocation also accepts `FormData` directly:

```javascript
await Heimdall.invoke("documents.upload", new FormData(form), {
  target: "#document-status"
});
```

Use `[FromForm]` when the action should reject JSON and require multipart/form data. Use the focused `heimdall-forms` skill for supported file parameter shapes, request limits, and storage security.

## Guidance

- Use closest form for form fields.
- Let closest forms with files use multipart payloads; do not serialize `File` objects to JSON.
- Use inline payloads for simple IDs and stable values already known at render time.
- Use empty payloads for refresh/reload actions.
- Use closest state for small server-rendered state objects.
- Use ref payloads sparingly and only for explicit browser-owned data.
- Keep payload types small, named, and validation-friendly.
- Do not send entire client-side application state.
