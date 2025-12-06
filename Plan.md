You are an expert .NET library & tooling engineer.
Your job is to design and implement the Rndr framework described below.

Treat everything here as the spec + product brief + design doc.
Your output should be production-quality, idiomatic .NET, and easy to maintain.

⸻

0. High-level vision

Rndr is a TUI (text user interface) framework for .NET with these core goals:
	1.	Feels like ASP.NET Minimal APIs at the top level:
	•	TuiApplication.CreateBuilder(args), Build(), MapView("/", ...), RunAsync().
	2.	Feels like Vue single-file components inside views:
	•	.tui files with <Column>, <Row>, <Panel>, <Text>, <Button> etc.
	•	@code { ... } blocks with simple reactive state: var count = State("count", 0);.
	3.	Beautiful by default, more visually polished than Ratatui/Textual/Ink/BubbleTea:
	•	Semantic layout primitives, clean panels, consistent spacing, tasteful default theme.
	•	Theming via design tokens (accent color, borders, spacing, etc.) – no ugly ANSI soup.
	4.	AOT-friendly and trimming-safe by design:
	•	Avoid reflection-heavy magic and runtime discovery in core.
	•	Views ultimately compile to plain C# classes via Razor.
	5.	Beginner-friendly → scales to large apps:
	•	Simple to start; grows into components, navigation, shared state, themes.

⸻

1. Technical context & constraints
	•	Target runtime:
Implement against .NET 8 or later, but design for .NET 10 future-proofing.
	•	Language: C# 12+ where possible (records, primary constructors, etc.).
	•	Project structure (solution-level):
	•	Rndr
Core runtime, layout, rendering, navigation, state, theming, diagnostics.
	•	Rndr.Razor
Razor/.tui integration, code generation.
	•	Rndr.Samples.MyFocusTui
Sample app described in Section 6.
	•	Console environment:
	•	Start with ANSI/VT-friendly terminals.
	•	MVP can assume a “modern” terminal (UTF-8, box-drawing characters OK).

⸻

2. MVP vs Phases

Implement in phases, building from a minimal usable core to full experience.

Phase 1 (MVP): Core runtime, C# views, basic layout + render

Scope:
	1.	TuiApplication, TuiAppBuilder, TuiApp.
	2.	MapView(string route, Action<ViewDefinition> configure) + a simple ViewDefinition.
	3.	ViewContext, Signal<T> state, NavigationContext skeleton.
	4.	Layout primitives via builders (C#-only, no .tui yet):
	•	Column, Row, Panel, Text, Button, Spacer(), Centered(...).
	5.	Basic renderer:
	•	Converts layout tree to a 2D buffer, writes to console with minimal flicker.
	•	No need for pixel-perfect layout; just consistent and readable.
	6.	Input loop:
	•	Reads key presses, dispatches to focused buttons or view key handlers.
	7.	Map a C#-only sample view (Counter) and show it works.

Phase 2: Navigation, shared state, global key handling, theming

Scope:
	1.	Routing:
	•	Multiple MapView calls (e.g. /, /log, /about).
	•	Navigation.Navigate("/path"), Navigation.Back(), Navigation.Replace("/path").
	2.	Shared app-level state:
	•	State("key", initial) is route-scoped.
	•	StateGlobal("key", initial) (or similar) is app-scoped.
	3.	Global key handling:
	•	app.OnGlobalKey(Func<KeyEvent, GlobalContext, bool> handler).
	4.	Theming:
	•	Theme object with accent color, border style, spacing unit.
	•	A default theme; ability to register custom theme via DI.
	5.	Sample: “Most Important Todo + Log” app in pure C# builder style.

Phase 3: .tui Razor integration (Vue-like single-file components)

Scope:
	1.	.tui file format:
	•	Top-level directive: @view.
	•	Optional @using, @inject, @code { ... }.
	•	Markup tags: <Column>, <Row>, <Panel>, <Text>, <Button>, <TextInput>, <Spacer>, <Centered>.
	2.	Razor/Generator:
	•	Rndr.Razor project:
	•	MSBuild item type <TuiComponent Include="Pages\**\*.tui" />.
	•	Razor-based generator that turns Home.tui into a partial class inheriting TuiComponentBase.
	3.	.tui to C# mapping:
	•	@view → partial class Home : TuiComponentBase.
	•	State("key", initial) inside @code uses Rndr’s state.
	•	Tags call layout APIs in generated Build(LayoutBuilder layout).
	4.	Sample: Rewrite the “Most Important Todo + Log” app using .tui files.

Phase 4 (stretch): Tooling & polish

Scope (lower priority):
	1.	VS Code extension:
	•	File association for *.tui.
	•	Basic syntax highlighting (Razor-ish).
	2.	Optional Rider plugin scaffolding:
	•	Register .tui as Razor-like.
	3.	Hot reload / dotnet watch integration (optional).

Focus implementation primarily on Phases 1–3. Phase 4 can be sketched or partially implemented if time allows.

⸻

3. Core runtime design (Phase 1)

3.1 Namespaces

Use consistent namespaces:
	•	Rndr for core types.
	•	Rndr.Layout for layout primitives & node tree.
	•	Rndr.Rendering for renderers.
	•	Rndr.Input for input events.
	•	Rndr.Navigation for navigation.

3.2 TuiApplication / builder / app (uses generic host)

TuiApplication

namespace Rndr;

public static class TuiApplication
{
    public static TuiAppBuilder CreateBuilder(string[] args)
        => new(new HostApplicationBuilder(args));
}

TuiAppBuilder

public sealed class TuiAppBuilder
{
    public HostApplicationBuilder HostBuilder { get; }
    public IServiceCollection Services => HostBuilder.Services;

    public TuiAppBuilder(HostApplicationBuilder hostBuilder)
    {
        HostBuilder = hostBuilder;
        // Register Rndr core services here (renderer, event loop, etc).
    }

    public TuiApp Build()
        => new(HostBuilder.Build());
}

TuiApp

public sealed class TuiApp
{
    private readonly IHost _host;

    public IServiceProvider Services => _host.Services;

    internal TuiApp(IHost host)
    {
        _host = host;
    }

    public TuiApp MapView(string route, Action<ViewDefinition> configure);

    public TuiApp MapView(string route, Type viewComponentType); // for .tui integration

    public TuiApp OnGlobalKey(Func<KeyEvent, GlobalContext, bool> handler);

    public Task RunAsync(CancellationToken cancellationToken = default);

    internal void Quit();
}

	•	CreateBuilder builds on the generic host (HostApplicationBuilder).
	•	Build() creates an IHost and then a TuiApp.
	•	MapView registers routes internally (e.g. dictionary).
	•	RunAsync resolves IEventLoop (see section 9.6) and calls it.

3.3 ViewDefinition & ViewContext

ViewDefinition

namespace Rndr;

public sealed class ViewDefinition
{
    internal ViewDefinition(string route);

    public string Route { get; }

    public ViewDefinition Title(string title);

    public ViewDefinition Use(Action<ViewContext> build);

    public ViewDefinition OnKey(Func<KeyEvent, ViewContext, bool> handler);
}

ViewContext

public sealed class ViewContext
{
    private readonly IStateStore _stateStore;

    public IServiceProvider Services { get; }
    public NavigationContext Navigation { get; }
    public ILogger Logger { get; }

    public string Route { get; }

    internal ViewContext(
        IServiceProvider services,
        NavigationContext navigation,
        IStateStore stateStore,
        ILogger logger,
        string route)
    {
        Services = services;
        Navigation = navigation;
        _stateStore = stateStore;
        Logger = logger;
        Route = route;
    }

    public Signal<T> State<T>(string key, T initialValue);

    public Signal<T> StateGlobal<T>(string key, T initialValue);
}

	•	State uses a route-based scope.
	•	StateGlobal uses a global scope (e.g. "global").

3.4 State: Signal + state store

Signal

namespace Rndr;

public interface ISignal
{
    object? UntypedValue { get; set; }
}

public sealed class Signal<T> : ISignal
{
    private readonly Action _onChanged;
    private T _value;

    public Signal(T initial, Action onChanged)
    {
        _value = initial;
        _onChanged = onChanged;
    }

    public T Value
    {
        get => _value;
        set
        {
            if (!EqualityComparer<T>.Default.Equals(_value, value))
            {
                _value = value;
                _onChanged();
            }
        }
    }

    object? ISignal.UntypedValue
    {
        get => _value;
        set => Value = (T)value!;
    }
}

IStateStore

namespace Rndr;

public interface IStateStore
{
    Signal<T> GetOrCreate<T>(string scopeKey, string key, Func<T> initialFactory);
}

public sealed class InMemoryStateStore : IStateStore
{
    private readonly Dictionary<(string Scope, string Key), ISignal> _signals = new();

    public Signal<T> GetOrCreate<T>(string scopeKey, string key, Func<T> initialFactory)
    {
        var tuple = (scopeKey, key);
        if (_signals.TryGetValue(tuple, out var existing))
            return (Signal<T>)existing;

        var signal = new Signal<T>(initialFactory(), () => { /* mark dirty / request rerender */ });
        _signals[tuple] = signal;
        return signal;
    }
}

	•	ViewContext.State(key, initial) uses scopeKey = Route.
	•	ViewContext.StateGlobal(key, initial) uses scopeKey = "global".

3.5 NavigationContext & navigation stack

NavigationContext

namespace Rndr.Navigation;

public sealed class NavigationContext
{
    private readonly Func<string, bool> _navigate;
    private readonly Func<bool> _back;
    private readonly Func<string, bool> _replace;

    internal NavigationContext(
        string currentRoute,
        Func<string, bool> navigate,
        Func<bool> back,
        Func<string, bool> replace)
    {
        CurrentRoute = currentRoute;
        _navigate = navigate;
        _back = back;
        _replace = replace;
    }

    public string CurrentRoute { get; private set; }

    public bool Navigate(string route)
        => _navigate(route);

    public bool Back()
        => _back();

    public bool Replace(string route)
        => _replace(route);
}

Navigation stack & INavigationState
	•	TuiApp maintains a navigation stack: Stack<string>.

namespace Rndr.Navigation;

public interface INavigationState
{
    string CurrentRoute { get; }
    IReadOnlyList<string> Stack { get; }
}

	•	Implement this in TuiApp or a dedicated class and register in DI.
	•	Every navigation operation updates INavigationState.

3.6 Input: KeyEvent & global key handling

KeyEvent

namespace Rndr.Input;

public sealed record KeyEvent(
    ConsoleKey Key,
    char KeyChar,
    ConsoleModifiers Modifiers);

GlobalContext

namespace Rndr;

public sealed class GlobalContext
{
    public NavigationContext Navigation { get; }
    public TuiApp Application { get; }

    internal GlobalContext(NavigationContext navigation, TuiApp app)
    {
        Navigation = navigation;
        Application = app;
    }
}

Global handlers
	•	TuiApp.OnGlobalKey maintains a list of handlers:

private readonly List<Func<KeyEvent, GlobalContext, bool>> _globalHandlers = new();

public TuiApp OnGlobalKey(Func<KeyEvent, GlobalContext, bool> handler)
{
    _globalHandlers.Add(handler);
    return this;
}

At runtime:
	•	For each key event:
	•	Call each global handler in order; if any returns true, stop.
	•	If none handle it, pass to current view’s OnKey (if configured).

3.7 Layout tree & builders

Node types

namespace Rndr.Layout;

public enum NodeKind
{
    Column,
    Row,
    Panel,
    Text,
    Button,
    TextInput,
    Spacer,
    Centered
}

public abstract class Node
{
    protected Node(NodeKind kind)
    {
        Kind = kind;
        Children = new List<Node>();
    }

    public NodeKind Kind { get; }
    public List<Node> Children { get; }
    public NodeStyle Style { get; set; } = new NodeStyle();
}

public sealed class ColumnNode : Node
{
    public ColumnNode() : base(NodeKind.Column) { }
}

public sealed class RowNode : Node
{
    public RowNode() : base(NodeKind.Row) { }
}

public sealed class PanelNode : Node
{
    public string Title { get; set; } = string.Empty;

    public PanelNode() : base(NodeKind.Panel) { }
}

public sealed class TextNode : Node
{
    public string Content { get; set; } = string.Empty;

    public TextNode() : base(NodeKind.Text) { }
}

public sealed class ButtonNode : Node
{
    public string Label { get; set; } = string.Empty;
    public Action? OnClick { get; set; }
    public bool IsPrimary { get; set; }
    public int? Width { get; set; }

    public ButtonNode() : base(NodeKind.Button) { }
}

// Similarly define TextInputNode, SpacerNode, CenteredNode.

NodeStyle (MVP)

namespace Rndr.Layout;

public sealed class NodeStyle
{
    public int? Padding { get; set; }
    public int? Gap { get; set; }
    public TextAlign? Align { get; set; }
    public bool Bold { get; set; }
    public bool Accent { get; set; }
    public bool Faint { get; set; }
}

public enum TextAlign
{
    Left,
    Center,
    Right
}

Builder APIs

namespace Rndr.Layout;

public sealed class LayoutBuilder
{
    private readonly List<Node> _children = new();
    private readonly NodeStyle _style = new();

    public IReadOnlyList<Node> Build() => _children;

    public LayoutBuilder Column(Action<ColumnBuilder> column)
    {
        var node = new ColumnNode { Style = _style };
        var cb = new ColumnBuilder(node);
        column(cb);
        _children.Add(node);
        return this;
    }

    public LayoutBuilder Row(Action<RowBuilder> row)
    {
        var node = new RowNode { Style = _style };
        var rb = new RowBuilder(node);
        row(rb);
        _children.Add(node);
        return this;
    }

    public LayoutBuilder Padding(int value)
    {
        _style.Padding = value;
        return this;
    }

    public LayoutBuilder Gap(int value)
    {
        _style.Gap = value;
        return this;
    }
}

public sealed class ColumnBuilder
{
    private readonly ColumnNode _node;

    public ColumnBuilder(ColumnNode node) => _node = node;

    public ColumnBuilder Text(string text, Action<NodeStyle>? configure = null)
    {
        var node = new TextNode { Content = text };
        configure?.Invoke(node.Style);
        _node.Children.Add(node);
        return this;
    }

    public ColumnBuilder Panel(string title, Action<LayoutBuilder> body)
    {
        var panel = new PanelNode { Title = title };
        var inner = new LayoutBuilder();
        body(inner);
        panel.Children.AddRange(inner.Build());
        _node.Children.Add(panel);
        return this;
    }

    public ColumnBuilder Button(string label, Action? onClick)
    {
        var node = new ButtonNode { Label = label, OnClick = onClick };
        _node.Children.Add(node);
        return this;
    }

    public ColumnBuilder Spacer(int weight = 1)
    {
        var node = new SpacerNode { /* weight if desired */ };
        _node.Children.Add(node);
        return this;
    }

    public ColumnBuilder Centered(Action<LayoutBuilder> child)
    {
        var centered = new CenteredNode();
        var inner = new LayoutBuilder();
        child(inner);
        centered.Children.AddRange(inner.Build());
        _node.Children.Add(centered);
        return this;
    }

    public ColumnBuilder Gap(int value)
    {
        _node.Style.Gap = value;
        return this;
    }

    public ColumnBuilder AlignCenter()
    {
        _node.Style.Align = TextAlign.Center;
        return this;
    }

    public ColumnBuilder Padding(int value)
    {
        _node.Style.Padding = value;
        return this;
    }
}

// RowBuilder similar, but oriented horizontally.

3.8 Rendering backend decision (IMPORTANT)

Decision: Implement a custom console renderer in Rndr.
Do not wrap Spectre.Console, Terminal.Gui, or other frameworks in core.

Rationale
	•	Full control over the look and behavior (beauty, spacing, borders).
	•	Avoids external dependencies that might constrain AOT, testing, or licensing.
	•	ITuiRenderer abstraction still allows alternate backends later if desired.

Interfaces

namespace Rndr.Rendering;

public interface ITuiRenderer
{
    void Render(IReadOnlyList<Node> rootNodes);
}

Console adapter (for testability)

namespace Rndr.Rendering;

public interface IConsoleAdapter
{
    int WindowWidth { get; }
    int WindowHeight { get; }
    void Clear();
    void WriteAt(int left, int top, string text,
                 ConsoleColor? foreground = null,
                 ConsoleColor? background = null);
    void HideCursor();
    void ShowCursor();
}

ConsoleRenderer

namespace Rndr.Rendering;

public sealed class ConsoleRenderer : ITuiRenderer
{
    private readonly IConsoleAdapter _console;
    private readonly RndrTheme _theme;

    public ConsoleRenderer(IConsoleAdapter console, RndrTheme theme)
    {
        _console = console;
        _theme = theme;
    }

    public void Render(IReadOnlyList<Node> rootNodes)
    {
        // MVP: simple layout, clear screen each frame.
        // Compute positions, draw panels, text, buttons using ASCII/Unicode.
    }
}

Implementation details:
	•	MVP layout can be simple:
	•	Columns stack vertically; rows distribute width.
	•	Panels draw borders using box-drawing chars.
	•	Use theme colors for accent, text, etc.
	•	Later you can optimize with double-buffering and diffing.

⸻

4. Phase 2: Navigation, shared state, theming

4.1 Navigation example

Ensure code like this works:

app.MapView("/", view =>
{
    view.Use(ctx =>
    {
        var nav = ctx.Navigation;

        var layout = new LayoutBuilder();
        layout.Column(col =>
        {
            col.Text("Home");
            col.Button("Go to Log", () => nav.Navigate("/log"));
        });
    });
});

app.MapView("/log", view =>
{
    view.Use(ctx =>
    {
        var nav = ctx.Navigation;
        var layout = new LayoutBuilder();
        layout.Column(col =>
        {
            col.Text("Log Page");
            col.Button("Back", () => nav.Back());
        });
    });
});

Implement internal mechanisms to:
	•	Switch the current route in TuiApp.
	•	Rebuild the view for the new route.
	•	Trigger rerender.

4.2 Shared state example (route vs global)

Use:

public sealed class FocusState
{
    public string CurrentTodo { get; set; } = "(No current todo)";
    public List<ActionEntry> Log { get; } = new();
}

// In Home view
var focus = ctx.StateGlobal("focus", new FocusState());

// In Log view
var focus = ctx.StateGlobal("focus", new FocusState());

Implement:
	•	State uses scopeKey = Route.
	•	StateGlobal uses scopeKey = "global".

4.3 Global key handling example

app.OnGlobalKey((key, globalCtx) =>
{
    if (key.KeyChar is 'q' or 'Q')
    {
        globalCtx.Application.Quit();
        return true;
    }

    if (key.KeyChar is 'h' or 'H')
    {
        globalCtx.Navigation.Navigate("/");
        return true;
    }

    if (key.KeyChar is 'l' or 'L')
    {
        globalCtx.Navigation.Navigate("/log");
        return true;
    }

    return false;
});

4.4 Theming (basic)

Theme objects

namespace Rndr;

public sealed class RndrTheme
{
    public ConsoleColor AccentColor { get; set; } = ConsoleColor.Cyan;
    public ConsoleColor TextColor { get; set; } = ConsoleColor.Gray;
    public ConsoleColor MutedTextColor { get; set; } = ConsoleColor.DarkGray;
    public ConsoleColor ErrorColor { get; set; } = ConsoleColor.Red;

    public PanelTheme Panel { get; } = new PanelTheme();
    public int SpacingUnit { get; set; } = 1;
}

public sealed class PanelTheme
{
    public BorderStyle BorderStyle { get; set; } = BorderStyle.Rounded;
}

public enum BorderStyle
{
    Square,
    Rounded
}

Registration

namespace Rndr;

public static class RndrServiceCollectionExtensions
{
    public static IServiceCollection AddRndrTheme(
        this IServiceCollection services,
        Action<RndrTheme>? configure = null)
    {
        var theme = new RndrTheme();
        configure?.Invoke(theme);
        services.AddSingleton(theme);
        return services;
    }
}

ConsoleRenderer uses the theme when drawing.

⸻

5. Phase 3: .tui Razor integration (Vue-like SFCs)

5.1 .tui format

Example:

@view
@using Rndr
@inject ITimeService Time

@code {
    var count = State("count", 0);

    void Increment() => count.Value++;
    void Decrement() => count.Value--;
}

<Column Padding="1" Gap="1">
  <Spacer />

  <Centered>
    <Panel Title="Counter">
      <Column Gap="1">
        <Text Align="Center" Bold="true" Accent="true">
          Count: @count.Value
        </Text>

        <Text Align="Center" Faint="true">
          Last updated at @Time.Now.ToLongTimeString()
        </Text>

        <Row Gap="1" Align="Center">
          <Button Width="8" OnClick="@Decrement">-</Button>
          <Button Width="8" Primary="true" OnClick="@Increment">+</Button>
        </Row>
      </Column>
    </Panel>
  </Centered>

  <Spacer />
</Column>

5.2 TuiComponentBase

namespace Rndr;

public abstract class TuiComponentBase
{
    protected ViewContext Context { get; private set; } = default!;

    internal void AttachContext(ViewContext context)
    {
        Context = context;
        OnInit();
    }

    protected virtual void OnInit() { }

    protected Signal<T> State<T>(string key, T initialValue)
        => Context.State(key, initialValue);

    protected Signal<T> StateGlobal<T>(string key, T initialValue)
        => Context.StateGlobal(key, initialValue);

    public abstract void Build(LayoutBuilder layout);
}

MapView with component type

public TuiApp MapView(string route, Type viewComponentType)
{
    // Validate type derives from TuiComponentBase,
    // store Type in route table, and at runtime instantiate via DI.
}

At runtime:
	•	Resolve the component type via DI.
	•	Call AttachContext.
	•	For each render, create a new LayoutBuilder, call Build, get nodes, render.

5.3 Razor integration (Rndr.Razor)
	•	Use Razor SDK to treat .tui as Razor files.
	•	Provide an MSBuild item type:

<ItemGroup>
  <TuiComponent Include="Pages\**\*.tui" />
</ItemGroup>

	•	Implement a Razor configuration that:
	•	Recognizes @view directive.
	•	Maps <Column>, <Row>, <Panel>, <Text>, <Button>, <TextInput>, <Spacer>, <Centered> to methods on LayoutBuilder, ColumnBuilder, RowBuilder, etc.
	•	The generator should emit partial classes that inherit TuiComponentBase and implement Build(LayoutBuilder layout).

Mapping rules (simplified)
	•	Root markup inside .tui → body of Build(LayoutBuilder layout).

Examples:
	•	<Column Padding="1" Gap="1">...</Column>
→ layout.Column(col => { ... }).Padding(1).Gap(1);
	•	<Row Gap="1" Align="Center">...</Row>
→ col.Row(row => { ... }).Gap(1).AlignCenter();
	•	<Panel Title="X"> body </Panel>
→ col.Panel("X", bodyLayout => { /* children */ });
	•	<Text Bold="true" Accent="true" Align="Center">Value</Text>
→ col.Text("Value", style => { style.Bold = true; style.Accent = true; }).AlignCenter();
	•	<Button Width="14" Primary="true" OnClick="@Handler">Caption</Button>
→ col.Button("Caption", Handler).Width(14).Primary();
	•	<TextInput Value="@editingText.Value" OnChanged="@(t => editingText.Value = t)" Placeholder="..."/>
→ col.TextInput(editingText.Value, t => editingText.Value = t, placeholder: "...");

The generator does not need to support every Razor feature; focus on what is required for the sample app and general layouts.

⸻

6. Example app: “Most Important Todo + Log”

Ensure Rndr supports this sample app as a .tui-based project.

6.1 Behavior
	•	Track a single “Most Important Todo” string.
	•	Buttons:
	•	Start → log Started: "<todo>".
	•	Finish → log Finished: "<todo>".
	•	Edit → show text input to change the todo.
	•	Clear → reset todo to (No current todo) and log the clear action.
	•	ActionEntry log:
	•	Timestamp + message.
	•	Displayed in reverse chronological order.
	•	Two pages:
	•	/ = main screen, shows current todo and last few log entries.
	•	/log = full log view.
	•	Navigation:
	•	Button “View Full Log” → /log.
	•	Button “Back” → /.
	•	Global keys:
	•	H/h → /
	•	L/l → /log
	•	Q/q → Quit.

6.2 Sample project structure

Rndr.Samples.MyFocusTui/
  MyFocusTui.csproj
  Program.cs
  Models/
    ActionEntry.cs
    FocusState.cs
  Pages/
    Home.tui
    Log.tui

Implement the models and pages as described. They should demonstrate:
	•	Global state via StateGlobal("focus", new FocusState()).
	•	Navigation via Navigation.Navigate.
	•	Layout with Column, Row, Panel, Text, Button, TextInput, Spacer.
	•	Theme usage for nice visuals.

⸻

7. Non-goals (for now)
	•	Mouse support (design for future, but not required in MVP).
	•	Advanced layout (wrapping, alignment beyond simple rows/columns).
	•	Full plugin system.
	•	Dynamic runtime view discovery in core (to stay AOT-friendly).
	•	Full-blown inspections/analyzers (can be outlined but not fully implemented in initial version).

⸻

8. Deliverables
	1.	Core library Rndr:
	•	TuiApplication, TuiAppBuilder, TuiApp.
	•	ViewDefinition, ViewContext, NavigationContext, GlobalContext.
	•	Signal<T>, IStateStore, InMemoryStateStore.
	•	Layout node types and builder APIs.
	•	IConsoleAdapter, ITuiRenderer, ConsoleRenderer.
	•	Basic theme support (RndrTheme).
	•	Diagnostics hooks (RndrDiagnostics, see 9.9).
	2.	Razor integration Rndr.Razor:
	•	MSBuild items/targets for *.tui.
	•	Razor configuration & generator that outputs TuiComponentBase partials.
	•	Mapping for tags and attributes as described.
	3.	Sample app Rndr.Samples.MyFocusTui:
	•	Implemented & runnable using .tui views and navigation.
	4.	README for Rndr:
	•	Goals, quickstart, code samples, and explanation of the sample app.
	5.	Tests:
	•	Unit tests for Signal<T>, InMemoryStateStore.
	•	Navigation stack behavior (INavigationState).
	•	Smoke tests for layout building and rendering (using fake console).

⸻

9. Testability & .NET ecosystem integration (REQUIRED)

To ensure Rndr apps are highly testable and integrate naturally with the .NET ecosystem, the implementation must follow these guidelines.

9.1 Use the generic host
	•	TuiApplication.CreateBuilder(args) must wrap HostApplicationBuilder, not a custom DI container.
	•	TuiAppBuilder must expose:
	•	HostApplicationBuilder HostBuilder { get; }
	•	IServiceCollection Services => HostBuilder.Services
	•	TuiApp.Build() must call HostBuilder.Build() and keep a reference to the created IHost.

This gives:
	•	First-class IConfiguration, IHostEnvironment, ILogger<>, IHostApplicationLifetime.
	•	Ability to run Rndr side-by-side with ASP.NET, workers, etc.

9.2 Abstract I/O for testability

Define and use:

namespace Rndr.Rendering;

public interface IConsoleAdapter
{
    int WindowWidth { get; }
    int WindowHeight { get; }
    void Clear();
    void WriteAt(int left, int top, string text,
                 ConsoleColor? foreground = null,
                 ConsoleColor? background = null);
    void HideCursor();
    void ShowCursor();
}

namespace Rndr.Input;

public interface IInputSource
{
    KeyEvent ReadKey(bool intercept);
}

namespace Rndr;

public interface IClock
{
    DateTime Now { get; }
}

	•	Provide real implementations wrapping System.Console and DateTime.Now.
	•	Inject IConsoleAdapter, IInputSource, IClock into renderer/event loop.
	•	Tests can use fakes instead of touching the real console or clock.

9.3 View purity & test helpers
	•	TuiComponentBase.Build(LayoutBuilder layout) and C# view delegates registered via MapView(route, Action<ViewDefinition>) must not directly call Console or perform I/O.
	•	All side effects should be:
	•	In injected services (ViewContext.Services), or
	•	In event handlers (e.g., button click handlers, async services), not inside the pure layout description.

Provide a test helper:

namespace Rndr.Testing;

public static class RndrTestHost
{
    public static (IReadOnlyList<Node> Nodes, ViewContext Context) BuildComponent<TComponent>(
        IServiceProvider? services = null,
        IStateStore? stateStore = null)
        where TComponent : TuiComponentBase;
}

Behavior:
	•	Create a ServiceCollection if services is null.
	•	Register required Rndr core services.
	•	Build ViewContext with a fake NavigationContext, IStateStore, ILogger.
	•	Instantiate TComponent via DI, call AttachContext, then Build.
	•	Return the node tree and context so tests can inspect the layout and state.

9.4 Explicit state scopes

Implement and document:

public sealed class ViewContext
{
    public Signal<T> State<T>(string key, T initialValue);        // route-scoped
    public Signal<T> StateGlobal<T>(string key, T initialValue);  // app-scoped
}

	•	State: scopeKey = current route.
	•	StateGlobal: scopeKey = "global".

Make sure IStateStore supports both scopes.

9.5 Navigation observability

Use INavigationState:

namespace Rndr.Navigation;

public interface INavigationState
{
    string CurrentRoute { get; }
    IReadOnlyList<string> Stack { get; }
}

	•	Register an implementation in DI.
	•	Keep it updated from NavigationContext operations (Navigate, Back, Replace).
	•	Tests can assert on Stack contents and CurrentRoute.

9.6 Event loop abstraction

Define:

namespace Rndr;

public interface IEventLoop
{
    Task RunAsync(TuiApp app, CancellationToken cancellationToken = default);
}

public sealed class DefaultEventLoop : IEventLoop
{
    // Uses IInputSource, ITuiRenderer, IStateStore, etc.
}

	•	TuiApp.RunAsync must resolve IEventLoop from DI and call it.
	•	Tests can register a TestEventLoop that:
	•	Feeds scripted KeyEvents.
	•	Drives the app deterministically.
	•	Observes state/navigation changes.

9.7 Logging & options
	•	ViewContext must expose:

public ILogger Logger { get; }

	•	Define RndrOptions:

namespace Rndr;

public sealed class RndrOptions
{
    public bool EnableDoubleBuffering { get; set; } = true;
    public bool UseAnsiColors { get; set; } = true;
    public TimeSpan IdleFrameDelay { get; set; } = TimeSpan.FromMilliseconds(16);
}

	•	Register via IOptions<RndrOptions> and use IOptionsMonitor<RndrOptions> where appropriate.
	•	Allow configuration via IConfiguration (e.g. builder.HostBuilder.Configuration.GetSection("Rndr")).

9.8 Analyzers (optional but recommended)

Plan (at least in design) for a Rndr.Analyzers project with rules:
	•	Forbid direct Console usage in TuiComponentBase descendants and view delegates.
	•	Warn on static mutable fields in component classes.
	•	Warn when Build methods perform direct file/network I/O.

Implementation of analyzers can be deferred, but APIs and patterns should anticipate them.

9.9 OpenTelemetry friendliness

Rndr must be OpenTelemetry-friendly, but not hard-dependent on OTel.

Requirements:
	1.	Diagnostics entry points:
	•	Provide a static diagnostics class:

namespace Rndr.Diagnostics;

public static class RndrDiagnostics
{
    public static readonly ActivitySource ActivitySource = new("Rndr.Core");
    public static readonly Meter Meter = new("Rndr.Core");
}


	2.	Instrument core operations:
Use RndrDiagnostics.ActivitySource to create activities around:
	•	Navigation:

using var activity = RndrDiagnostics.ActivitySource.StartActivity("Navigate");
activity?.SetTag("route.from", currentRoute);
activity?.SetTag("route.to", newRoute);


	•	Render cycle:

using var activity = RndrDiagnostics.ActivitySource.StartActivity("RenderFrame");


	•	Key events:

using var activity = RndrDiagnostics.ActivitySource.StartActivity("KeyEvent");
activity?.SetTag("key", key.Key.ToString());
activity?.SetTag("char", key.KeyChar);


Use RndrDiagnostics.Meter to expose basic metrics, for example:
	•	Counter: frames rendered.
	•	Histogram: render duration.
Metric names can follow a simple pattern such as rndr.frames_rendered, rndr.render_duration_ms.

	3.	OTel configuration stays in the host:
The host app configures OpenTelemetry using normal patterns; for example:

builder.HostBuilder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing
            .AddSource("Rndr.Core")
            .AddSource("MyApp.*")
            .AddConsoleExporter();
    })
    .WithMetrics(metrics =>
    {
        metrics
            .AddMeter("Rndr.Core")
            .AddMeter("MyApp.*")
            .AddConsoleExporter();
    });

Rndr itself should not directly depend on OpenTelemetry packages, but it may target System.Diagnostics.DiagnosticSource and System.Diagnostics.Metrics, which OTel uses.

	4.	User code can use ActivitySource:
Document (in README or XML docs) how user code can:

private static readonly ActivitySource ActivitySource = new("MyApp.Focus");

void StartTodo(string todo)
{
    using var activity = ActivitySource.StartActivity("StartTodo");
    activity?.SetTag("todo.text", todo);
    // ...
}

This should integrate naturally into OTel traces when the host enables it.

⸻

Use this spec as your single source of truth for Rndr.
Where something is underspecified, choose the option that:
	•	Feels most natural to a .NET dev used to Minimal APIs + Vue,
	•	Keeps the implementation AOT-friendly and testable, and
	•	Keeps the public API small, consistent, and discoverable.