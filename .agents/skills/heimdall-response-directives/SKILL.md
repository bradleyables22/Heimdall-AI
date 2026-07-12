---
name: heimdall-response-directives
description: Use when a Heimdall content action needs response directives such as out-of-band DOM updates, aborting the main swap, redirects, or explicit JavaScript void calls before or after a swap.
---

# Heimdall Response Directives

Use this skill when a Heimdall content action needs to update multiple DOM targets, abort the main swap, redirect, or invoke explicit JavaScript void functions.

Official docs: https://heimdall-framework.org/out-of-band

Response directives are special HTML elements that Heimdall processes before applying the main swap.

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
