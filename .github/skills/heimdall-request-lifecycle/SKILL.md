---
name: heimdall-request-lifecycle
description: Use when coordinating Heimdall browser requests, preventing stale results, configuring parallel/replace/drop/queue-latest, understanding queued payload and target snapshots, setting timeouts or AbortSignal cancellation, resolving asynchronous request headers, using Heimdall.invoke, or integrating request, unauthorized, and swap lifecycle events.
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
-> queued payload/target refresh when execution begins
-> antiforgery and client-info preparation
-> async requestHeaders
-> request-before (cancellable)
-> fetch and response processing
-> request-after, or request-cancel/request-timeout
-> request-finally (always)
```

`request-before` runs for each fetch attempt. Cancellation through `preventDefault()` uses the `event-cancelled` reason.

## Asynchronous Request Headers

Use the global async provider for credentials that must be fresh when an attempt actually starts:

```javascript
Heimdall.config.requestHeaders = async context => {
  const token = await auth.getAccessToken(context.signal);
  return token ? { Authorization: `Bearer ${token}` } : {};
};
```

The context contains `kind`, `url`, `method`, `actionId`, `topic`, `requestId`, `attempt`, `sourceElement`, `signal`, and prepared `headers`. Kinds are `content-action`, `csrf-token`, and `bifrost-token`; action-only values and the abort signal are `null` for internal token requests.

The provider may mutate `context.headers`, return a `Headers` instance, header pairs, a plain object, or return nothing. Header precedence is:

```text
framework headers
-> provider mutations
-> provider return value
-> explicit Heimdall.invoke/request-config headers
-> request-before mutations
```

Names are merged case-insensitively. A provider rejection fails closed with `ok: false`, status `0`, code `request-headers-failed`, and `heimdall:error` phase `request-headers`. Normal finalization still runs.

The provider also authenticates CSRF-token and Bifrost token-minting requests. It cannot add headers to the native `EventSource` stream. Filter both `context.kind` and `context.url` before attaching credentials to configurable endpoints.

## Unauthorized Responses

Raw `401 Unauthorized` responses from content actions, CSRF-token requests, and Bifrost token requests emit the cancellable `heimdall:unauthorized` event:

```javascript
document.addEventListener("heimdall:unauthorized", event => {
  event.preventDefault(); // Suppress a Location-based redirect.
  showLoginOrNavigate(event.detail);
});
```

The detail contains request kind and identifiers, URL, method, status, body, optional normalized redirect URL, and the browser `Response`. Preventing default does not make the request succeed or suppress normal error reporting. The event is intentionally limited to `401`; `403` means an authenticated caller lacks permission.

## Queue-Latest Data And Target Rules

Queued work behaves like a real invocation when execution begins while preserving input the user already submitted:

- Closest-form fields and selected files are snapshotted when the trigger fires.
- Explicit and programmatic payloads remain the submitted values.
- Closest-state payloads are re-read when queued execution begins so an earlier response can advance state first.
- A `request-config` payload replacement remains authoritative.
- Selector targets are re-resolved after waiting and again after out-of-band directives that may replace them.
- A disconnected direct-element target cancels with `target-disconnected`.
- A missing or replaced closest-state source cancels with `payload-source-unavailable`.
- A newer queued item replaces the older pending item with `queue-replaced`.
- An antiforgery retry reuses the attempt's payload snapshot instead of silently reading newer state or form values.

These rules prevent stale DOM references while avoiding surprising changes to a form submission.

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
- Queue-latest re-resolves live selector and state boundaries while preserving submitted form and file data.
- Request-header failures do not send the protected request.
- Request timeouts and external abort signals use the normal cancellation result and lifecycle.
- Existing `heimdall:before`, `heimdall:after`, `heimdall:error`, `heimdall:abort`, and `heimdall:redirect` events remain supported.
- `heimdall:abort` represents the server `<abort>` directive; client cancellation uses `heimdall:request-cancel`.

## Guidance

- Prefer `replace` plus debounce for type-ahead search.
- Prefer named groups only when multiple elements truly share one request lane.
- Use lifecycle events for observability and narrow integration, not as a hidden client application layer.
- Honor `context.signal` in async header providers so replaced, aborted, or timed-out work can stop promptly.
- Keep action IDs, payloads, targets, swaps, and synchronization visible in markup or typed helpers.
