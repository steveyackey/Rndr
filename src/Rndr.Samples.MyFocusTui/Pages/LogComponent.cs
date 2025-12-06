using Rndr;
using Rndr.Layout;
using Rndr.Samples.MyFocusTui.Models;

namespace Rndr.Samples.MyFocusTui.Pages;

/// <summary>
/// Log page component showing full activity history.
/// </summary>
public sealed class LogComponent : TuiComponentBase
{
    public override void Build(LayoutBuilder layout)
    {
        var state = StateGlobal("focus", new FocusState());

        layout.Column(col =>
        {
            col.Padding(1).Gap(1);

            // Header
            col.Text("📋 Activity Log", style =>
            {
                style.Bold = true;
                style.Accent = true;
            });

            // Log Panel
            col.Panel("All Activity", panel =>
            {
                panel.Column(inner =>
                {
                    inner.Padding(1);

                    if (state.Value.Log.Count == 0)
                    {
                        inner.Text("No activity recorded yet.", style => style.Faint = true);
                        inner.Text("Start a focus session from the home screen.", style => style.Faint = true);
                    }
                    else
                    {
                        foreach (var entry in state.Value.Log.OrderByDescending(e => e.Timestamp))
                        {
                            inner.Text($"{entry.Timestamp:yyyy-MM-dd HH:mm:ss} - {entry.Message}");
                        }
                    }
                });
            });

            // Navigation
            col.Spacer();
            col.Row(row =>
            {
                row.Gap(2);
                row.Button("← Back to Home", () => Context.Navigation.Back());
                row.Button("Clear All", () => state.Value.Log.Clear());
            });

            col.Text("[Q] Quit  [H] Home  [Tab] Navigate", style => style.Faint = true);
        });
    }
}

