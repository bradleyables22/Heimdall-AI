---
name: heimdall-review
description: Use when reviewing Heimdall code or generated output for correctness, including imports, real package/API usage, IHtmlContent rendering, encoding, content actions, payloads, swaps, response directives, Bifrost SSE, architecture fit, and anti-patterns.
---

# Heimdall Review

Use this skill when reviewing Heimdall code or checking generated output.

Official docs: https://heimdall-framework.org/security

Prioritize correctness, security, and whether the code actually follows Heimdall's native HTML-first model:

```text
event -> server action -> HTML -> targeted DOM update
```

## Imports And Packages

- Uses `Heimdall.Server` for page mapping, content actions, services, and Bifrost.
- Uses `Heimdall.Server.Rendering` for `Html`, `FluentHtml`, and `HeimdallHtml`.
- Uses `Microsoft.AspNetCore.Html` for `IHtmlContent` return values.
- Uses `Heimdall.Bootstrap.Bootstrap` only when the Bootstrap package is installed.
- Does not invent Heimdall package names, endpoint paths, helper names, or browser-runtime paths.

## Rendering

- Pages, layouts, fragments, and action responses return `IHtmlContent`.
- New markup uses `FluentHtml` unless local code clearly prefers lower-level `Html` helpers.
- Dynamic or user-provided strings use `.Text(...)`.
- `.Raw(...)` is limited to trusted static markup such as `<!DOCTYPE html>`.
- Rendering modules stay as ordinary C# functions, not Razor components or SPA state machines.
- App-owned CSS classes are typed constants near the owning component, such as `NotesPanel.Css.Root`.
- Framework classes use typed helpers such as `Bootstrap.*` when available.
- Repeated raw app class strings are treated as maintainability issues.

## Interactions

- User interactions are expressed on rendered HTML with `.Heimdall()`.
- Invocation IDs are stable and dotted, such as `orders.filter` or `todos.add`.
- Targets are stable and specific, such as `#orders-list` or `#todo-panel`.
- Swap mode matches the returned fragment:
  - `SwapOuter()` when replacing the selected target.
  - `SwapInner()` when replacing children.
  - `SwapBeforeEnd()` when appending.
  - `SwapNone()` when directives or side effects are the main result.
- Forms use `PayloadFromClosestForm()` when submitting user input.
- Buttons with static context use `.Payload(...)` or `.PayloadEmptyObject()`.

## Content Actions

- Content actions use `[ContentInvocation]` or `[ContentInvocationPrefix]`.
- Action methods return `IHtmlContent`, `Task<IHtmlContent>`, or `ValueTask<IHtmlContent>`.
- Action responses return HTML fragments, not JSON UI state.
- Payload binding uses one payload object, optionally marked with `[ContentPayload]`.
- Dependencies are supplied by constructor injection, implicit service parameters, or `[FromServices]`.
- Long-running actions honor `CancellationToken` and request timeout metadata.
- Authorization uses ASP.NET Core metadata such as `[Authorize]`.
- Actions are colocated with the rendered component or partial they primarily update.
- Component-owned actions use a nested `ComponentNameActions` class, split to `Component.Actions.cs` with partial classes when needed.
- Static action methods are limited to tiny, pure, or demo interactions.
- Real persistence, repositories, Bifrost, MVC rendering, logging, and other services use instance action classes with constructor DI.
- Top-level action classes are reserved for cross-component or domain workflows.
- MVC controller-local content methods use `[NonAction]` as a safety convention.
- Static mutable state is flagged unless it is clearly demo-only.

## Response Directives

- Multi-target updates use Heimdall response directives instead of ad hoc JavaScript rendering.
- Validation failures use `Abort(...)` when the main swap should be suppressed.
- Redirects use Heimdall redirect directives.
- JavaScript void invocation is limited to explicit browser effects.
- JavaScript function paths are rooted at `window.`, `globalThis.`, or `document.`.
- Responses do not evaluate JavaScript source.

## Bifrost SSE

- SSE subscriptions use stable topic names and explicit targets.
- Published SSE payloads are HTML fragments.
- Named events are used when one topic feeds multiple UI regions.
- Bifrost authorization is configured for user-specific or sensitive topics.
- TTL values are short enough for stale messages to expire naturally.

## Architecture

- The UI follows `event -> server action -> HTML -> targeted DOM update`.
- Server rendering owns UI state wherever practical.
- JavaScript stays small and explicit.
- MVC controllers are not used as the normal route for Heimdall content actions.
- Component or partial boundaries own their host IDs, action IDs, payload models, CSS constants, render methods, and local actions.
- SPA shells are introduced only when explicitly requested.
