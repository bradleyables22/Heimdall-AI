---
name: heimdall-payloads
description: Use when choosing Heimdall payload sources and binding data to content actions, including closest-form, inline object payloads, empty payloads, closest-state, selector/self/ref payloads, ContentPayload, and payload-source debugging.
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

## Guidance

- Use closest form for form fields.
- Use inline payloads for simple IDs and stable values already known at render time.
- Use empty payloads for refresh/reload actions.
- Use closest state for small server-rendered state objects.
- Use ref payloads sparingly and only for explicit browser-owned data.
- Keep payload types small, named, and validation-friendly.
- Do not send entire client-side application state.
