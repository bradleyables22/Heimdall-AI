---
name: heimdall-bootstrap
description: Use when styling Heimdall UI with HeimdallFramework.Bootstrap typed helpers, including Bootstrap class aliases for layout, spacing, forms, tables, buttons, cards, flex, breakpoints, and Raw class escape hatches.
---

# Heimdall Bootstrap

Use this skill when styling Heimdall UI with `HeimdallFramework.Bootstrap`.

Official docs: https://heimdall-framework.org/bootstrap

The Bootstrap package provides strongly typed class helpers. It is not a component framework. It maps to real Bootstrap classes while keeping markup structure under your control.

## Imports

```csharp
using Bs = Heimdall.Bootstrap.Bootstrap;
```

## Core Pattern

```csharp
public static partial class DashboardPanel
{
    public static class Css
    {
        public const string Root = "dashboard-panel";
    }

    public static IHtmlContent Render()
        => FluentHtml.Div(div =>
        {
            div.Class(
                Css.Root,
                Bs.Layout.Container,
                Bs.Spacing.Py(4));

            div.H1(h =>
            {
                h.Class(Bs.Text.Lead);
                h.Text("Dashboard");
            });
        });
}
```

Many examples use the alias `Bs`:

```csharp
using Bs = Heimdall.Bootstrap.Bootstrap;
```

## Buttons

```csharp
button.Class(
    Bs.Btn.Base,
    Bs.Btn.Primary);
```

Outline button:

```csharp
button.Class(Bs.Btn.OutlinePrimary);
```

## Layout And Spacing

```csharp
div.Class(
    Bs.Layout.Row,
    Bs.Spacing.Mb(3),
    Bs.Spacing.Mt(3));
```

Columns:

```csharp
col.Class(Bs.Layout.ColSpan(6, Bs.Breakpoint.Md));
```

Flex:

```csharp
toolbar.Class(
    Bs.Display.Flex,
    Bs.Spacing.Gap(2),
    Bs.Flex.AlignItemsCenter,
    Bs.Flex.JustifyBetween);
```

## Forms

```csharp
input.Class(Bs.Form.Control);
label.Class(Bs.Form.Label);
select.Class(Bs.Form.Select);
```

## Tables

```csharp
table.Class(
    Bs.Table.Base,
    Bs.Table.Hover,
    Bs.Table.Borderless);
```

## Cards

Bootstrap card classes can be used, but keep the markup explicit:

```csharp
FluentHtml.Div(card =>
{
    card.Class(Bs.Card.Base, Bs.Spacing.Mb(3));

    card.Div(header =>
    {
        header.Class(Bs.Card.Header);
        header.Text("Live Data");
    });

    card.Div(body =>
    {
        body.Class(Bs.Card.Body);
        body.Heimdall().Sse("metrics", "#metrics-body", HeimdallHtml.Swap.Inner);
    });
});
```

## Raw Escape Hatch

When a Bootstrap class is not covered, use `Bs.Raw(...)`. For app-owned CSS, prefer component constants instead of repeated raw strings:

```csharp
div.Class(Bs.Raw("display-6"), AppShell.Css.Root);
```

Prefer typed helpers when available. Prefer `Component.Css.*` for project classes and `Bs.*` for Bootstrap classes.
