---
name: heimdall-mutations
description: Use when changing existing Heimdall DOM nodes in place from action or Bifrost responses, including Mutate, MutateAll, attribute, class, and state operations, self/subtree/selector scopes, directive ordering, lifecycle events, and preserving element identity.
---

# Heimdall Mutations

Use mutations when a response should change attributes, classes, or Heimdall state without replacing the existing DOM node.

Official docs: https://heimdall-framework.org/response-directives

Mutations are out-of-band response directives. They work in content-action and Bifrost SSE responses and participate in normal Heimdall response ordering.

## Fluent Response Pattern

```csharp
return FluentHtml.Fragment(fragment =>
{
    fragment.Add(OrderPanel.Render(order));

    fragment.Heimdall().Mutate("#order-status", mutation => mutation
        .Attr("aria-live", "polite")
        .RemoveAttr("aria-busy")
        .RemoveClass("loading")
        .AddClass("ready")
        .State(new { order.Id, order.Status }));
});
```

`Mutate(...)` resolves the first root matching the target selector. Use `MutateAll(...)` when every matching root should be processed:

```csharp
fragment.Heimdall().MutateAll(".order-card", mutation => mutation
    .AddClass("stale")
    .Attr("data-refresh-required", "true"));
```

## Operations

`MutationBuilder` supports ordered operations:

```csharp
mutation
    .Attr("data-status", "complete")
    .Set(Html.Attr("aria-busy", "false"))
    .RemoveAttr("disabled")
    .AddClass("complete", "highlight")
    .RemoveClass("pending")
    .State(new { Count = 3 })
    .State("row", new { Selected = true })
    .StateJson("{\"count\":3}")
    .RemoveState()
    .RemoveState("row");
```

An empty attribute value is an explicit value. An omitted value in a raw `<mutation-attr>` removes the attribute. Removing an absent attribute or class and adding an existing class are safe no-ops.

## Scopes

The default scope is `Self`:

```csharp
fragment.Heimdall().Mutate(
    "#order-panel",
    mutation => mutation.AddClass("updated"),
    HeimdallHtml.MutationScope.Self);
```

Available scopes:

- `Self`: mutate the resolved root only.
- `Subtree`: mutate the root and every descendant.
- `Matching(".price")`: mutate matching descendants inside the resolved root.

`MutateAll` controls how many roots a target selector resolves. `MutationScope` controls which elements inside each resolved root receive the operations.

## Raw Directive Shape

```html
<mutation heimdall-content-target="#order-panel" scope="self">
  <mutation-attr name="aria-busy"></mutation-attr>
  <mutation-attr name="data-status" value="complete"></mutation-attr>
  <mutation-class remove="loading"></mutation-class>
  <mutation-class add="ready selected"></mutation-class>
</mutation>
```

Use `scope="subtree"`, or `scope="select" selector=".price"`, for descendant operations. Add the boolean `all` attribute when every matching root should be processed.

## Response Ordering

Invocations and mutations are processed in the order they occur in the response. This allows an invocation to create or replace a node and a following mutation to update that new node.

- Mutations run before the normal main-target swap.
- An `<abort>` suppresses the main swap but still allows response directives to run.
- A `<redirect>` wins and prevents mutation and swap effects for that response.
- Failed responses and responses processed with out-of-band handling disabled do not apply mutations.

Keep dependent directives in their required order instead of relying on concurrent DOM changes.

## Lifecycle Events

```javascript
document.addEventListener("heimdall:mutation-before", event => {
  // Cancellable. Inspect the directive, targets, operations, and origin.
});

document.addEventListener("heimdall:mutation-after", event => {
  console.log(event.detail.targetCount, event.detail.operationCount);
});

document.addEventListener("heimdall:mutation-error", event => {
  console.error(event.detail.code, event.detail.message);
});
```

Mutation events originate from the action source when available and bubble. The origin identifies `action` or `sse`.

## Element Identity And Runtime Behavior

Mutations preserve the existing node, which normally preserves focus, input state, references, and directly attached listeners. Mutating a Heimdall behavior attribute is supported: the runtime reconciles added, changed, and removed trigger behavior after the mutation.

Response mutations are authoritative over temporary busy-state attributes. Do not use mutation directives as a general client renderer; return replacement HTML when the content or structure itself changed.

## Guidance

- Use a mutation for attributes, classes, accessibility state, and small DOM-adjacent state changes.
- Use a normal swap when markup structure or text content changes substantially.
- Prefer typed builder operations over hand-written directive tags in C#.
- Use `MutateAll` and descendant scopes deliberately; broad selectors can touch many nodes.
- Keep response order visible when one directive depends on another.
- Treat mutations of Heimdall's own attributes as intentional runtime behavior changes.
