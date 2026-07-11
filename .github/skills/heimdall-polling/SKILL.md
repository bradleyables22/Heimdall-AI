---
name: heimdall-polling
description: Use when implementing Heimdall polling interactions, including poll intervals, refresh fragments, dashboard updates, progress checks, when to prefer Bifrost SSE, and avoiding overly aggressive polling.
---

# Heimdall Polling

Use this skill when a Heimdall region should periodically ask the server for fresh HTML.

Official docs: https://heimdall-framework.org/polling

Polling is a trigger modifier pattern. The browser invokes a content action at an interval, receives HTML, and applies the configured swap.

## Basic Pattern

```csharp
section.Heimdall()
    .Load("metrics.refresh")
    .PayloadEmptyObject()
    .PollMs(5000)
    .Target("#metrics-panel")
    .SwapOuter();
```

Action:

```csharp
[ContentInvocation("metrics.refresh")]
public static async Task<IHtmlContent> RefreshMetrics(
    IMetricsService metrics,
    CancellationToken ct)
{
    var snapshot = await metrics.GetSnapshotAsync(ct);
    return MetricsPanel.Render(snapshot);
}
```

## When To Use Polling

Use polling for:

- low-frequency dashboard refresh
- progress checks
- background job status
- cache-friendly periodic updates
- simple infrastructure where SSE is unnecessary

Prefer Bifrost SSE for:

- server-initiated events
- high-frequency or unpredictable updates
- notification streams
- live feeds
- user-specific push updates

## Guidance

- Keep intervals conservative.
- Return the smallest useful HTML fragment.
- Use stable target selectors.
- Stop polling when the UI no longer needs updates if the runtime supports that pattern.
- Avoid polling as a substitute for event-driven SSE when the server knows exactly when updates occur.
- Do not use polling to maintain a full client-side state machine.
