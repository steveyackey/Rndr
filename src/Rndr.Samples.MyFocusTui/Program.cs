using Rndr;
using Rndr.Extensions;
using Rndr.Samples.MyFocusTui.Pages;

// Create the app builder
var builder = TuiApplication.CreateBuilder(args);

// Configure theme (optional)
builder.Services.AddRndrTheme(theme =>
{
    theme.AccentColor = ConsoleColor.Cyan;
    theme.Panel.BorderStyle = BorderStyle.Rounded;
});

// Build the app
var app = builder.Build();

// Map views
app.MapView("/", typeof(HomeComponent));
app.MapView("/log", typeof(LogComponent));

// Register global key handlers
app.OnGlobalKey((key, ctx) =>
{
    switch (key.KeyChar)
    {
        case 'q' or 'Q':
            ctx.Application.Quit();
            return true;

        case 'h' or 'H':
            ctx.Navigation.Navigate("/");
            return true;

        case 'l' or 'L':
            ctx.Navigation.Navigate("/log");
            return true;
    }

    return false;
});

// Run the app
await app.RunAsync();
