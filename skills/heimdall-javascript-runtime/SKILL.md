---
name: heimdall-javascript-runtime
description: Use when reasoning about Heimdall's browser runtime, including script loading, DOM attribute scanning, triggers, synchronized requests, async request headers, client information, unauthorized handling, local-time processing, mutations, cancellation, swaps, SSE, and lifecycle behavior.
---

# Heimdall JavaScript Runtime

Use this skill when reasoning about the Heimdall browser runtime.

Official docs: https://heimdall-framework.org/javascript

The runtime is intentionally small. It observes Heimdall attributes, invokes server actions, processes returned HTML/directives, and applies swaps. It should not become a client-side application framework.

## Load The Runtime

```csharp
head.Script(script =>
{
    script.Src("/_content/HeimdallFramework.Web/heimdall-bundle.min.js");
});
```

Debug build:

```text
/_content/HeimdallFramework.Web/heimdall-bundle.js
```

## Runtime Responsibilities

- find Heimdall trigger attributes in rendered DOM
- listen for trigger events
- invoke document-visible actions when a hidden document becomes visible
- invoke online actions when browser connectivity returns
- expose offline transitions as the client-only `heimdall:offline` document event
- collect payloads
- coordinate parallel, replace, drop, and queue-latest requests
- send requests to content actions
- emit request and swap lifecycle events
- process response directives
- normalize and apply successful action history directives
- apply the configured swap
- handle SSE messages from Bifrost
- collect optional browser client information for content actions
- resolve optional asynchronous application request headers
- localize marked absolute times before content enters the DOM
- apply ordered in-place mutation directives
- invoke explicitly named JavaScript functions when directed

## Debugging Interactions

Inspect rendered HTML first:

1. Runtime script is loaded.
2. The element has exactly one Heimdall trigger attribute.
3. The trigger action ID matches a discovered content action.
4. The payload source can be resolved.
5. The target selector matches a real element.
6. The swap mode matches the returned fragment.
7. The synchronization strategy did not intentionally cancel, drop, or queue the request.
8. The element is not disabled.

## Request And Swap Hooks

Use the focused `heimdall-request-lifecycle` skill for the complete coordinator contract. The primary integration events are:

```text
heimdall:request-config
heimdall:request-before
heimdall:request-after
heimdall:request-finally
heimdall:request-cancel
heimdall:request-timeout
heimdall:swap-before
heimdall:swap-after
heimdall:unauthorized
heimdall:client-info-before
heimdall:time-before
heimdall:time-after
heimdall:time-error
heimdall:mutation-before
heimdall:mutation-after
heimdall:mutation-error
heimdall:history-before
heimdall:history-after
heimdall:history-error
heimdall:history-pop
heimdall:offline
```

`request-before` and `swap-before` are cancellable. Request configuration and pre-swap target/fragment/mode are mutable. Replace-mode stale responses are suppressed before DOM, OOB, JavaScript, or redirect effects.

`requestHeaders` is asynchronous and runs at request-attempt time before `request-before`; a rejection fails closed. `unauthorized` is cancellable only to suppress Heimdall's default `Location` redirect for a raw `401`.

`heimdall:offline` is not a request lifecycle event. It is dispatched on `document` with `event.detail.online === false`, is not cancellable, and never creates or queues a content action. The `Online` trigger performs a normal action attempt when connectivity returns; browser online status does not guarantee server reachability.

`history-before` is cancellable and may change `detail.mode` or `detail.url`. `history-after` reports normalized values. `history-error` reports invalid or duplicate directives. `history-pop` is cancellable and otherwise causes a normal page reload for managed Back/Forward traversal. Rootless history routes are origin-rooted: `orders/42` and `/orders/42` both become `/orders/42`.

Use `heimdall-time-localization`, `heimdall-mutations`, `heimdall-client-info`, and `heimdall-request-lifecycle` for the complete contracts of those runtime areas.

## Guidance

- Keep behavior visible in attributes or fluent helpers.
- Do not use the runtime as a general-purpose state manager.
- Do not generate JavaScript source from server responses.
- Prefer response directives for multi-target updates and explicit effects.
- Prefer Bifrost SSE for server-pushed HTML.
- Debug from rendered DOM outward.
