using Microsoft.Extensions.DependencyInjection;
using Rndr;
using Rndr.Extensions;
using MyFocusTui.Models;
using MyFocusTui.Pages;

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

// Map views - use generated .tui components with generic syntax for better type safety
app.MapView<Home>("/");
app.MapView<Log>("/log");

// Register global key handlers
app.OnGlobalKey((key, ctx) =>
{
    // Get global state using the new helper method
    var focusState = ctx.StateGlobal("focus", new FocusState());

    // Handle Enter while editing to save immediately
    if (focusState.Value.IsEditing && key.Key == ConsoleKey.Enter)
    {
        var edited = focusState.Value.EditingText?.Trim();
        if (!string.IsNullOrWhiteSpace(edited))
        {
            var oldTodo = focusState.Value.CurrentTodo;
            focusState.Value.CurrentTodo = edited;
            focusState.Value.Log.Add(new ActionEntry
            {
                Timestamp = DateTime.Now,
                Message = $"Edited: {oldTodo} → {focusState.Value.CurrentTodo}"
            });
        }
        focusState.Value.IsEditing = false;
        focusState.Value.EditingText = string.Empty;
        return true;
    }

    // Handle Escape to close edit modal
    if (key.Key == ConsoleKey.Escape && focusState.Value.IsEditing)
    {
        focusState.Value.IsEditing = false;
        focusState.Value.EditingText = string.Empty;
        return true;
    }

    switch (key.KeyChar)
    {
        case 'q' or 'Q':
            // Don't quit if editing
            if (focusState.Value.IsEditing) return false;
            ctx.Application.Quit();
            return true;

        case 'h' or 'H':
            // Don't navigate if editing
            if (focusState.Value.IsEditing) return false;
            ctx.Navigation.NavigateHome();
            return true;

        case 'l' or 'L':
            // Don't navigate if editing
            if (focusState.Value.IsEditing) return false;
            ctx.Navigation.Navigate("/log");
            return true;

        case 'e' or 'E':
            // Open edit modal anytime
            if (!focusState.Value.IsEditing)
            {
                focusState.Value.EditingText = focusState.Value.CurrentTodo ?? string.Empty;
                focusState.Value.IsEditing = true;
                return true;
            }
            return false;
    }

    return false;
});

// Run the app
await app.RunAsync();
