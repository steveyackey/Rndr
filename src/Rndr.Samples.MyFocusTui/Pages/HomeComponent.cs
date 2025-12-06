using Rndr;
using Rndr.Layout;
using Rndr.Samples.MyFocusTui.Models;

namespace Rndr.Samples.MyFocusTui.Pages;

/// <summary>
/// Home page component for the MyFocus TUI app.
/// Demonstrates global state, buttons, panels, and navigation.
/// </summary>
public sealed class HomeComponent : TuiComponentBase
{
    public override void Build(LayoutBuilder layout)
    {
        var state = StateGlobal("focus", new FocusState());

        layout.Column(col =>
        {
            col.Padding(1).Gap(1);

            // Header
            col.Text("🎯 MyFocus TUI", style =>
            {
                style.Bold = true;
                style.Accent = true;
            });
            col.Text("Stay focused. Get things done.", style => style.Faint = true);

            // Current Focus Panel
            col.Panel("Current Focus", panel =>
            {
                panel.Column(inner =>
                {
                    inner.Padding(1).Gap(1);

                    if (state.Value.HasActiveFocus)
                    {
                        inner.Text(state.Value.CurrentTodo!, style =>
                        {
                            style.Bold = true;
                            style.Accent = true;
                        });

                        inner.Row(row =>
                        {
                            row.Gap(2);
                            row.Button("✓ Finish", () =>
                            {
                                var todo = state.Value.CurrentTodo!;
                                state.Value.Log.Add(new ActionEntry
                                {
                                    Timestamp = DateTime.Now,
                                    Message = $"Completed: {todo}"
                                });
                                state.Value.CurrentTodo = null;
                            });
                            row.Button("✕ Clear", () =>
                            {
                                var todo = state.Value.CurrentTodo!;
                                state.Value.Log.Add(new ActionEntry
                                {
                                    Timestamp = DateTime.Now,
                                    Message = $"Cleared: {todo}"
                                });
                                state.Value.CurrentTodo = null;
                            });
                        });
                    }
                    else
                    {
                        inner.Text("No active focus", style => style.Faint = true);
                        inner.Button("+ Start Focus", () =>
                        {
                            // Simple default for demo
                            state.Value.CurrentTodo = "Working on important task";
                            state.Value.Log.Add(new ActionEntry
                            {
                                Timestamp = DateTime.Now,
                                Message = $"Started: {state.Value.CurrentTodo}"
                            });
                        });
                    }
                });
            });

            // Recent Activity Panel
            col.Panel("Recent Activity", panel =>
            {
                panel.Column(inner =>
                {
                    inner.Padding(1);

                    if (state.Value.Log.Count == 0)
                    {
                        inner.Text("No activity yet", style => style.Faint = true);
                    }
                    else
                    {
                        // Show last 3 entries
                        var recent = state.Value.Log
                            .OrderByDescending(e => e.Timestamp)
                            .Take(3);

                        foreach (var entry in recent)
                        {
                            inner.Text($"{entry.Timestamp:HH:mm} - {entry.Message}");
                        }

                        if (state.Value.Log.Count > 3)
                        {
                            inner.Text($"  ... and {state.Value.Log.Count - 3} more", style => style.Faint = true);
                        }
                    }
                });
            });

            // Footer
            col.Spacer();
            col.Text("[Q] Quit  [L] View Log  [Tab] Navigate", style => style.Faint = true);
        });
    }
}

