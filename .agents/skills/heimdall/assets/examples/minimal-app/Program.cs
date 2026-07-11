using Heimdall.Server;
using Heimdall.Server.Rendering;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAntiforgery();
builder.Services.AddHeimdall(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
});

var app = builder.Build();

app.UseAntiforgery();

app.MapStaticAssets();
app.UseStaticFiles();

app.UseHeimdall();

app.MapHeimdallPage("/", () =>
    MainLayout.Render(HomePage.Render(), "Home"));

app.Run();
