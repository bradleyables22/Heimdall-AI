---
name: heimdall-lazy-loading
description: Use when lazy-loading Heimdall fragments or deferred UI, including load and visible triggers, placeholders, skeletons, deferred panels, infinite scroll sentinels, and returning targeted HTML once content is needed.
---

# Heimdall Lazy Loading

Use this skill when UI should be rendered later instead of with the initial page.

Official docs: https://heimdall-framework.org/lazy-loading

Lazy loading in Heimdall is still server-rendered. The initial page renders a placeholder or sentinel, a trigger invokes a content action, and the action returns HTML for the target.

## Load Trigger

Use a load trigger for content that should load after the page boots:

```csharp
FluentHtml.Section(section =>
{
    section.Id("recommendations");
    section.Text("Loading...");
    section.Heimdall()
        .Load("recommendations.load")
        .PayloadEmptyObject()
        .Target("#recommendations")
        .SwapOuter();
});
```

## Visible Trigger

Use visible for content that should load when scrolled into view:

```csharp
sentinel.Heimdall()
    .Visible("feed.more")
    .VisibleOnce()
    .Payload(new { Cursor = nextCursor })
    .Target("#feed")
    .SwapBeforeEnd();
```

## Action

```csharp
public static partial class RecommendationPanel
{
    public const string HostId = "recommendations";

    [ContentInvocationPrefix("recommendations")]
    public sealed class RecommendationPanelActions(IRecommendationService recommendations)
    {
        [ContentInvocation("load")]
        public async Task<IHtmlContent> Load(CancellationToken ct)
        {
            var items = await recommendations.GetAsync(ct);
            return RecommendationPanel.Render(items);
        }
    }
}
```

## Guidance

- Render a meaningful placeholder or skeleton.
- Use `Load` for immediate deferred regions.
- Use `Visible` for below-the-fold or infinite-scroll regions.
- Use `VisibleOnce()` when a region should not repeatedly load.
- Return HTML fragments, not JSON.
- Keep lazy-loaded targets stable.
- Keep lazy-load actions beside the component or sentinel they replace or append to.
- Do not lazy-load critical content needed for first meaningful render unless there is a good reason.
