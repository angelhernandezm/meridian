// ============================================================================
// Meridian
// File: IMeridianAgentService.cs
// Author: Angel Hernandez (me@angelhernandezm.com)
// Description:
// COM-visible, dual-interface contract for multi-turn conversations with a
// named agent (one configured via options.AddAgent("name", ...) at startup).
// A session id returned by CreateSession must be passed to Run and, when the
// caller is done, to CloseSession to release it.
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

using System.Runtime.InteropServices;

namespace Meridian.Interop.Com.Interfaces;

/// <summary>
/// Multi-turn, session-based conversations with a named agent, callable from VB6/VFP/Delphi/VBA.
/// </summary>
[ComVisible(true)]
[Guid("3E8C3BBC-341A-459A-AC73-F8B093F17854")]
[InterfaceType(ComInterfaceType.InterfaceIsDual)]
public interface IMeridianAgentService {
    /// <summary>
    /// Starts a new conversation with the agent named <paramref name="agentName" /> (registered via
    /// <c>options.AddAgent(agentName, ...)</c>) and returns a session id to pass to <see cref="Run" />.
    /// </summary>
    /// <param name="agentName">The name of the registered agent to start a conversation with.</param>
    /// <returns>A session id to pass to <see cref="Run" /> and <see cref="CloseSession" />.</returns>
    string CreateSession(string agentName);

    /// <summary>
    /// Sends <paramref name="message" /> in the conversation identified by <paramref name="sessionId" /> and returns the agent's reply.
    /// </summary>
    /// <param name="sessionId">The session id returned by <see cref="CreateSession" />.</param>
    /// <param name="message">The message to send.</param>
    /// <returns>The agent's text reply.</returns>
    string Run(string sessionId, string message);

    /// <summary>
    /// Releases a session created by <see cref="CreateSession" />. Safe to call on an unknown or already-closed id.
    /// </summary>
    /// <param name="sessionId">The session id to close.</param>
    void CloseSession(string sessionId);
}
