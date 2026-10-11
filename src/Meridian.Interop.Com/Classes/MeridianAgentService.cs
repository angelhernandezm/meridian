// ============================================================================
// Meridian
// File: MeridianAgentService.cs
// Author: Angel Hernandez (me@angelhernandezm.com)
// Description:
// COM-creatable implementation of IMeridianAgentService (ProgId
// "Meridian.AgentService"). Looks agents up by name from IMeridianAgentRegistry
// and keeps their MAF AgentSession alive in a process-wide table keyed by a
// generated session id, since COM clients cannot hold a .NET object reference
// across calls the way managed code can.
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

using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using Meridian.Agents.Agents;
using Meridian.Interop.Com.Configuration;
using Meridian.Interop.Com.Interfaces;
using Meridian.Interop.Com.Support;
using Microsoft.Agents.AI;
using Microsoft.Extensions.DependencyInjection;

namespace Meridian.Interop.Com.Classes;

/// <summary>
/// Class MeridianAgentService. This class cannot be inherited.
/// Implements the <see cref="IMeridianAgentService" />
/// </summary>
/// <seealso cref="IMeridianAgentService" />
/// <inheritdoc cref="IMeridianAgentService" />
[ComVisible(true)]
[Guid("30774F81-7357-478F-A161-D040B16B72E9")]
[ClassInterface(ClassInterfaceType.None)]
[ComDefaultInterface(typeof(IMeridianAgentService))]
[ProgId("Meridian.AgentService")]
public sealed class MeridianAgentService : IMeridianAgentService {
    // Shared across every MeridianAgentService instance in the process, since each COM call may land on
    // a freshly CoCreateInstance'd object but must still find the session a previous call opened.
    /// <summary>
    /// The s sessions
    /// </summary>
    private static readonly ConcurrentDictionary<string, (AIAgent Agent, AgentSession Session)> s_sessions = new(StringComparer.Ordinal);

    /// <summary>
    /// The registry
    /// </summary>
    private readonly IMeridianAgentRegistry _registry;

    /// <summary>
    /// COM clients create this type via <c>CreateObject("Meridian.AgentService")</c>, which requires a public parameterless constructor.
    /// </summary>
    public MeridianAgentService() {
        _registry = MeridianComHost.Services.GetRequiredService<IMeridianAgentRegistry>();
    }

    /// <summary>
    /// Starts a new conversation with the agent named <paramref name="agentName" /> (registered via
    /// <c>options.AddAgent(agentName, ...)</c>) and returns a session id to pass to <see cref="Run" />.
    /// </summary>
    /// <param name="agentName">The name of the registered agent to start a conversation with.</param>
    /// <returns>A session id to pass to <see cref="Run" /> and <see cref="CloseSession" />.</returns>
    public string CreateSession(string agentName) => ComSync.Run(async () => {
        var agent = _registry.Get(agentName);
        var session = await agent.CreateSessionAsync().ConfigureAwait(false);

        var sessionId = Guid.NewGuid().ToString("n");
        s_sessions[sessionId] = (agent, session);
        return sessionId;
    });

    /// <summary>
    /// Sends <paramref name="message" /> in the conversation identified by <paramref name="sessionId" /> and returns the agent's reply.
    /// </summary>
    /// <param name="sessionId">The session id returned by <see cref="CreateSession" />.</param>
    /// <param name="message">The message to send.</param>
    /// <returns>The agent's text reply.</returns>
    public string Run(string sessionId, string message) => ComSync.Run(async () => {
        if (!s_sessions.TryGetValue(sessionId, out var entry))
            throw new InvalidOperationException($"Unknown or closed session \"{sessionId}\". Call CreateSession first.");

        var response = await entry.Agent.RunAsync(message, entry.Session).ConfigureAwait(false);
        return response.Text;
    });


    /// <summary>
    /// Releases a session created by <see cref="CreateSession" />. Safe to call on an unknown or already-closed id.
    /// </summary>
    /// <param name="sessionId">The session id to close.</param>
    public void CloseSession(string sessionId) => s_sessions.TryRemove(sessionId, out _);
}