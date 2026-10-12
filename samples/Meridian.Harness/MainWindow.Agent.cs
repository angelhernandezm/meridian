// ============================================================================
// Meridian
// File: MainWindow.Agent.cs
// Author: Angel Hernandez (me@angelhernandezm.com)
// Description:
// The Agent tab: multi-turn conversation with the "assistant" agent configured
// on the Setup tab, keeping one AgentSession alive across messages -- the
// direct-.NET equivalent of Meridian.Interop.Com's IMeridianAgentService.
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

namespace Meridian.Harness;

partial class MainWindow {
    /// <summary>
    /// The agent transcript
    /// </summary>
    private readonly TextBox _agentTranscript = Output(260);
    /// <summary>
    /// The agent message
    /// </summary>
    private readonly TextBox _agentMessage = new() { Watermark = "Message to the agent" };

    /// <summary>
    /// Builds the Agent tab.
    /// </summary>
    /// <returns>The configured <see cref="TabItem" />.</returns>
    private TabItem BuildAgentTab() {
        var root = Stack();
        root.Children.Add(Heading("Multi-turn agent session"));
        root.Children.Add(Help("Talks to the \"assistant\" agent configured on the Setup tab. The session (conversation memory) stays open until you click \"New session\" or re-initialize."));
        root.Children.Add(_agentTranscript);

        var newSessionButton = new Button { Content = "New session" };
        newSessionButton.Click += (_, _) => RunSafely(newSessionButton, _agentTranscript, "Starting new session...", NewAgentSessionAsync);
        root.Children.Add(newSessionButton);

        root.Children.Add(_agentMessage);
        var sendButton = new Button { Content = "Send" };
        sendButton.Click += (_, _) => RunSafely(sendButton, _agentTranscript, "Thinking...", SendAgentMessageAsync);
        root.Children.Add(sendButton);

        return new TabItem { Header = "Agent", Content = Page(root) };
    }

    /// <summary>
    /// New agent session as an asynchronous operation.
    /// </summary>
    /// <returns>A Task representing the asynchronous operation.</returns>
    private async Task NewAgentSessionAsync() {
        if (!RequireServices(_agentTranscript))
            return;

        _agentSession = await _agent!.CreateSessionAsync().ConfigureAwait(true);
        _agentTranscript.Text = "New session started.";
    }

    /// <summary>
    /// Send agent message as an asynchronous operation.
    /// </summary>
    /// <returns>A Task representing the asynchronous operation.</returns>
    private async Task SendAgentMessageAsync() {
        if (!RequireServices(_agentTranscript))
            return;

        var message = _agentMessage.Text;
        if (string.IsNullOrWhiteSpace(message))
            return;

        _agentSession ??= await _agent!.CreateSessionAsync().ConfigureAwait(true);

        var transcriptSoFar = _agentTranscript.Text is null or "New session started." ? string.Empty : _agentTranscript.Text + "\n\n";
        _agentTranscript.Text = transcriptSoFar + $"You: {message}\n\nAssistant: (thinking...)";

        var response = await _agent!.RunAsync(message, _agentSession).ConfigureAwait(true);
        _agentTranscript.Text = transcriptSoFar + $"You: {message}\n\nAssistant: {response.Text}";
        _agentMessage.Text = string.Empty;
    }
}
