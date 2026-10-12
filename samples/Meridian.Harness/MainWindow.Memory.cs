// ============================================================================
// Meridian
// File: MainWindow.Memory.cs
// Author: Angel Hernandez (me@angelhernandezm.com)
// Description:
// The Memory tab: remember/recall/forget against Meridian.Agents' long-term
// semantic memory -- the direct-.NET equivalent of Meridian.Interop.Com's
// IMeridianMemoryService.
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
using Meridian.Agents.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace Meridian.Harness;

/// <summary>
/// Class MainWindow.
/// Implements the <see cref="Window" />
/// </summary>
/// <seealso cref="Window" />
partial class MainWindow {
    /// <summary>
    /// The remember text
    /// </summary>
    private readonly TextBox _rememberText = new() { AcceptsReturn = true, Height = 60, Watermark = "Fact to remember" };
    /// <summary>
    /// The recall query
    /// </summary>
    private readonly TextBox _recallQuery = new() { Watermark = "Recall query" };
    /// <summary>
    /// The recall top k
    /// </summary>
    private readonly NumericUpDown _recallTopK = new() { Value = 5, Minimum = 1, Maximum = 50, Width = 90 };
    /// <summary>
    /// The forget identifier
    /// </summary>
    private readonly TextBox _forgetId = new() { Watermark = "Id to forget" };
    /// <summary>
    /// The memory output
    /// </summary>
    private readonly TextBox _memoryOutput = Output(200);

    /// <summary>
    /// Builds the Memory tab.
    /// </summary>
    /// <returns>The configured <see cref="TabItem" />.</returns>
    private TabItem BuildMemoryTab() {
        var root = Stack();
        root.Children.Add(Heading("Long-term memory"));
        root.Children.Add(Help("Requires \"Long-term memory\" enabled on the Setup tab. Independent of any chat/agent session's transcript."));

        root.Children.Add(_rememberText);
        var rememberButton = new Button { Content = "Remember" };
        rememberButton.Click += (_, _) => RunSafely(rememberButton, _memoryOutput, "Saving...", RememberAsync);
        root.Children.Add(rememberButton);

        root.Children.Add(new Separator());
        var recallRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        recallRow.Children.Add(_recallQuery);
        recallRow.Children.Add(new TextBlock { Text = "Top K:", VerticalAlignment = VerticalAlignment.Center });
        recallRow.Children.Add(_recallTopK);
        root.Children.Add(recallRow);
        var recallButton = new Button { Content = "Recall" };
        recallButton.Click += (_, _) => RunSafely(recallButton, _memoryOutput, "Recalling...", RecallAsync);
        root.Children.Add(recallButton);

        root.Children.Add(new Separator());
        root.Children.Add(_forgetId);
        var forgetButton = new Button { Content = "Forget" };
        forgetButton.Click += (_, _) => RunSafely(forgetButton, _memoryOutput, "Forgetting...", ForgetAsync);
        root.Children.Add(forgetButton);

        root.Children.Add(_memoryOutput);
        return new TabItem { Header = "Memory", Content = Page(root) };
    }

    /// <summary>
    /// Remember as an asynchronous operation.
    /// </summary>
    /// <returns>A Task representing the asynchronous operation.</returns>
    private async Task RememberAsync() {
        if (!RequireServices(_memoryOutput))
            return;

        if (string.IsNullOrWhiteSpace(_rememberText.Text)) {
            _memoryOutput.Text = "Enter a fact to remember first.";
            return;
        }

        var memory = _services!.GetRequiredService<IMeridianMemory>();
        var id = await memory.RememberAsync(_rememberText.Text).ConfigureAwait(true);
        _memoryOutput.Text = $"Remembered as id \"{id}\".";
        _rememberText.Text = string.Empty;
    }

    /// <summary>
    /// Recall as an asynchronous operation.
    /// </summary>
    /// <returns>A Task representing the asynchronous operation.</returns>
    private async Task RecallAsync() {
        if (!RequireServices(_memoryOutput))
            return;

        if (string.IsNullOrWhiteSpace(_recallQuery.Text)) {
            _memoryOutput.Text = "Enter a recall query first.";
            return;
        }

        var memory = _services!.GetRequiredService<IMeridianMemory>();
        var topK = (int)(_recallTopK.Value ?? 5);
        var results = await memory.RecallAsync(_recallQuery.Text, topK).ConfigureAwait(true);

        _memoryOutput.Text = results.Count == 0
            ? "Nothing relevant found."
            : string.Join("\n\n", results.Select(m => $"id={m.Id} score={m.Score:F4}\n{m.Text}"));
    }

    /// <summary>
    /// Forget as an asynchronous operation.
    /// </summary>
    /// <returns>A Task representing the asynchronous operation.</returns>
    private async Task ForgetAsync() {
        if (!RequireServices(_memoryOutput))
            return;

        if (string.IsNullOrWhiteSpace(_forgetId.Text)) {
            _memoryOutput.Text = "Enter the id to forget first.";
            return;
        }

        var memory = _services!.GetRequiredService<IMeridianMemory>();
        await memory.ForgetAsync(_forgetId.Text).ConfigureAwait(true);
        _memoryOutput.Text = $"Forgot id \"{_forgetId.Text}\" (if it existed).";
        _forgetId.Text = string.Empty;
    }
}
