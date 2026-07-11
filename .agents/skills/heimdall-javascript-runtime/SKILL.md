---
name: heimdall-javascript-runtime
description: Use when reasoning about Heimdall's browser runtime, including runtime script loading, DOM attribute scanning, trigger handling, request lifecycle, swaps, response directive processing, runtime debugging, and lifecycle behavior.
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
- collect payloads
- send requests to content actions
- process response directives
- apply the configured swap
- handle SSE messages from Bifrost
- invoke explicitly named JavaScript functions when directed

## Debugging Interactions

Inspect rendered HTML first:

1. Runtime script is loaded.
2. The element has exactly one Heimdall trigger attribute.
3. The trigger action ID matches a discovered content action.
4. The payload source can be resolved.
5. The target selector matches a real element.
6. The swap mode matches the returned fragment.
7. The element is not disabled.

## Guidance

- Keep behavior visible in attributes or fluent helpers.
- Do not use the runtime as a general-purpose state manager.
- Do not generate JavaScript source from server responses.
- Prefer response directives for multi-target updates and explicit effects.
- Prefer Bifrost SSE for server-pushed HTML.
- Debug from rendered DOM outward.
