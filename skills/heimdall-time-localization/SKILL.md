---
name: heimdall-time-localization
description: Use when rendering absolute server times in a Heimdall user's browser-local timezone, including LocalizeTime, standard and custom C#-style formats, locale resolution, initial/action/OOB/SSE content, dynamic DOM updates, and time lifecycle events.
---

# Heimdall Time Localization

Use browser-local time when the server owns the instant but the user should see that instant in the browser's local timezone.

Official docs: https://heimdall-framework.org/fluent-html

This feature is client-side localization only. It makes no formatting request to the server and does not choose an arbitrary named timezone.

## Fluent HTML

`LocalizeTime(...)` works on any fluent HTML element; it does not dictate the tag:

```csharp
var createdAt = new DateTimeOffset(
    2026, 8, 26, 18, 30, 5, TimeSpan.Zero);

return FluentHtml.Span(span => span
    .Class("order-created")
    .LocalizeTime(createdAt, "MMM d, yyyy 'at' h:mm tt"));
```

It accepts:

- `DateTimeOffset`
- `DateTime` whose `Kind` is `Utc` or `Local`

It rejects `DateTimeKind.Unspecified` because that value does not identify an absolute instant.

The server emits an invariant timestamp with an explicit offset, the selected format, and encoded fallback text formatted with `CultureInfo.CurrentCulture`. If JavaScript is unavailable or formatting fails, the fallback remains visible.

## Formats

Supported standard formats are:

- `d`: short date
- `D`: long date
- `t`: short time
- `T`: medium time
- `g`: short date and short time
- `G`: short date and medium time; this is the default

Supported custom tokens are the normal repeated forms of:

```text
d M y h H m s t f z
```

Custom formats support quoted literals, escaped characters, and `%` for a single custom token. Fractional seconds support up to milliseconds. This is a documented C#-style subset, not complete `DateTime.ToString` parity.

## Locale And Timezone

The runtime uses the browser's current IANA timezone. Locale resolution prefers:

1. The closest `lang` attribute on the localized element.
2. A relevant surrounding response context.
3. The document language.
4. The browser language.

Use `TimeZoneInfo.ConvertTime(...)` and ordinary `.Text(...)` when the application must render a predetermined timezone. Do not use `LocalizeTime` for that case.

## Where Localization Runs

The browser runtime processes local-time elements:

- during initial startup
- in the main result of a content action
- inside out-of-band invocation content
- inside Bifrost SSE content
- when matching elements are added dynamically
- when `heimdall-time`, `heimdall-time-format`, or an inherited `lang` value changes

Action, invocation, and SSE fragments are localized before insertion so users do not see an avoidable UTC-to-local text flash.

## Raw HTML Shape

```html
<time
  heimdall-time="2026-08-26T18:30:05.000Z"
  heimdall-time-format="MMM d, yyyy 'at' h:mm tt">
  Aug 26, 2026 at 6:30 PM
</time>
```

`heimdall-time` must contain an ISO timestamp with `Z` or an explicit offset. Prefer `.LocalizeTime(...)` in C# so validation, encoding, and fallback rendering remain consistent.

## Lifecycle Events

```javascript
document.addEventListener("heimdall:time-before", event => {
  // Mutable: value, format, timeZone, locale, and text.
  // preventDefault() keeps the existing fallback text.
});

document.addEventListener("heimdall:time-after", event => {
  console.log(event.detail.text);
});

document.addEventListener("heimdall:time-error", event => {
  console.error(event.detail.error);
});
```

Assigning `event.detail.text` in `time-before` supplies the final text directly. Output is always assigned with `textContent`, not interpreted as HTML.

The runtime tracks processed elements by DOM identity with a `WeakMap`. Duplicate timestamp values on the same page are independent because each element is a separate key. A changed value, format, timezone, or locale produces a new processing signature.

## Guidance

- Store and transport absolute instants, preferably with `DateTimeOffset` or UTC `DateTime`.
- Use `LocalizeTime` only when browser-local display is the intent.
- Put `lang` on the nearest meaningful language boundary.
- Keep the server fallback useful and accessible.
- Use lifecycle hooks for narrow customization, not a second rendering system.
- Test daylight-saving boundaries and the locales/formats the application actually uses.
