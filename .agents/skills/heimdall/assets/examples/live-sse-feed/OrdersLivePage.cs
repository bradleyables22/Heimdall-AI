using Heimdall.Server;
using Heimdall.Server.Rendering;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

public sealed record OrderEvent(string Number, string Status);

public static class OrdersLivePage
{
    public static IHtmlContent Render()
        => FluentHtml.Main(main =>
        {
            main.Id("orders-live")
                .H1(h => h.Text("Live Orders"))
                .Div(feed =>
                {
                    feed.Id("orders-feed");
                    feed.Heimdall()
                        .SseTopic("orders")
                        .SseTarget("#orders-feed")
                        .SseSwap(HeimdallHtml.Swap.BeforeEnd);
                });
        });
}

public static class OrderToast
{
    public static IHtmlContent Render(OrderEvent order)
        => FluentHtml.Div(toast =>
        {
            toast.Class("order-toast");
            toast.Strong(strong => strong.Text(order.Number));
            toast.Span(span => span.Text($" {order.Status}"));
        });
}

[ContentInvocationPrefix("orders")]
public sealed class OrderActions
{
    [ContentInvocation("publish")]
    public static async Task<IHtmlContent> Publish([FromServices] Bifrost bifrost, CancellationToken ct)
    {
        await bifrost.PublishAsync(
            topic: "orders",
            content: OrderToast.Render(new OrderEvent("A-1007", "updated")),
            ttl: TimeSpan.FromSeconds(10),
            ct: ct);

        return HtmlString.Empty;
    }
}
