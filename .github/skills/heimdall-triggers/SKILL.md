---
name: heimdall-triggers
description: Use when choosing Heimdall triggers for interactions, including load, click, change, input, submit, keydown, blur, hover, visible, scroll, trigger attributes, event-to-action mapping, and trigger debugging.
---

# Heimdall Triggers

Use this skill when deciding when a Heimdall interaction should invoke a server action.

Official docs: https://heimdall-framework.org/triggers

A trigger says when to invoke. Payload says what to send. Target and swap say how returned HTML is applied.

## Common Triggers

```csharp
.Load("feed.initial")
.Click("orders.refresh")
.Change("filters.changed")
.Input("search.preview")
.Submit("form.save")
.KeyDown("command.enter")
.Blur("field.validate")
.Hover("card.preview")
.Visible("feed.more")
.Scroll("feed.more")
```

## Examples

Click:

```csharp
button.Heimdall()
    .Click("orders.refresh")
    .PayloadEmptyObject()
    .Target("#orders-list")
    .SwapOuter();
```

Submit:

```csharp
form.Heimdall()
    .Submit("orders.filter")
    .PayloadFromClosestForm()
    .Target("#orders-table")
    .SwapInner()
    .PreventDefault();
```

Input preview:

```csharp
input.Heimdall()
    .Input("search.preview")
    .PayloadFromClosestForm()
    .DebounceMs(300)
    .Target("#search-results")
    .SwapInner();
```

Visible:

```csharp
sentinel.Heimdall()
    .Visible("feed.more")
    .Payload(new { Cursor = nextCursor })
    .Target("#feed")
    .SwapBeforeEnd()
    .VisibleOnce();
```

## Guidance

- Use `Click` for buttons and explicit commands.
- Use `Submit` for forms.
- Use `Input` for live search, validation, and previews.
- Use `Change` for lower-frequency field or filter changes.
- Use `Load` for initial fragment loading.
- Use `Visible` for reveal/load-more patterns.
- Use `Scroll` only when scroll position itself matters.
- Use `KeyDown` with `.Key(...)` for keyboard commands.
- Keep triggers close to their targets and payload sources in rendered HTML.
