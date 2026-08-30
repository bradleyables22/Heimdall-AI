---
name: heimdall-triggers
description: Use when choosing Heimdall triggers for interactions, including load, click, change, input, submit, keydown, blur, hover, element visibility, document visibility, online connectivity, scroll, the client-only offline event, trigger attributes, event-to-action mapping, and trigger debugging.
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
.OnDocumentVisible("dashboard.refresh")
.OnOnline("connection.restore")
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
.DocumentVisible("dashboard.refresh")
.Online("connection.restore")
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
HeimdallHtml.OnDocumentVisible("dashboard.refresh")
HeimdallHtml.OnOnline("connection.restore")
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
heimdall-content-document-visible="dashboard.refresh"
heimdall-content-online="connection.restore"
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

Document visible:

```csharp
dashboard.Heimdall()
    .DocumentVisible("dashboard.refresh")
    .PayloadEmptyObject()
    .Target("#dashboard")
    .SwapInner();
```

`DocumentVisible` runs on each hidden-to-visible document transition. It does not run during initial boot; combine it with `Load` when both initial and return-time refreshes are required.

Online:

```csharp
connectionPanel.Heimdall()
    .Online("connection.restore")
    .PayloadEmptyObject()
    .Target("#connection-panel")
    .SwapInner();
```

An online browser event indicates that connectivity may be available again; it does not guarantee that the application server is reachable. Normal request error handling still applies.

Offline is intentionally client-only and never invokes or queues a content action:

```javascript
document.addEventListener("heimdall:offline", event => {
    console.log(event.detail.online); // false
});
```

## Guidance

- Use `Click` for buttons and explicit commands.
- Use `Submit` / `OnSubmit` for forms; the runtime prevents the native form submit by default for Heimdall-managed submit triggers.
- Use `Input` for live search, validation, and previews.
- Use `Change` for lower-frequency field or filter changes.
- Use `Load` for initial fragment loading.
- Use `Visible` for element reveal/load-more patterns; visible triggers are observed with `IntersectionObserver` and default to firing once unless `.VisibleOnce(false)` / `heimdall-visible-once="false"` is set.
- Use `DocumentVisible` to refresh when a user returns to a hidden tab. It is document visibility, not element visibility.
- Use `Online` to attempt a refresh after the browser reports restored connectivity.
- Handle `heimdall:offline` locally for offline UI. Do not expect a server invocation or deferred action queue.
- Use `Scroll` only when scroll position itself matters.
- Use `KeyDown` with `.Key(...)` for keyboard commands.
- Use `.DebounceMs(...)` for noisy `Input` or `Change` triggers; input has a runtime default debounce.
- Use `.HoverDelayMs(...)` for hover previews; hover has a runtime default delay.
- Use `.PreventDefault()` explicitly when a click trigger is on a link or command surface whose browser default should not run.
- Keep triggers close to their targets and payload sources in rendered HTML.
