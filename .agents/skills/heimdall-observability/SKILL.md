---
name: heimdall-observability
description: Use when instrumenting or monitoring Heimdall server behavior, including HeimdallDiagnostics, OpenTelemetry ActivitySource and Meter registration, content-action traces and metrics, Bifrost connection and delivery metrics, tags, outcomes, privacy, and cardinality.
---

# Heimdall Observability

Use Heimdall's built-in diagnostics to trace content actions and monitor Bifrost without modifying framework internals.

Official docs: https://heimdall-framework.org/configuration

The Server package emits dependency-free `ActivitySource` traces and `System.Diagnostics.Metrics` instruments. An application chooses whether and where to export them.

## OpenTelemetry Registration

Register the public source and meter constants with the application's OpenTelemetry providers:

```csharp
using Heimdall.Server;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddSource(HeimdallDiagnostics.ActivitySourceName))
    .WithMetrics(metrics => metrics
        .AddMeter(HeimdallDiagnostics.MeterName));
```

Add exporters and sampling through normal OpenTelemetry configuration. Heimdall does not require OpenTelemetry packages merely to emit diagnostics.

## Activities

The public activity names are available as constants:

- `heimdall.content_action`
- `heimdall.bifrost.connection`
- `heimdall.bifrost.publish`

Use the constants on `HeimdallDiagnostics` instead of repeating string literals when configuring filters, processors, or tests.

Content-action activities record the resolved action ID, response status, outcome, exception type, or cancellation reason as appropriate. Bifrost activities describe connection and publish outcomes.

## Metrics

Content-action instruments cover:

- request count
- request duration
- request body size
- rendered response body size
- exception count
- cancellation count

Bifrost instruments cover:

- active SSE connections
- active topic subscribers
- published messages
- delivered messages
- expired messages
- dropped messages

Metric and tag names are exposed as `HeimdallDiagnostics` constants. Prefer those constants in dashboards, tests, and custom instrumentation adapters.

## Tags And Privacy

Common dimensions include:

- action ID
- outcome
- HTTP response status
- error type
- cancellation reason
- Bifrost event name
- Bifrost drop reason

Heimdall intentionally excludes action payloads, user identities, and Bifrost topic names. Do not add those values back as high-cardinality or sensitive telemetry dimensions.

Action IDs and named Bifrost event names should come from bounded application-defined sets. Do not generate action or event names from user input.

## Interpreting Outcomes

- Treat normal client cancellation separately from exceptions.
- Distinguish timeouts from replacement or external cancellation.
- A Bifrost `no_subscribers` publish outcome can be normal, especially for best-effort live UI updates.
- Expired and dropped message metrics help reveal slow subscribers, undersized buffers, or work published after its useful lifetime.
- Active connection and subscriber counts describe the current application instance unless the telemetry backend aggregates instances.

`Bifrost.HasSubscribers(topic)` is an instantaneous local optimization hint. It complements metrics but is not an observability query or a delivery guarantee.

## Guidance

- Register the source and meter once in application startup.
- Add exporters, retention, alerts, and sampling through the application's observability stack.
- Build alerts from stable outcomes and rates, not isolated expected cancellations.
- Avoid payloads, users, topic names, selectors, or URLs as metric dimensions.
- Preserve trace context through normal ASP.NET Core and OpenTelemetry instrumentation.
- Test important instrumentation using the public constants instead of private implementation details.
