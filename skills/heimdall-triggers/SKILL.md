---
name: heimdall-triggers
description: Use when choosing Heimdall triggers for interactions, including load, click, change, input, submit, keydown, blur, hover, visible, scroll, trigger attributes, event-to-action mapping, and trigger debugging.
---

# Heimdall Triggers

Use this skill when deciding when a Heimdall interaction should invoke a server action.

Official docs: https://heimdall-framework.org/triggers

A trigger says when to invoke. Payload says what to send. Target and swap say how returned HTML is applied.

## Common Triggers

Prefer the explicit `On*` helpers when matching documentation examples. The short helpers are aliases and are also valid:

```csharp
.OnLoad("feed.initial")
.OnClick("orders.refresh")
.OnChange("filters.changed")
.OnInput("search.preview")
.OnSubmit("form.save")
.OnKeyDown("command.enter")
.OnBlur("field.validate")
.OnHover("card.preview")
.OnVisible("feed.more")
.OnScroll("feed.more")

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

Static helpers emit the same attributes:

```csharp
HeimdallHtml.OnLoad("feed.initial")
HeimdallHtml.OnClick("orders.refresh")
HeimdallHtml.OnChange("filters.changed")
HeimdallHtml.OnInput("search.preview")
HeimdallHtml.OnSubmit("form.save")
HeimdallHtml.OnKeyDown("command.enter")
HeimdallHtml.OnBlur("field.validate")
HeimdallHtml.OnHover("card.preview")
HeimdallHtml.OnVisible("feed.more")
HeimdallHtml.OnScroll("feed.more")
```

Raw HTML attributes are:

```html
heimdall-content-load="feed.initial"
heimdall-content-click="orders.refresh"
heimdall-content-change="filters.changed"
heimdall-content-input="search.preview"
heimdall-content-submit="form.save"
heimdall-content-keydown="command.enter"
heimdall-content-blur="field.validate"
heimdall-content-hover="card.preview"
heimdall-content-visible="feed.more"
heimdall-content-scroll="feed.more"
```

## Examples

Click:

```csharp
button.Heimdall()
    .OnClick("orders.refresh")
    .PayloadEmptyObject()
    .Target("#orders-list")
    .SwapOuter();
```

Submit:

```csharp
form.Heimdall()
    .OnSubmit("orders.filter")
    .PayloadFromClosestForm()
    .Target("#orders-table")
    .SwapInner();
```

Input preview:

```csharp
input.Heimdall()
    .OnInput("search.preview")
    .PayloadFromClosestForm()
    .DebounceMs(300)
    .Target("#search-results")
    .SwapInner();
```

Visible:

```csharp
sentinel.Heimdall()
    .OnVisible("feed.more")
    .Payload(new { Cursor = nextCursor })
    .Target("#feed")
    .SwapBeforeEnd();
```

## Guidance

- Use `Click` for buttons and explicit commands.
- Use `Submit` / `OnSubmit` for forms; the runtime prevents the native form submit by default for Heimdall-managed submit triggers.
- Use `Input` for live search, validation, and previews.
- Use `Change` for lower-frequency field or filter changes.
- Use `Load` for initial fragment loading.
- Use `Visible` for reveal/load-more patterns; visible triggers are observed with `IntersectionObserver` and default to firing once unless `.VisibleOnce(false)` / `heimdall-visible-once="false"` is set.
- Use `Scroll` only when scroll position itself matters.
- Use `KeyDown` with `.Key(...)` for keyboard commands.
- Use `.DebounceMs(...)` for noisy `Input` or `Change` triggers; input has a runtime default debounce.
- Use `.HoverDelayMs(...)` for hover previews; hover has a runtime default delay.
- Use `.PreventDefault()` explicitly when a click trigger is on a link or command surface whose browser default should not run.
- Keep triggers close to their targets and payload sources in rendered HTML.
