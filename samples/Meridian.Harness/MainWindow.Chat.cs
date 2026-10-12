// ============================================================================
// Meridian
// File: MainWindow.Chat.cs
// Author: Angel Hernandez (me@angelhernandezm.com)
// Description:
// The Chat tab: stateless, single-turn chat against the configured default
// IChatClient, with an optional system instruction -- the direct-.NET
// equivalent of Meridian.Interop.Com's IMeridianChatService.
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
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;

namespace Meridian.Harness;

/// <summary>
/// Class MainWindow.
/// Implements the <see cref="Window" />
/// </summary>
/// <seealso cref="Window" />
partial class MainWindow {
    /// <summary>
    /// The chat instructions
    /// </summary>
    private readonly TextBox _chatInstructions = new() { AcceptsReturn = true, Height = 50, Watermark = "System instructions (optional)" };
    /// <summary>
    /// The chat prompt
    /// </summary>
    private readonly TextBox _chatPrompt = new() { AcceptsReturn = true, Height = 70, Watermark = "Your message" };
    /// <summary>
    /// The chat output
    /// </summary>
    private readonly TextBox _chatOutput = Output(220);

    /// <summary>
    /// Builds the Chat tab.
    /// </summary>
    /// <returns>The configured <see cref="TabItem" />.</returns>
    private TabItem BuildChatTab() {
        var root = Stack();
        root.Children.Add(Heading("Stateless chat"));
        root.Children.Add(Help("One prompt in, one reply out. No history kept between calls -- see the Agent tab for multi-turn conversations."));
        root.Children.Add(_chatInstructions);
        root.Children.Add(_chatPrompt);

        var sendButton = new Button { Content = "Send" };
        sendButton.Click += (_, _) => RunSafely(sendButton, _chatOutput, "Sending...", SendChatAsync);
        root.Children.Add(sendButton);
        root.Children.Add(_chatOutput);

        return new TabItem { Header = "Chat", Content = Page(root) };
    }

    /// <summary>
    /// Send chat as an asynchronous operation.
    /// </summary>
    /// <returns>A Task representing the asynchronous operation.</returns>
    private async Task SendChatAsync() {
        if (!RequireServices(_chatOutput))
            return;

        var prompt = _chatPrompt.Text;
        if (string.IsNullOrWhiteSpace(prompt)) {
            _chatOutput.Text = "Enter a message first.";
            return;
        }

        var chatClient = _services!.GetRequiredService<IChatClient>();
        var messages = new List<ChatMessage>();
        if (!string.IsNullOrWhiteSpace(_chatInstructions.Text))
            messages.Add(new ChatMessage(ChatRole.System, _chatInstructions.Text));
        messages.Add(new ChatMessage(ChatRole.User, prompt));

        var response = await chatClient.GetResponseAsync(messages).ConfigureAwait(true);
        _chatOutput.Text = response.Text;
    }
}