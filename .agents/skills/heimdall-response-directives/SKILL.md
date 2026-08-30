---
name: heimdall-response-directives
description: Use when a Heimdall response needs directives such as out-of-band DOM updates, in-place mutations, aborting the main swap, redirects, browser-history updates, or explicit JavaScript void calls before or after a swap.
---

# Heimdall Response Directives

Use this skill when a Heimdall response needs to update multiple DOM targets, mutate existing nodes, abort the main swap, redirect, update browser history, or invoke explicit JavaScript void functions.

Official docs: https://heimdall-framework.org/out-of-band

Response directives are special HTML elements that Heimdall processes before applying the main swap.

## In-Place Mutations

Use mutations when attributes, classes, or Heimdall state should change without replacing a node:

```csharp
return FluentHtml.Fragment(fragment =>
{
    fragment.Add(MainPanel.Render(model));
    fragment.Heimdall().Mutate("#status", mutation => mutation
        .RemoveAttr("aria-busy")
        .RemoveClass("loading")
        .AddClass("ready")
        .State(new { model.Version }));
});
```

Use the focused `heimdall-mutations` skill for `MutateAll`, root and descendant scopes, raw directive shape, lifecycle hooks, behavior reconciliation, and state operations.

## Out-Of-Band Updates

Use out-of-band invocations when a response should update more than one target.

```csharp
public static partial class NotesPanel
{
    [ContentInvocationPrefix("notes")]
    public sealed class NotesPanelActions(NoteStore notes)
    {
        [ContentInvocation("save")]
        public IHtmlContent Save([ContentPayload] NotePayload payload)
            => FluentHtml.Fragment(fragment =>
            {
                var model = notes.Save(payload);

                fragment.Add(NotesForm.Render(model.Form));
                fragment.Heimdall().Invocation(
                    targetSelector: "#notes-list",
                    swap: HeimdallHtml.Swap.Inner,
                    payload: NotesList.Render(model.Notes));
            });
    }
}
```

Static helper form:

```csharp
return Html.Fragment(
    NotesForm.Render(),
    HeimdallHtml.Invocation(
        targetSelector: "#notes-list",
        swap: HeimdallHtml.Swap.Inner,
        payload: NotesList.Render(notes)));
```

Set `wrapInTemplate: true` when the payload should be wrapped in a `<template>` element.

## Abort

Use abort to suppress the main target swap while still allowing other directives to run.

```csharp
return FluentHtml.Fragment(fragment =>
{
    fragment.Add(ValidationSummary.Render(errors));
    fragment.Heimdall().Invocation("#form-errors", payload: ErrorList.Render(errors));
    fragment.Heimdall().Abort("validation-failed");
});
```

Static helper:

```csharp
return Html.Fragment(
    HeimdallHtml.Abort("validation-failed"),
    ErrorSummary.Render(errors));
```

## Redirect

Use redirect when the browser should navigate after an action:

```csharp
return HeimdallHtml.Redirect("/login");
```

Fluent fragment form:

```csharp
return FluentHtml.Fragment(fragment =>
{
    fragment.Heimdall().Redirect("/orders/complete");
});
```

## Browser History

Use history when a successful content action should expose a canonical, shareable URL without navigating immediately:

Official history docs: https://heimdall-framework.org/history

```csharp
return FluentHtml.Fragment(fragment =>
{
    fragment.Add(OrderPanel.Render(order));
    fragment.Heimdall().HistoryPush("orders/42");
});

fragment.Heimdall().HistoryReplace("/orders/42?tab=activity");
```

Raw primitives:

```html
<history mode="push" url="orders/42"></history>
<history mode="replace" url="/orders/42?tab=activity"></history>
```

Rules:

- `orders/42` and `/orders/42` both normalize to `/orders/42`; plain paths are rooted at the current origin, not the current directory.
- Same-origin absolute URLs, query-only URLs, and hash-only URLs are supported.
- Cross-origin, protocol-relative, blank, and backslash-containing URLs are rejected.
- One action response may contain at most one history directive.
- Push adds a history entry; replace changes the current entry.
- There is no response `pop` command. Browser Back and Forward fully load the canonical GET route.
- History runs after successful action swaps and before JavaScript-after directives.
- Redirect wins, error responses do not apply history, and abort suppresses only the main swap so explicit history still applies.
- History directives in Bifrost SSE messages are stripped and ignored.

Map every canonical history URL as a real page route. Use `heimdall-pages-routing-middleware` for route guidance.

## JavaScript Void Invocation

Use JavaScript void invocation only for explicit browser effects, not for rendering UI. Return HTML for UI updates.

```csharp
return FluentHtml.Fragment(fragment =>
{
    fragment.Add(SavedBanner.Render());
    fragment.Heimdall().JsInvokeVoidAfter("window.App.toast.success", "Saved");
});
```

Before-swap variant:

```csharp
fragment.Heimdall().JsInvokeVoidBefore("document.body.focus");
```

Rules:

- Function paths must be explicitly rooted at `window.`, `globalThis.`, or `document.`.
- Bare paths such as `App.toast.success` are invalid.
- Bracket notation such as `window.App['toast']` is invalid.
- Arguments are serialized as a JSON array.
- Return values are ignored.
- Heimdall does not evaluate JavaScript source from responses.
- A redirect is a hard stop and prevents JavaScript invocation.

## Directive Order

Out-of-band invocations and mutations are processed in response order. A later mutation can therefore target a node created by an earlier invocation.

- Directives run before the normal main-target swap.
- Abort suppresses the main swap while allowing response directives to run.
- Redirect wins for the response and prevents mutation, invocation, JavaScript, and main-swap effects.
- A successful action applies one history directive after swaps; redirect suppresses it.
- Non-success responses do not apply response directives.

Keep order intentional when one directive depends on another.

## Directive-First Pattern

When combining directives, keep the returned fragment easy to read:

```csharp
return FluentHtml.Fragment(fragment =>
{
    fragment.Add(MainTarget.Render(model));

    fragment.Heimdall().Invocation(
        targetSelector: "#sidebar",
        payload: Sidebar.Render(model));

    fragment.Heimdall().JsInvokeVoidAfter("window.App.analytics.track", "saved");
});
```

Keep directive-heavy actions beside the component that owns the main swap target. Use out-of-band directives for sibling regions, not as an excuse to scatter unrelated UI behavior through a generic action class.
