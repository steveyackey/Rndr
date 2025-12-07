using Microsoft.Extensions.DependencyInjection;
using Rndr;
using Rndr.Extensions;
using UsingSpectre.Pages;

// Create the app builder
var builder = TuiApplication.CreateBuilder(args);

// Configure theme (optional)
builder.Services.AddRndrTheme(theme =>
{
    theme.AccentColor = ConsoleColor.Green;
    theme.Panel.BorderStyle = BorderStyle.Rounded;
});

// Build the app
var app = builder.Build();

// Map views - use generated .tui components
app.MapView("/", typeof(Home));

// Register global key handlers
app.OnGlobalKey((key, ctx) =>
{
    if (key.KeyChar is 'q' or 'Q')
    {
        ctx.Application.Quit();
        return true;
    }
    return false;
});

// Run the app
await app.RunAsync();
