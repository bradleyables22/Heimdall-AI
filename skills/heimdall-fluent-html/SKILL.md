---
name: heimdall-fluent-html
description: Use when generating or editing Heimdall server-rendered markup with FluentHtml or Html, including IHtmlContent render functions, element builders, attributes, forms, tables, text encoding, Raw usage, and Heimdall behavior extension points.
---

# Heimdall FluentHtml

Use this skill when generating Heimdall markup.

Official docs: https://heimdall-framework.org/html-attributes

`FluentHtml` is the preferred authoring API for server-rendered Heimdall UI. It produces `IHtmlContent`, encodes text by default when `.Text(...)` is used, and composes naturally with Heimdall behavior helpers.

## Imports

```csharp
using Heimdall.Server.Rendering;
using Microsoft.AspNetCore.Html;
```

## Core Pattern

```csharp
public static IHtmlContent Render()
    => FluentHtml.Section(section =>
    {
        section.Id("profile")
            .Class("panel")
            .H2(h => h.Text("Profile"))
            .P(p => p.Text("Server-rendered HTML."));
    });
```

Most element helpers accept a builder callback:

```csharp
FluentHtml.Div(div =>
{
    div.Id("summary");
    div.Text("Hello");
});
```

Nested elements are available as builder methods:

```csharp
FluentHtml.Main(main =>
{
    main.Section(section =>
    {
        section.H1(h => h.Text("Orders"));
        section.Div(panel => panel.Id("orders-list"));
    });
});
```

## Common Builder Methods

Use these methods instead of string concatenation:

```csharp
b.Id("orders")
b.Class("container", "py-4")
b.Attr("data-kind", "order")
b.Data("order-id", order.Id.ToString())
b.Aria("label", "Orders")
b.Text(order.Name)
b.Content(OrderSummary.Render(order))
b.Raw("<!DOCTYPE html>")
```

Boolean attributes:

```csharp
input.Required();
input.Disabled(isBusy);
input.Checked(isSelected);
```

Form attributes:

```csharp
input.Name("email");
input.Value(model.Email);
input.Placeholder("you@example.com");
label.For("email");
```

## Text, Content, And Raw

Use `.Text(...)` for strings:

```csharp
p.Text(user.DisplayName);
```

Use `.Content(...)` for prebuilt `IHtmlContent`:

```csharp
main.Content(UserCard.Render(user));
```

Use `.Raw(...)` only for trusted markup:

```csharp
fragment.Raw("<!DOCTYPE html>");
```

Do not pass untrusted user content to `Raw`.

## Forms

Use `FluentHtml.Form` plus Heimdall behavior helpers:

```csharp
FluentHtml.Form(form =>
{
    form.Id("todo-form");
    form.Heimdall()
        .Submit("todos.add")
        .PayloadFromClosestForm()
        .Target("#todo-panel")
        .SwapOuter()
        .PreventDefault()
        .Disable();

    form.Label(label =>
    {
        label.For("title");
        label.Text("Title");
    });

    form.Input(Html.InputType.text, input =>
    {
        input.Id("title")
            .Name("title")
            .Required()
            .Placeholder("New todo");
    });

    form.Button(button =>
    {
        button.Type("submit");
        button.Text("Add");
    });
});
```

Known input types use `Html.InputType`, including `text`, `email`, `password`, `number`, `date`, `search`, `checkbox`, `radio`, `hidden`, and `datetime_local`.

## Lists And Tables

Use normal C# loops inside the builder callback:

```csharp
FluentHtml.Ul(ul =>
{
    foreach (var item in items)
    {
        ul.Li(li => li.Text(item.Title));
    }
});
```

```csharp
FluentHtml.Table(table =>
{
    table.TableHead(head =>
    {
        head.TableRow(row =>
        {
            row.TableHeaderCell(th => th.Text("Order"));
            row.TableHeaderCell(th => th.Text("Status"));
        });
    });

    table.TableBody(body =>
    {
        foreach (var order in orders)
        {
            body.TableRow(row =>
            {
                row.TableDataCell(cell => cell.Text(order.Number));
                row.TableDataCell(cell => cell.Text(order.Status));
            });
        }
    });
});
```

## Lower-Level Html Helpers

Use `Html` for compact literal composition or response directives:

```csharp
Html.Tag("main",
    Html.Attr("id", "home"),
    Html.Tag("h1", "Home"));
```

Prefer `FluentHtml` for larger markup.

## Heimdall Behavior Extension

Call `.Heimdall()` on an element builder to add Heimdall behavior attributes:

```csharp
button.Heimdall()
    .Click("orders.refresh")
    .PayloadEmptyObject()
    .Target("#orders-list")
    .SwapOuter();
```

Call `.Heimdall()` on a fragment builder to add response directives:

```csharp
FluentHtml.Fragment(fragment =>
{
    fragment.Add(MainResult.Render());
    fragment.Heimdall().Invocation("#sidebar", payload: Sidebar.Render());
});
```
