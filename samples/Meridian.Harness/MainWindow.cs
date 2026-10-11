// ============================================================================
// Meridian
// File: MainWindow.cs
// Author: Angel Hernandez (me@angelhernandezm.com)
// Description:
// The harness's single window: a TabControl with one tab per Meridian capability
// (Setup, Chat, Agent, Embeddings, Knowledge/RAG, Memory, Images). Each tab is
// built in its own partial-class file (MainWindow.<Tab>.cs); this file holds the
// window shell, the shared service-provider/agent-session state every tab reads,
// and small UI helpers reused across tabs.
//
// License: MIT
// ============================================================================
//
// MIT License
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// ============================================================================

using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Microsoft.Agents.AI;

namespace Meridian.Harness;

/// <summary>
/// The harness's main (and only) window.
/// </summary>
public sealed partial class MainWindow : Window {
    /// <summary>
    /// The Meridian service provider built by the Setup tab's "Initialize" button, or
    /// <see langword="null" /> before initialization.
    /// </summary>
    private IServiceProvider? _services;

    /// <summary>
    /// The "assistant" agent resolved from <see cref="_services" />, cached for the lifetime of the
    /// current initialization so the Agent tab doesn't re-resolve it on every message.
    /// </summary>
    private AIAgent? _agent;

    /// <summary>
    /// The active multi-turn session for <see cref="_agent" />, or <see langword="null" /> until the
    /// Agent tab's "New session" button is pressed (or the first message is sent).
    /// </summary>
    private AgentSession? _agentSession;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindow" /> class, building every tab.
    /// </summary>
    public MainWindow() {
        Title = "Meridian Harness (cross-platform, no COM)";
        Width = 980;
        Height = 760;
        MinWidth = 760;
        MinHeight = 560;

        var tabs = new TabControl();
        tabs.Items.Add(BuildSetupTab());
        tabs.Items.Add(BuildChatTab());
        tabs.Items.Add(BuildAgentTab());
        tabs.Items.Add(BuildEmbeddingsTab());
        tabs.Items.Add(BuildKnowledgeTab());
        tabs.Items.Add(BuildMemoryTab());
        tabs.Items.Add(BuildImagesTab());

        Content = tabs;
    }

    /// <summary>
    /// Wraps a tab's root panel in a padded, scrollable container, consistent across every tab.
    /// </summary>
    /// <param name="content">The tab's root content panel.</param>
    /// <returns>A <see cref="ScrollViewer" /> ready to assign as a <see cref="TabItem" />'s content.</returns>
    private static ScrollViewer Page(Control content) => new() {
        Content = new Border { Padding = new Avalonia.Thickness(16), Child = content }
    };

    /// <summary>
    /// Builds a vertical stack panel with consistent spacing, used as the root of every tab.
    /// </summary>
    /// <param name="spacing">The spacing between children, in pixels.</param>
    /// <returns>The configured <see cref="StackPanel" />.</returns>
    private static StackPanel Stack(double spacing = 10) => new() { Orientation = Orientation.Vertical, Spacing = spacing };

    /// <summary>
    /// Builds a bold section-heading <see cref="TextBlock" />.
    /// </summary>
    /// <param name="text">The heading text.</param>
    /// <returns>The configured <see cref="TextBlock" />.</returns>
    private static TextBlock Heading(string text) => new() { Text = text, FontWeight = FontWeight.Bold, FontSize = 15 };

    /// <summary>
    /// Builds a muted, wrapping helper/description <see cref="TextBlock" />.
    /// </summary>
    /// <param name="text">The helper text.</param>
    /// <returns>The configured <see cref="TextBlock" />.</returns>
    private static TextBlock Help(string text) => new() { Text = text, TextWrapping = TextWrapping.Wrap, Opacity = 0.7, FontSize = 12 };

    /// <summary>
    /// Builds a read-only, wrapping, scrollable output/status <see cref="TextBox" /> used across tabs
    /// to show responses, errors, and results.
    /// </summary>
    /// <param name="minHeight">The control's minimum height, in pixels.</param>
    /// <returns>The configured output <see cref="TextBox" />.</returns>
    private static TextBox Output(double minHeight = 120) => new() {
        IsReadOnly = true,
        AcceptsReturn = true,
        TextWrapping = TextWrapping.Wrap,
        MinHeight = minHeight,
        FontFamily = new FontFamily("Consolas,Menlo,monospace")
    };

    /// <summary>
    /// Returns <see langword="true" /> when Meridian has been initialized (the Setup tab's "Initialize"
    /// button has run successfully); otherwise writes a reminder to <paramref name="status" /> and returns
    /// <see langword="false" />.
    /// </summary>
    /// <param name="status">The calling tab's status/output control to write the reminder to.</param>
    /// <returns><see langword="true" /> if <see cref="_services" /> is set; otherwise <see langword="false" />.</returns>
    private bool RequireServices(TextBox status) {
        if (_services is not null)
            return true;

        status.Text = "Not initialized. Go to the Setup tab, configure a provider, and click \"Initialize Meridian\" first.";
        return false;
    }

    /// <summary>
    /// Runs <paramref name="work" />, disabling <paramref name="trigger" /> for its duration and routing
    /// any exception's message to <paramref name="status" /> instead of letting it escape onto the UI thread.
    /// </summary>
    /// <param name="trigger">The button that started the operation, disabled while it runs.</param>
    /// <param name="status">The output control to report progress/errors to.</param>
    /// <param name="busyText">The text shown in <paramref name="status" /> while <paramref name="work" /> runs.</param>
    /// <param name="work">The asynchronous operation to run.</param>
    private static async void RunSafely(Button trigger, TextBox status, string busyText, Func<Task> work) {
        trigger.IsEnabled = false;
        status.Text = busyText;
        try {
            await work().ConfigureAwait(true);
        } catch (Exception ex) {
            status.Text = $"Error: {ex.Message}";
        } finally {
            trigger.IsEnabled = true;
        }
    }
}
