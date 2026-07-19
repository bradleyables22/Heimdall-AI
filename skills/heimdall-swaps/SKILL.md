---
name: heimdall-swaps
description: Use when choosing or debugging Heimdall swap behavior, including target selectors, inner, outer, beforeend, afterbegin, none, fragment shape, append/prepend behavior, swap lifecycle hooks, and matching returned HTML to DOM update intent.
---

# Heimdall Swaps

Use this skill when choosing how returned HTML should be applied to the DOM.

Official docs: https://heimdall-framework.org/swaps

A content action returns HTML. The target selector chooses where the HTML goes. The swap mode chooses how it is applied.

## Common Swap Modes

```csharp
button.Heimdall()
    .Click("orders.refresh")
    .PayloadEmptyObject()
    .Target("#orders-list")
    .SwapOuter();
```

Use:

- `SwapInner()` when the response contains the target's children.
- `SwapOuter()` when the response replaces the whole target element.
- `SwapBeforeEnd()` when appending to the target.
- `SwapAfterBegin()` when prepending to the target.
- `SwapNone()` when response directives or side effects are the main result.

## Match Fragment Shape To Swap Mode

If targeting `#orders-list`:

```html
<ul id="orders-list">...</ul>
```

Use `SwapOuter()` when returning:

```html
<ul id="orders-list">
  <li>Order A</li>
</ul>
```

Use `SwapInner()` when returning:

```html
<li>Order A</li>
```

Use `SwapBeforeEnd()` when returning:

```html
<li>New order</li>
```

## Swap None

Use `SwapNone()` when the response uses directives:

```csharp
form.Heimdall()
    .Submit("contact.submit")
    .PayloadFromClosestForm()
    .SwapNone();
```

Then the action can choose targets:

```csharp
return HeimdallHtml.Invocation(
    targetSelector: "#statusMessage",
    swap: HeimdallHtml.Swap.Inner,
    payload: StatusMessage.Render("Saved"));
```

## Swap Lifecycle

Action, invocation-directive, and SSE swaps emit the same events:

```javascript
document.addEventListener("heimdall:swap-before", event => {
  // Mutable: event.detail.target, fragment, and swap.
  // event.preventDefault() skips this swap.
});

document.addEventListener("heimdall:swap-after", event => {
  console.log(event.detail.appliedRoot);
});
```

Detail identifies `origin` (`action` or `sse`) and `kind` (`main` or `invocation`). Mutated fragments are sanitized again before application. Use `heimdall-request-lifecycle` for the complete request and swap event contract.

## Guidance

- Always choose the target and returned fragment together.
- Use stable IDs for targets that actions update repeatedly.
- Avoid broad selectors for dynamic interactions.
- Prefer `SwapOuter()` for component-like render functions that include their own root ID.
- Prefer `SwapInner()` for stable containers that should keep their identity.
- Prefer append/prepend swaps for feeds, logs, notifications, and live streams.
- Prefer `SwapNone()` when the response uses out-of-band directives.
- Keep swap hooks narrow; do not move UI ownership into a hidden client renderer.
