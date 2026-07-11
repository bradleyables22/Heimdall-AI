---
name: heimdall-javascript-interop
description: Use when invoking explicit JavaScript functions from Heimdall responses, including JsInvokeVoidBefore, JsInvokeVoidAfter, function path safety, JSON arguments, redirect/abort/SSE interactions, and avoiding eval or client-side rendering.
---

# Heimdall JavaScript Interop

Use this skill when a Heimdall response needs an explicit browser-side effect.

Official docs: https://heimdall-framework.org/javascript-interop

JavaScript interop participates in the same response directive model as other Heimdall directives. Use it for browser effects, not for rendering UI.

## Basic Pattern

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

Static helper with abort:

```csharp
return Html.Fragment(
    HeimdallHtml.Abort("validation-failed"),
    HeimdallHtml.JsInvokeVoid(
        "window.App.form.shake",
        "#checkout-form"));
```

## SSE Payloads

Streamed Bifrost HTML can also carry JavaScript directives:

```csharp
fragment.Heimdall()
    .JsInvokeVoidAfter(
        "window.App.notifications.pulse",
        "orders");
```

## No Registration Step

The server names an explicit browser function path. The browser resolves that path at runtime.

Server owns:

```csharp
HeimdallHtml.JsInvokeVoid("window.App.toast.success", "Saved")
```

Browser owns:

```javascript
window.App.toast.success = message => {
  // show toast
};
```

## Safety Model

Allowed:

- call an existing rooted function path
- pass JSON arguments

Not allowed:

- evaluating JavaScript source
- dynamic imports
- bracket expressions
- bare global names
- using return values

Function paths must start with `window.`, `globalThis.`, or `document.`.

Redirect wins and stops later work. Abort can suppress the main swap while still allowing directive processing.

## Guidance

- Use JavaScript interop for focus, clipboard, analytics, toasts, scroll, and small browser effects.
- Return HTML for UI rendering.
- Keep functions owned and tested in browser JavaScript.
- Keep server directives explicit and narrow.
- Do not use interop to reintroduce a SPA render path.
