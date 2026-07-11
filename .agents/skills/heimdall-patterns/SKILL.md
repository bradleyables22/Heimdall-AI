---
name: heimdall-patterns
description: Use when choosing Heimdall architecture and interaction patterns, including when to use pages vs content actions, forms, validation loops, lazy loading, polling vs SSE, out-of-band updates, MVC hybrid adoption, and avoiding SPA or JSON-first designs.
---

# Heimdall Patterns

Use this skill when choosing a Heimdall design pattern.

Official docs: https://heimdall-framework.org/patterns

Heimdall's core pattern is:

```text
event -> server action -> HTML -> targeted DOM update
```

## Pages vs Content Actions

Pages are route-level rendering:

- mapped with `MapHeimdallPage(...)`
- handle URLs such as `/`, `/orders`, or `/admin`
- return full HTML for a browser request
- usually wrapped in a layout
- participate in the normal ASP.NET Core endpoint pipeline

Content actions are interaction-level rendering:

- marked with `[ContentInvocation]`
- triggered by Heimdall attributes
- return fragment HTML
- typically swapped into an existing target
- serve interaction flows instead of route-level page loads

## Form Validation

Use a form boundary, closest-form payload, a content action, and returned HTML.

For simple submits, use `Submit`, `PayloadFromClosestForm`, and either `SwapOuter` or `SwapNone`.

For interactive validation, use `Input` or `Change` with debounce and re-render the form host.

## Multi-Target Updates

Use response directives when one action needs to update several regions:

```csharp
return FluentHtml.Fragment(fragment =>
{
    fragment.Add(MainPanel.Render(model));
    fragment.Heimdall().Invocation("#sidebar", payload: Sidebar.Render(model));
});
```

## Lazy Loading

Use `Load` for immediate deferred regions and `Visible` for below-the-fold or infinite-load regions.

## Polling vs SSE

Use polling for simple low-frequency refresh where asking periodically is acceptable.

Use Bifrost SSE when the server knows when updates occur or when updates are user-specific, live, or event-driven.

## MVC Hybrid Adoption

In MVC-heavy apps:

- keep Razor views and partials where they are working
- use Heimdall attributes in rendered markup
- use `IHeimdallMvcRenderer` to return partial HTML from content actions
- place actions beside controllers when that improves discoverability

## Guidance

- Start with one page or interaction.
- Keep server rendering as the source of truth.
- Return HTML fragments for UI changes.
- Use stable target IDs.
- Use response directives for coordinated updates.
- Use JavaScript only for explicit browser effects.
- Avoid JSON-first UI interactions unless there is a non-Heimdall API consumer.
- Avoid SPA shells unless explicitly required.
