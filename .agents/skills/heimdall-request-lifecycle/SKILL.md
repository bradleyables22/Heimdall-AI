---
name: heimdall-request-lifecycle
description: Use when coordinating overlapping Heimdall browser requests, preventing stale search or navigation results, configuring parallel/replace/drop/queue-latest behavior, setting client timeouts or AbortSignal cancellation, using Heimdall.invoke, or integrating request and swap lifecycle DOM events.
---

# Heimdall Request Lifecycle

Use Heimdall's built-in coordinator and DOM events instead of adding a client request library.

Official docs:

- https://heimdall-framework.org/modifiers
- https://heimdall-framework.org/configuration
- https://heimdall-framework.org/javascript

## Choose A Synchronization Strategy

- `parallel`: run independently. This is the backward-compatible default.
- `replace`: cancel active work and let the newest request win. Prefer for search, filters, previews, and navigation-like replacement.
- `drop`: ignore new work while one request is active. Prefer for non-repeatable commands.
- `queue-latest`: retain only the latest pending request. Prefer when one final refresh must run after active work finishes.

Without a group, synchronization is scoped to the triggering element. Use a shared group when separate elements must coordinate.

## Declarative And Typed Setup

```html
<input
  heimdall-content-input="search.query"
  heimdall-content-target="#results"
  heimdall-debounce="250"
  heimdall-sync="replace"
  heimdall-sync-group="search">
```

```csharp
input.Heimdall()
    .OnInput("search.query")
    .Target("#results")
    .DebounceMs(250)
    .SyncReplace("search");
```

Available fluent helpers are `SyncParallel`, `SyncReplace`, `SyncDrop`, and `SyncQueueLatest`. The static equivalents are:

```csharp
HeimdallHtml.Sync(HeimdallHtml.RequestSync.Replace)
HeimdallHtml.SyncGroup("search")
```

Do not add synchronization attributes when parallel behavior is correct.

## Programmatic Invocation

Use the same coordinator through `Heimdall.invoke`:

```javascript
const controller = new AbortController();

const result = await Heimdall.invoke("search.query", { query }, {
  target: "#results",
  swap: "inner",
  sync: "replace",
  syncGroup: "search",
  timeoutMs: 5000,
  signal: controller.signal
});

if (result.cancelled) {
  console.log(result.cancelReason);
}
```

Expected cancellation resolves normally with `cancelled: true`; do not treat it as a Heimdall error.

Global defaults remain migration-safe:

```javascript
Heimdall.config.requestSync = "parallel";
Heimdall.config.requestTimeoutMs = 0;
```

A timeout of `0` means no client timeout.

## Request Lifecycle Events

Declarative request events originate from the triggering element and bubble to `document`:

```javascript
document.addEventListener("heimdall:request-config", event => {
  event.detail.headers["X-Correlation-ID"] = crypto.randomUUID();
  event.detail.sync = "replace";
  event.detail.syncGroup = "search";
});

document.addEventListener("heimdall:request-before", event => {
  // event.preventDefault() cancels before fetch.
});

document.addEventListener("heimdall:request-after", event => {});
document.addEventListener("heimdall:request-finally", event => {});
document.addEventListener("heimdall:request-cancel", event => {});
document.addEventListener("heimdall:request-timeout", event => {});
```

Order the integration around this pipeline:

```text
request-config (mutable)
-> synchronization
-> request-before (cancellable)
-> fetch and response processing
-> request-after, or request-cancel/request-timeout
-> request-finally (always)
```

`request-before` runs for each fetch attempt. Cancellation through `preventDefault()` uses the `event-cancelled` reason.

## Swap Lifecycle Events

Action, invocation-directive, and SSE swaps share the same hooks:

```javascript
document.addEventListener("heimdall:swap-before", event => {
  // Mutable: target, fragment, and swap.
  // event.preventDefault() skips this swap.
});

document.addEventListener("heimdall:swap-after", event => {});
```

Swap detail identifies `origin` (`action` or `sse`), `kind` (`main` or `invocation`), `sourceElement`, and `requestContext`. Successful `swap-after` detail also includes `appliedRoot`. Mutated fragments are sanitized again before application.

## Correctness Guarantees

- Replace-mode stale responses cannot mutate the DOM, apply OOB updates, invoke JavaScript, or redirect.
- Request timeouts and external abort signals use the normal cancellation result and lifecycle.
- Existing `heimdall:before`, `heimdall:after`, `heimdall:error`, `heimdall:abort`, and `heimdall:redirect` events remain supported.
- `heimdall:abort` represents the server `<abort>` directive; client cancellation uses `heimdall:request-cancel`.

## Guidance

- Prefer `replace` plus debounce for type-ahead search.
- Prefer named groups only when multiple elements truly share one request lane.
- Use lifecycle events for observability and narrow integration, not as a hidden client application layer.
- Keep action IDs, payloads, targets, swaps, and synchronization visible in markup or typed helpers.
