using Heimdall.Server.Rendering;
using Microsoft.AspNetCore.Html;

public static class HomePage
{
    public static IHtmlContent Render()
        => FluentHtml.Main(main =>
        {
            main.Id("home")
                .H1(h => h.Text("Hello Heimdall"))
                .P(p => p.Text("HTML-first server-driven UI."));
        });
}
