---
name: heimdall-html-attributes
description: Use when writing or debugging raw Heimdall and native HTML attributes, especially in Razor or hand-authored markup, including heimdall-content-* triggers, payloads, targets, swaps, synchronization attributes, command/commandfor, modifiers, and rendered DOM implementation checks.
---

# Heimdall HTML Attributes

Use this skill when writing or debugging Heimdall attributes directly in rendered HTML.

Official docs: https://heimdall-framework.org/html-attributes

Fluent helpers and Razor attributes both produce the same runtime contract. When something does not work, inspect the rendered DOM.

## Razor Attribute Example

```html
<form
  id="order-filter"
  heimdall-content-submit="orders.filter"
  heimdall-payload-from="closest-form"
  heimdall-content-target="#orders-table"
  heimdall-content-swap="inner">
  <input
    name="Search"
    type="search"
    autocomplete="off"
    heimdall-content-input="orders.filter"
    heimdall-payload-from="closest-form"
    heimdall-debounce="300"
    heimdall-content-target="#orders-table"
    heimdall-content-swap="inner" />

  <button type="submit">Apply</button>
</form>

<div id="orders-table"></div>
```

## Attribute Groups

Triggers:

```text
heimdall-content-click
heimdall-content-submit
heimdall-content-input
heimdall-content-change
heimdall-content-load
heimdall-content-visible
heimdall-content-scroll
```

Payload:

```text
heimdall-payload-from="closest-form"
heimdall-payload-from="closest-state"
```

Target and swap:

```text
heimdall-content-target="#orders-table"
heimdall-content-swap="inner"
```

Modifiers:

```text
heimdall-debounce="300"
heimdall-sync="replace"
heimdall-sync-group="search"
```

Synchronization values are `parallel`, `replace`, `drop`, and `queue-latest`. Parallel is the default. A group coordinates requests from separate elements; without one, coordination is scoped to the triggering element.

## Browser-Native Commands

The native `command` and `commandfor` attributes work without the Heimdall client runtime:

```html
<button commandfor="confirmation-dialog" command="show-modal">
  Open confirmation
</button>
```

Typed C# helpers are available on element and fragment builders:

```csharp
button.CommandFor("confirmation-dialog")
    .Command(Html.CommandType.show_modal);
```

Typed values are `toggle_popover`, `show_popover`, `hide_popover`, `close`, `request_close`, and `show_modal`. Use the string overload for a custom command such as `--archive-record`. `CommandFor` takes an element ID without a leading `#`.

## Implementation Checklist

When an interaction does not work, walk the markup in this order:

1. Is the runtime script loaded?
   `/_content/HeimdallFramework.Web/heimdall-bundle.min.js`
2. Is there exactly one trigger attribute with an action ID?
   `heimdall-content-click="orders.refresh"`
3. Does the action ID match the resolved content invocation ID?
   `[ContentInvocationPrefix("orders")]` plus `[ContentInvocation("refresh")]` resolves to `orders.refresh`.
4. Does the payload source point to real data?
   Examples include `closest-form`, `closest-state`, `self`, selector, or `ref:path`.
5. Does the target selector match an element?
   `heimdall-content-target="#orders"`
6. Is the swap mode appropriate?
   Examples include `inner`, `outer`, `beforeend`, `afterbegin`, or `none`.
7. Did `heimdall-sync` intentionally replace, drop, or queue the request?
8. Is the element disabled?
   `disabled` or `aria-disabled="true"` prevents invocation.

## Guidance

- Prefer fluent helpers in FluentHtml code.
- Use raw attributes naturally in Razor views and partials.
- Debug rendered DOM, not only source code.
- Keep one clear trigger per interaction element.
- Keep payload source, target, and swap visible near the trigger.
- Keep synchronization visible when non-parallel behavior is required.
- Do not invent attribute names.
